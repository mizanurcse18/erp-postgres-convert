using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.EntityBase;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using Newtonsoft.Json;
using Security.DAL.Entities;
using Security.Manager.Dto;
using Security.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Implementations
{

    public class CommonInterfaceManager : ManagerBase, ICommonInterfaceManager
    {
        private readonly IRepository<CommonInterface> CommonInterfaceRepo;
        private readonly IRepository<CommonInterfaceFields> CommonInterfaceFieldsRepo;


        public CommonInterfaceManager(IRepository<CommonInterface> commonInterfaceRepo, IRepository<CommonInterfaceFields> commonInterfaceFieldsRepo)
        {
            CommonInterfaceRepo = commonInterfaceRepo;
            CommonInterfaceFieldsRepo = commonInterfaceFieldsRepo;
            //EntityBaseRepo = entityBaseRepo;
        }

        public GridModel GetListForGrid(GridParameter parameters)
        {
            var commonInterface = CommonInterfaceRepo.Get(parameters.MenuID);
            string filter = "";
            string where = "";
            //switch (parameters.ApprovalFilterData)
            //{
            //    case "All":
            //        filter = "";
            //        break;
            //    case "Active":
            //        filter = $@" U.IsActive=1";
            //        break;
            //    case "InActive":
            //        filter = $@" U.IsActive=0";
            //        break;

            //    default:
            //        break;
            //}
            //where = filter.IsNotNullOrEmpty() ? "WHERE " : "";
            string sql = $@"{commonInterface.GridDataSource}
							{where} {filter}
                            ";
            var result = CommonInterfaceRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public async Task<Dictionary<string, object>> LoadCommonInterfaceUIData(int MenuID)
        {
            string sql = $@"SELECT 
	                            MenuID
	                            ,TableName
	                            ,KeyFields
	                            ,InterfaceType
	                            ,GridHeaderFilter
	                            ,GridRows
                                ,GridDataSort
								,GridDataOrder
                                ,IsRowSelectAction
                                ,Tabs
                                ,ColumnsPerRow
                            FROM 
	                            CommonInterface 
                            WHERE MenuID = {MenuID}";
            return await Task.FromResult(CommonInterfaceRepo.GetData(sql));
        }

        public async Task<CommonInterface> GetCommoninterface(int MenuID)
        {
            return CommonInterfaceRepo.Get(MenuID);
        }
        public async Task<List<CommonInterfaceFields>> GetCommonInterfaceFields(int MenuID)
        {
            return CommonInterfaceFieldsRepo.GetAllList(x => x.MenuID == MenuID).ToList();
        }

        public async Task<List<Dictionary<string, object>>> LoadCommonInterfaceUIFields(int MenuID)
        {
            string sql = $@"SELECT 
	                            *
                            FROM 
	                            CommonInterfaceFields 
                            WHERE MenuID = {MenuID}
                            ORDER BY SeqNo asc
";
            return await Task.FromResult(CommonInterfaceRepo.GetDataDictCollection(sql).ToList());
        }
        public async Task<Dictionary<string, object>> GetCommonInterfaceData(string primaryKeyID, int menuID)
        {
            var commoninterface = await GetCommoninterface(menuID);
            string KeyFields = "{" + commoninterface.KeyFields + "}";
            string sql = commoninterface.GetDataGetSource.Replace($"{KeyFields}", primaryKeyID);
            return await Task.FromResult(CommonInterfaceRepo.GetData(sql));
        }
        public async Task<(bool, string)> SaveChanges(dynamic jsonData)
        {
            try
            {
                var MenuID = (int)jsonData["UIMenuID"].Value;
                var commoninterface = await GetCommoninterface(MenuID);
                var commoninterfacefields = await GetCommonInterfaceFields(MenuID);

                var assemblyInfoList = commoninterface.AssemblyInfo.IsNotNullOrEmpty() ? JsonConvert.DeserializeObject<List<AssemblyInfo>>(commoninterface.AssemblyInfo) : new List<AssemblyInfo>();

                List<object> AssemblyModeList = new List<object>();
                foreach (var assemblyInfo in assemblyInfoList)
                {
                    var PrimaryKeys = commoninterfacefields.FindAll(x => x.IsPrimaryKey);

                    if (PrimaryKeys.IsNull() || PrimaryKeys.Count == 0) return (false, $"Primary key not found,{Util.FaildToMapData}");
                    else if (PrimaryKeys.Count > 1) return (false, $"Multiple primary key assigned,{Util.FaildToMapData}");

                    var PrimaryKey = PrimaryKeys.First();

                    Type Class = Assembly.Load(assemblyInfo.AssemblyName).GetType(assemblyInfo.AssemblyWithClassName, true);
                    Object Model = Activator.CreateInstance(Class);
                    var properties = TypeDescriptor.GetProperties(Model);

                    Dictionary<string, object> list = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData.ToString());
                    foreach (var field in list)
                    {
                        PropertyDescriptor propInfo = properties.Find(field.Key, true);
                        if (propInfo.IsNull() || propInfo.IsReadOnly) continue;
                        object value = null;
                        if (field.Value == null)
                            continue;

                        if (propInfo.PropertyType.IsGenericType && propInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        {
                            var underlyingType = Nullable.GetUnderlyingType(propInfo.PropertyType);
                            value = Convert.ChangeType(field.Value, underlyingType);
                        }
                        else
                        {
                            value = Convert.ChangeType(field.Value, propInfo.PropertyType);
                        }

                        propInfo.SetValue(Model, value);
                    }
                    #region Add Or Modify Status
                    var PrimaryID = properties.Find(PrimaryKey.DataMember, false);
                    var state = properties.Find("ObjectState", true);
                    var PrimaryIDValue = PrimaryID.GetValue(Model);
                    var IsAdded = true;

                    if (Convert.ToInt32(PrimaryID.GetValue(Model)) == 0 && !PrimaryKey.IsAutoIncrement)
                    {
                        UniqueCode uniqueCode = GenerateSystemCodeWithTransaction(assemblyInfo.ClassName, "Auto");
                        PrimaryID.SetValue(Model, uniqueCode.MaxNumber);


                        if (state.IsNotNull()) state.SetValue(Model, ModelState.Added);
                        else return (false, $"{Util.FaildToMapData}");
                    }
                    else
                    {
                        if (state.IsNotNull()) { state.SetValue(Model, ModelState.Modified); IsAdded = false; }
                        else return (false, $"{Util.FaildToMapData}");
                    }
                    #endregion Add Or Modify Status

                    var existingModel = new Dictionary<string, object>();
                    existingModel = await GetCommonInterfaceData(PrimaryIDValue.ToString(), MenuID);

                    using (var unitOfWork = new UnitOfWork())
                    {
                        var dbs = AppContexts.GetActiveDbContexts<BaseDbContext>();
                        //var context = dbs.Find(x => x.Value() == "SecurityDbContext");
                        var updateModel = (EntityBase)Model;
                        
                        if (IsAdded.IsFalse())
                        {                            
                            SetExistingModelAuditFields(updateModel, existingModel);
                        }
                        SetAuditFields(updateModel);
                        if (IsAdded)
                            dbs[0].Add(updateModel);
                        else dbs[0].Update(updateModel);
                        //CommonInterfaceRepo.Add(updateModel);
                        //EntityBaseRepo.Add(updateModel);
                        unitOfWork.CommitChangesWithAudit();
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            await Task.CompletedTask;
            return (true, $"{Util.SaveSuccessfullyMessage}");
        }

        public async Task<(bool, string)> SaveChangesV2(dynamic jsonData)
        {
            try
            {
                var MenuID = (int)jsonData["MenuID"].Value;
                var commoninterface = await GetCommoninterface(MenuID);
                var commoninterfacefields = await GetCommonInterfaceFields(MenuID);

                Assembly objAssembly = Assembly.Load("Security.DAL");
                Type elementType = objAssembly.GetType("Security.DAL.Entities.Division", true);

                Object Model = Activator.CreateInstance(elementType);
                var properties = TypeDescriptor.GetProperties(Model);
                Dictionary<string, object> list = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData.ToString());
                foreach (var field in list)
                {
                    PropertyDescriptor propInfo = properties.Find(field.Key, true);
                    if (propInfo.IsNull() || propInfo.IsReadOnly) continue;
                    object value = null;
                    if (field.Value == null)
                        continue;

                    if (propInfo.PropertyType.IsGenericType && propInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        var underlyingType = Nullable.GetUnderlyingType(propInfo.PropertyType);
                        value = Convert.ChangeType(field.Value, underlyingType);
                    }
                    else
                    {
                        value = Convert.ChangeType(field.Value, propInfo.PropertyType);
                    }

                    propInfo.SetValue(Model, value);
                }
                var state = properties.Find("ObjectState", true);
                if (state.IsNotNull()) state.SetValue(Model, ModelState.Added);
                else return (false, $"Data Failed Successfully"); ;
                //else state.SetValue(Model, ModelState.Added);
                using (var unitOfWork = new UnitOfWork())
                {
                    var dbs = AppContexts.GetActiveDbContexts<BaseDbContext>();
                    //var context = dbs.Find(x => x.Value() == "SecurityDbContext");
                    var updateModel = (EntityBase)Model;
                    SetAuditFields(updateModel);

                    dbs[0].Add(updateModel);
                    //CommonInterfaceRepo.Add(updateModel);
                    //EntityBaseRepo.Add(updateModel);
                    unitOfWork.CommitChangesWithAudit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            await Task.CompletedTask;
            return (true, $"Information Submitted Successfully");
        }
        public async Task<(bool, string)> SaveChangesV1(dynamic jsonData)
        {
            try
            {
                var MenuID = (int)jsonData["MenuID"].Value;
                var commoninterface = await GetCommoninterface(MenuID);
                var commoninterfacefields = await GetCommonInterfaceFields(MenuID);

                Assembly objAssembly = Assembly.Load("Security.DAL");
                Type elementType = objAssembly.GetType("Security.DAL.Entities.User", true);
                //Type listType = typeof(List<>).MakeGenericType(new Type[] { elementType });
                Object Model = Activator.CreateInstance(elementType);
                var properties = TypeDescriptor.GetProperties(Model);
                Dictionary<string, object> list = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData.ToString());
                foreach (var field in list)
                {
                    PropertyDescriptor propInfo = properties.Find(field.Key, true);
                    if (propInfo.IsNull() || propInfo.IsReadOnly) continue;
                    //var value = field.Value.MapField(propInfo.PropertyType);
                    object value = null;
                    //if(propInfo.PropertyType == typeof(Nullable<>))
                    //{

                    //}
                    if (propInfo.PropertyType == typeof(DateTime?))
                    {                        
                        value = field.Value.ToString().IsNotNullOrEmpty() ? (DateTime?)Convert.ToDateTime(field.Value) : null;
                    }
                    else if (propInfo.PropertyType == typeof(Int32?))
                    {                        
                        value = field.Value.ToString().IsNotNullOrEmpty() ? Convert.ToInt32(field.Value) : (int?)null;
                    }
                    else if (propInfo.PropertyType == typeof(Int64?))
                    {
                        value = field.Value.ToString().IsNotNullOrEmpty() ? Convert.ToInt64(field.Value) : (long?)null;
                    }
                    else if (propInfo.PropertyType == typeof(float?) || propInfo.PropertyType == typeof(long?))
                    {
                        value = field.Value.ToString().IsNotNullOrEmpty() ? Convert.ToDouble(field.Value) : (double?)null;
                    }
                    else
                    {
                        value = Convert.ChangeType(field.Value, propInfo.PropertyType);
                    }

                    propInfo.SetValue(Model, value);
                }
                var state = properties.Find("State", true);
                if (state.IsNotNull()) state.SetValue(Model, ModelState.Unchanged);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


            await Task.CompletedTask;
            return (true, $"Information Submitted Successfully");
        }

        public async Task<(bool, string)> SaveChangesExample<T>(dynamic jsonData)
        {
            try
            {
                var MenuID = (int)jsonData["MenuID"].Value;
                var commoninterface = await GetCommoninterface(MenuID);
                var commoninterfacefields = await GetCommonInterfaceFields(MenuID);
                T Model = JsonConvert.DeserializeObject<T>(jsonData.ToString());

                foreach (var prop in typeof(T).GetProperties())
                {
                    if (prop.IsDefined(typeof(JsonIgnoreAttribute), true)) continue;

                    var jsonPropName = prop.Name;
                    if (prop.IsDefined(typeof(JsonPropertyAttribute), true))
                    {
                        jsonPropName = prop.GetCustomAttribute<JsonPropertyAttribute>().PropertyName;
                    }

                    var jsonValue = jsonData[jsonPropName];
                    if (jsonValue == null) continue;

                    object value = jsonValue;
                    Type propertyType = prop.PropertyType;
                    if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        propertyType = Nullable.GetUnderlyingType(propertyType);
                    }
                    switch (propertyType.Name)
                    {
                        case "DateTime":
                            value = Convert.ToDateTime(jsonValue);
                            break;
                        case "Int32":
                            value = Convert.ToInt32(jsonValue);
                            break;
                        case "Int64":
                            value = Convert.ToInt64(jsonValue);
                            break;
                        case "float":
                            value = Convert.ToSingle(jsonValue);
                            break;
                        case "long":
                            value = Convert.ToInt64(jsonValue);
                            break;
                        default:
                            value = Convert.ChangeType(jsonValue, propertyType);
                            break;
                    }
                    prop.SetValue(Model, value);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            await Task.CompletedTask;
            return (true, $"Information Submitted Successfully");
        }

    }
}
