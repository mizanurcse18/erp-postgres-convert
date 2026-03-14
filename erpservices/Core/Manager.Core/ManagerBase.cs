using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Core;

using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.EntityBase;
using Manager.Core.CommonDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Security.Manager.Dto;
using Microsoft.Extensions.Caching.Memory;
using Manager.Core.Caching;
using System.Collections.Concurrent;
using Core; // Add this using for GridParameter

namespace Manager.Core
{
    public class ManagerBase : IManager
    {
        private int acccode;

        public ManagerBase()
        {
            AppContexts.Resolve<DbUtility>();
        }

        public void EmailCalucation()
        {
            /// todo 
            /// 
            if (acccode == 100)
            {
                logoic();
            }
            else if (acccode == 200)
            {
                logic();
            }
        }

        private void logic()
        {
            throw new NotImplementedException();
        }

        private void logoic()
        {
            throw new NotImplementedException();
        }

        public UniqueCode GenerateSystemCodeWithTransaction(string tableName, string companyId, short addNumber = 1, string prefix = "",
            string suffix = "")
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var result = context.ExecuteScalarWithTransaction("MakeSystemCode @0,@1,@2,@3,@4,@5,@6", tableName, companyId,
                DateTime.Today.ToString(Util.DateFormat), addNumber, DateTime.Now, prefix, suffix);

            var uniqueCode = new UniqueCode();
            if (result.IsNull()) return uniqueCode;

            var dataArray = result.ToString().Split('%');
            uniqueCode.MaxNumber = Convert.ToInt32(dataArray[0]);
            if (dataArray.Length > 1) uniqueCode.SystemId = dataArray[1];
            if (dataArray.Length > 2) uniqueCode.SystemCode = dataArray[2];

            return uniqueCode;
        }
        public UniqueCode GenerateSystemCode(string tableName, string companyId, short addNumber = 1, string prefix = "",
            string suffix = "")
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));


            var result = context.ExecuteScalar("SELECT make_system_code(@0::varchar, @1::varchar, @2::varchar, @3::smallint, @4::timestamp, @5::varchar, @6::varchar)", tableName, companyId,
                DateTime.Today.ToString("yyyy-MM-dd"), addNumber, DateTime.UtcNow, prefix, suffix);

            var uniqueCode = new UniqueCode();
            if (result.IsNull()) return uniqueCode;

            var dataArray = result.ToString().Split('%');
            uniqueCode.MaxNumber = Convert.ToInt32(dataArray[0]);
            if (dataArray.Length > 1) uniqueCode.SystemId = dataArray[1];
            if (dataArray.Length > 2) uniqueCode.SystemCode = dataArray[2];

            return uniqueCode;
        }

        public Task<List<Dictionary<string, object>>> GetApiPath(string apipath, int UserID)
        {
            string commentSql = @$"SELECT * FROM ViewGetAllMenuPermissionWithApiPath where UserID={UserID} AND '/'+Controller+'/'+ApiPath ='{apipath}'";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var list = context.GetDataDictCollection(commentSql).ToList();
            return Task.FromResult(list);
        }

        public Task<List<Dictionary<string, object>>> GetBlackListToken(string token, int UserID)
        {
            string commentSql = @$"SELECT * FROM UserTokenBlackList where user_id={UserID} AND Token='{token}'";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var list = context.GetDataDictCollection(commentSql).ToList();
            return Task.FromResult(list);
        }

        public bool CheckChangeValidUser(int UserID)
        {
            string commentSql = @$"SELECT * FROM users where user_id={UserID} AND is_active = TRUE AND is_locked = FALSE";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var list = context.GetDataDictCollection(commentSql).ToList();
            return list.Count > 0;
        }

        public static void SetAuditFields(params IEnumerable<EntityBase>[] modelCollArray)
        {
            try
            {
                var dateTime = DateTime.UtcNow;
                foreach (var baseModel in modelCollArray.SelectMany(modelCollection => modelCollection))
                {
                    SetAuditFields(baseModel, dateTime);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void SetAuditFieldsOracle(params IEnumerable<EntityBase>[] modelCollArray)
        {
            try
            {
                var dateTime = DateTime.Now;
                foreach (var baseModel in modelCollArray.SelectMany(modelCollection => modelCollection))
                {
                    SetAuditFieldsOracle(baseModel, dateTime);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void SetAuditFields(EntityBase model)
        {
            SetAuditFields(model, DateTime.UtcNow);
        }
        public static void SetAuditFieldsOracle(EntityBase model)
        {
            SetAuditFieldsOracle(model, DateTime.UtcNow);
        }
        public static void SetAuditFields(EntityBase model, DateTime dateTime)
        {
            try
            {
                var properties = TypeDescriptor.GetProperties(model);
                if (model.IsAdded)
                {
                    var fields = new Dictionary<string, object>
                    {
                        {"CreatedBy", AppContexts.User.UserID},
                        {"CreatedDate", dateTime},
                        {"CreatedIP",AppContexts.GetIPAddress() },
                        {"CompanyID", AppContexts.User.CompanyID}
                    };

                    SetAuditFields(model, properties, fields);
                }
                else if (model.IsModified)
                {
                    var fields = new Dictionary<string, object>
                    {
                        {"UpdatedBy", AppContexts.User.UserID},
                        {"UpdatedDate", dateTime},
                        { "UpdatedIP", AppContexts.GetIPAddress() },
                        {"CompanyID", AppContexts.User.CompanyID}
                };

                    SetAuditFields(model, properties, fields);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void SetAuditFieldsOracle(EntityBase model, DateTime dateTime)
        {
            try
            {
                var properties = TypeDescriptor.GetProperties(model);
                if (model.IsAdded)
                {
                    var fields = new Dictionary<string, object>
                    {
                        {"CREATED_BY", AppContexts.User.UserID},
                        {"CREATED_DATE", dateTime},
                        {"CREATED_IP",AppContexts.GetIPAddress() },
                        {"COMPANY_ID", AppContexts.User.CompanyID}
                    };

                    SetAuditFields(model, properties, fields);
                }
                else if (model.IsModified)
                {
                    var fields = new Dictionary<string, object>
                    {
                        {"UPDATED_BY", AppContexts.User.UserID},
                        {"UPDATED_DATE", dateTime},
                        { "UPDATED_IP", AppContexts.GetIPAddress() },
                        {"COMPANY_ID", AppContexts.User.CompanyID}
                };

                    SetAuditFields(model, properties, fields);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void SetAuditFields(EntityBase model, PropertyDescriptorCollection properties, Dictionary<string, object> fields)
        {
            try
            {
                foreach (var field in fields)
                {
                    var propInfo = properties.Find(field.Key, true);
                    if (propInfo.IsNull()) continue;
                    propInfo.SetValue(model, field.Value);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public static void SetExistingModelAuditFields(EntityBase model, Dictionary<string, object> existingObject)
        {
            try
            {
                var properties = TypeDescriptor.GetProperties(model);
                if (model.IsModified)
                {
                    var fields = new Dictionary<string, object>
                    {
                        {"CreatedBy", existingObject.ContainsKey("CreatedBy") ? existingObject["CreatedBy"] :0},
                        {"CreatedDate",existingObject.ContainsKey("CreatedDate") ? existingObject["CreatedDate"]:DateTime.Now},
                        {"CreatedIP",existingObject.ContainsKey("CreatedIP") ? existingObject["CreatedIP"] :""},
                        {"RowVersion", existingObject.ContainsKey("RowVersion") ? existingObject["RowVersion"] : 0}
                    };

                    SetAuditFields(model, properties, fields);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        public static Widget GetWidgetFormattedData<T>(string widgeId, string widgeTitle, List<T> dataList,
            List<(string, string)> columnHeaderDef, List<(string, string, string, string)> cellBodyDef, string rowKey = "")
        {

            var data = dataList.Cast<T>().ToList();
            var rows = new List<Rows>();
            if (data.Count > 0)
            {

                if (data.IsNotNull())
                {
                    foreach (var (value, index) in data.Select((v, i) => (v, i)))
                    {
                        rows.Add(new Rows
                        {
                            id = !string.IsNullOrWhiteSpace(rowKey) ? data[index].GetType().GetProperty(rowKey).GetValue(data[index], null).ToString() : (index + 1).ToString(),
                            cells = cellBodyDef.SelectMany(x => new List<Cells>
                            {
                                new Cells
                                {
                                    id=x.Item1,
                                    value=data[index].GetType().GetProperty(x.Item2).GetValue(data[index],null).ToString(),
                                    classes=!string.IsNullOrWhiteSpace(x.Item3)?
                                    (x.Item3.Split(':')[0]=="Conditional"?
                                    data[index].GetType().GetProperty(x.Item3.Split(':')[1]).GetValue(data[index],null).ToString():x.Item3)
                                    :"",
                                    icon=x.Item4
                                }
                            }).ToList()
                        });
                    }
                }
            }
            Widget widget = new Widget
            {
                id = widgeId,
                title = widgeTitle,
                table = new CommonDto.Table
                {
                    columns = columnHeaderDef.SelectMany(x => new List<Columns> {
                        new Columns { id = x.Item1, title = x.Item2 } }).ToList(),
                    rows = rows
                },
                //FromDate =  DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd"),
                //ToDate =  DateTime.Now.ToString("yyyy-MM-dd")

                FromDate = rows.Count > 0 ? rows[rows.Count - 1].cells[1].value.ToDate().ToString("yyyy-MM-dd") : "",
                ToDate = rows.Count > 0 ? rows[0].cells[1].value.ToDate().ToString("yyyy-MM-dd") : ""

                //FromDate = rows.Count > 0 ? rows[rows.Count - 1].cells[1].value : "",
                //ToDate = rows.Count > 0 ? rows[0].cells[1].value : ""

            };

            return widget;
        }


        public static string GetDateDifferenceYearMonthDay(DateTime fromDate, DateTime toDate)
        {
            if (toDate > fromDate)
            {
                DateTime zeroTime = new DateTime(1, 1, 1);
                TimeSpan span = toDate - fromDate;

                int years = (zeroTime + span).Year - 1;
                int months = (zeroTime + span).Month - 1;
                int days = (zeroTime + span).Day;
                return years +
                         " year" +
                         (years == 1 ? " " : "s ") +
                         months +
                         " month" +
                         (months == 1 ? " " : "s ") +
                         "and " +
                         days +
                         " day" +
                         (days == 1 ? "" : "s");
            }
            return "Invalid Duration";
        }

        public static string CreateApprovalProcessForLeaveApplication(int ReferenceID, string ApprovalProcessDescription,
            string ApprovalProcessTitle, int EmployeeID, int LeaveCategoryID, bool IsLFA, bool IsFestival, decimal Days)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..CreateApprovalProcessForLeaveApplicationWithSupervisorHierarchy @0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10", ReferenceID,
                ApprovalProcessDescription, ApprovalProcessTitle, AppContexts.User.CompanyID, AppContexts.User.UserID,
                AppContexts.User.IPAddress, EmployeeID, LeaveCategoryID, IsLFA, IsFestival, Days);

            return result.ToString();
        }

        public static async Task<ApprovalResult> CreateApprovalProcessForLeaveApplicationNew(int ReferenceID, string ApprovalProcessDescription,
            string ApprovalProcessTitle, int EmployeeID, int LeaveCategoryID, bool IsLFA, bool IsFestival, decimal Days)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            ApprovalResult obj = new ApprovalResult();
            var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..CreateApprovalProcessForLeaveApplicationWithSupervisorHierarchyNew @0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10", ReferenceID,
                ApprovalProcessDescription, ApprovalProcessTitle, AppContexts.User.CompanyID, AppContexts.User.UserID,
                AppContexts.User.IPAddress, EmployeeID, LeaveCategoryID, IsLFA, IsFestival, Days);

            if (!string.IsNullOrEmpty(result.ToString()))
            {
                var parts = result.ToString().Split('$');
                if (parts.Length == 2)
                {

                    obj.ApprovalProcessID = parts[0].ToString();
                    obj.EmployeeIDs = parts[1].ToString();


                }
            }

            // Return null or throw an exception if parsing fails
            return obj;
        }
        public static string CreateApprovalProcessForLeaveEncashmentWithSupervisorHierarchy(int ReferenceID, string ApprovalProcessDescription,
            string ApprovalProcessTitle, int EmployeeID)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..CreateApprovalProcessForLeaveEncashmentWithSupervisorHierarchy @0,@1,@2,@3,@4,@5,@6", ReferenceID,
                ApprovalProcessDescription, ApprovalProcessTitle, AppContexts.User.CompanyID, AppContexts.User.UserID,
                AppContexts.User.IPAddress, EmployeeID);

            return result.ToString();
        }
        public static string CreateApprovalProcessForLeaveApplication(int ReferenceID, string ApprovalProcessDescription,
           string ApprovalProcessTitle, int EmployeeID, string LeaveTypeTitle, decimal Days)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..CreateApprovalProcessForLeaveEncashmentWithSupervisorHierarchy @0,@1,@2,@3,@4,@5,@6,@7,@8", ReferenceID,
                ApprovalProcessDescription, ApprovalProcessTitle, AppContexts.User.CompanyID, AppContexts.User.UserID,
                AppContexts.User.IPAddress, EmployeeID, LeaveTypeTitle, Days);

            return result.ToString();
        }
        public static string CreateApprovalProcess(int ReferenceID, string ApprovalProcessDescription,
           string ApprovalProcessTitle, int EmployeeID, int APTypeID, int APPanelID)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..CreateApprovalProcess @0,@1,@2,@3,@4,@5,@6,@7,@8", ReferenceID,
                ApprovalProcessDescription, ApprovalProcessTitle, AppContexts.User.CompanyID, AppContexts.User.UserID,
                AppContexts.User.IPAddress, EmployeeID, APTypeID, APPanelID);

            return result.ToString();
        }
        public static string CreateApprovalProcessByAPTypeIDAndAPPanelID(int ReferenceID, string ApprovalProcessDescription,
           string ApprovalProcessTitle, int EmployeeID, int APTypeID, int APPanelID)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..CreateApprovalProcessByAPTypeIDAndAPPanelID @0,@1,@2,@3,@4,@5,@6,@7,@8", ReferenceID,
                ApprovalProcessDescription, ApprovalProcessTitle, AppContexts.User.CompanyID, AppContexts.User.UserID,
                AppContexts.User.IPAddress, EmployeeID, APTypeID, APPanelID);

            return result.ToString();
        }
        public static string CreateApprovalProcessParallel(int ReferenceID, string ApprovalProcessDescription,
           string ApprovalProcessTitle, int EmployeeID, int APTypeID, int APPanelID)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..CreateApprovalProcessParallel @0,@1,@2,@3,@4,@5,@6,@7,@8", ReferenceID,
                ApprovalProcessDescription, ApprovalProcessTitle, AppContexts.User.CompanyID, AppContexts.User.UserID,
                AppContexts.User.IPAddress, EmployeeID, APTypeID, APPanelID);

            return result.ToString();
        }
        public static string CreateApprovalProcessForEmployeeProfileUpdate(int ReferenceID, string ApprovalProcessDescription,
           string ApprovalProcessTitle, int EmployeeID, int APTypeID, int APPanelID)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..CreateApprovalProcessForEmployeeProfileUpdate @0,@1,@2,@3,@4,@5,@6,@7,@8", ReferenceID,
                ApprovalProcessDescription, ApprovalProcessTitle, AppContexts.User.CompanyID, AppContexts.User.UserID,
                AppContexts.User.IPAddress, EmployeeID, APTypeID, APPanelID);

            return result.ToString();
        }

        public static ApprovalProcessReturnIDs CreateApprovalProcessForLimit(int ReferenceID, string ApprovalProcessDescription,
           string ApprovalProcessTitle, int EmployeeID, int APTypeID, decimal TotalAmount, string Months, bool IsException = false)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..CreateApprovalProcessForLimit @0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10", ReferenceID,
                ApprovalProcessDescription, ApprovalProcessTitle, AppContexts.User.CompanyID, AppContexts.User.UserID,
                AppContexts.User.IPAddress, EmployeeID, APTypeID, TotalAmount, Months, IsException);
            var dataArray = result.ToString().Split('%');
            var approvalProcessIDs = new ApprovalProcessReturnIDs();
            if (dataArray.Length > 0) approvalProcessIDs.ApprovalProcessID = dataArray[0];
            if (dataArray.Length > 1) approvalProcessIDs.IsAutoApproved = dataArray[1].ToInt() == 0 ? false : true;

            return approvalProcessIDs;
        }
        public static ApprovalProcessReturnIDs CreateApprovalProcessForLimitWithConfig(int ReferenceID, string ApprovalProcessDescription,
       string ApprovalProcessTitle, int EmployeeID, int APTypeID, decimal TotalAmount, string Months, bool IsException = false)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..CreateApprovalProcessForLimitWithConfig @0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10", ReferenceID,
                ApprovalProcessDescription, ApprovalProcessTitle, AppContexts.User.CompanyID, AppContexts.User.UserID,
                AppContexts.User.IPAddress, EmployeeID, APTypeID, TotalAmount, Months, IsException);
            var dataArray = result.ToString().Split('%');
            var approvalProcessIDs = new ApprovalProcessReturnIDs();
            if (dataArray.Length > 0) approvalProcessIDs.ApprovalProcessID = dataArray[0];
            if (dataArray.Length > 1) approvalProcessIDs.IsAutoApproved = dataArray[1].ToInt() == 0 ? false : true;

            return approvalProcessIDs;
        }

        public static ApprovalProcessReturnIDs CreateManualApprovalProcess(int ReferenceID, string ApprovalProcessDescription,
       string ApprovalProcessTitle, int EmployeeID, int APTypeID, int APPanelID, DbUtility context, string dbName)
        {
            //var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            //var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..CreateManualApprovalProcess @0,@1,@2,@3,@4,@5,@6,@7", ReferenceID,
            //    ApprovalProcessDescription, ApprovalProcessTitle, AppContexts.User.CompanyID, AppContexts.User.UserID,
            //    AppContexts.User.IPAddress, EmployeeID, APTypeID);
            var result = context.GetData(@$"EXEC {dbName}..CreateManualApprovalProcess  {ReferenceID},'{ApprovalProcessDescription}','{ApprovalProcessTitle}','{AppContexts.User.CompanyID}',{AppContexts.User.UserID},'{AppContexts.User.IPAddress}',{EmployeeID},{APTypeID},{APPanelID}");

            //var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..CreateManualApprovalProcess @0,@1,@2,@3,@4,@5,@6,@7,@8", {ReferenceID},'{ApprovalProcessDescription}','{ApprovalProcessTitle}','{AppContexts.User.CompanyID}',{AppContexts.User.UserID},'{AppContexts.User.IPAddress}',{EmployeeID},{APTypeID},{APPanelID}");



            var dataArray = result["ApprovalProcessID"].ToString().Split('%');
            var approvalProcessIDs = new ApprovalProcessReturnIDs();
            if (dataArray.Length > 0) approvalProcessIDs.ApprovalProcessID = dataArray[0];
            if (dataArray.Length > 1) approvalProcessIDs.IsAutoApproved = dataArray[1].ToInt() == 0 ? false : true;

            return approvalProcessIDs;
        }

        public static string CreateApprovalProcessForDocumentApprovalHR(int ReferenceID, string ApprovalProcessDescription,
           string ApprovalProcessTitle, int EmployeeID, int APTypeID, int APPanelID, DbUtility context, string dbName)
        {
            //var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.Default)}..CreateApprovalProcessForEmployeeProfileUpdate @0,@1,@2,@3,@4,@5,@6,@7,@8", ReferenceID,
                ApprovalProcessDescription, ApprovalProcessTitle, AppContexts.User.CompanyID, AppContexts.User.UserID,
                AppContexts.User.IPAddress, EmployeeID, APTypeID, APPanelID);

            return result.ToString();
        }


        public static string SaveManualApprovalPanel(int EmployeeID, int APPanelID, decimal SequenceNo, int ProxyEmployeeID, bool IsProxyEmployeeEnabled,
            int NFAApprovalSequenceType, bool IsEditable, bool IsSCM, bool IsMultiProxy, int APTypeID, int ReferenceID, string dbName)
        {

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{dbName}..spInsertManualApprovalPanelEmployee @0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13",
                EmployeeID, APPanelID, SequenceNo, ProxyEmployeeID, IsProxyEmployeeEnabled, NFAApprovalSequenceType, IsEditable, IsSCM, IsMultiProxy, APTypeID, ReferenceID, AppContexts.User.CompanyID, AppContexts.User.UserID,
                AppContexts.User.IPAddress);
            return result.ToString();
        }
        public static string DeleteManualApprovalPanel(int ReferenceID, string dbName, int APTypeID)
        {

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{dbName}..spDeleteManualApprovalPanelEmployeeWithProxy @0,@1", ReferenceID, APTypeID);
            return result.ToString();
        }
        public static string SaveManualApprovalPanelMultiProxy(int MAPPanelEmployeeID, int APPanelID, int DivisionID, int DepartmentID, int EmployeeID, string dbName)
        {

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{dbName}..spInsertManualApprovalPanelProxyEmployee @0,@1,@2,@3,@4,@5,@6,@7",
                MAPPanelEmployeeID, APPanelID, DivisionID, DepartmentID, EmployeeID, AppContexts.User.CompanyID, AppContexts.User.UserID,
                AppContexts.User.IPAddress);
            return result.ToString();
        }


        public static void UpdateApprovalStatusForAutoApproved(int APTypeID, int ReferenceID)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var sql = $@"EXEC {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..UpdateApprovalStatusForAutoApproved {APTypeID}, {ReferenceID}, '{AppContexts.User.CompanyID}', {AppContexts.User.UserID},
                '{AppContexts.User.IPAddress}'";
            var result = context.GetData(sql);
        }
        public static string UpdateApprovalProcessTitle(int ApprovalProcessID, string Title)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"UPDATE Approval..ApprovalProcess        
                                                    SET Title='{Title}',UpdatedBy={AppContexts.User.UserID},UpdatedDate=GETDATE(),UpdatedIP='{AppContexts.User.IPAddress}'
                                                WHERE ApprovalProcessID = {ApprovalProcessID}");

            return result.IsNotNull() ? result.ToString() : "";
        }
        public static void DeleteAllApprovalProcessRelatedData(int APTypeID, int ReferenceID)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..DeleteAllApprovalProcessRelatedData @0,@1", APTypeID, ReferenceID);
        }
        public static string UpdateLeaveBalanceAfterSubmit(int EmployeeID)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var sql = $@"EXEC {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..UpdateLeaveBalanceAfterSubmit {EmployeeID}";
            var result = context.GetData(sql);

            return result.IsNotNull() ? result.ToString() : "";
        }
        public static string UpdateAttendanceSummaryTable(int EmployeeID, DateTime StartDate, DateTime EndDate)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var sql = $@"EXEC {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..CursorForAttendanceScheduleByEmployeeIDAndDate {EmployeeID},'{StartDate}','{EndDate}'";
            var result = context.GetData(sql);

            return result.IsNotNull() ? result.ToString() : "";
        }

        public static string UpdateApprovalProcessFeedback(int ApprovalProcessID, int APEmployeeFeedbackID, int APFeedbackID, string Remarks, int APTypeID,
            int ReferenceID, int ToAPMemberFeedbackID = 0, bool IsEditable = false, bool IsForceMDToPanel = false, bool AlreadyAddedMD = false)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var result = context.ExecuteScalar(@$"Approval..spUpdateApprovalProcessFeedback @0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13", ApprovalProcessID,
            APEmployeeFeedbackID, APFeedbackID, Remarks.IsNull() ? "" : Remarks, APTypeID, ReferenceID, AppContexts.User.EmployeeID, AppContexts.User.CompanyID, AppContexts.User.UserID,
            AppContexts.User.IPAddress, ToAPMemberFeedbackID, IsEditable, IsForceMDToPanel, AlreadyAddedMD);
            return result.ToString();
        }

        //public static string UpdateApprovalProcessFeedback(int ApprovalProcessID, int APEmployeeFeedbackID, int APFeedbackID, string Remarks, int APTypeID,
        //    int ReferenceID, int ToAPMemberFeedbackID = 0
        //    )
        //{
        //    var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

        //    var result = context.ExecuteScalar(@$"Approval..spUpdateApprovalProcessFeedback @0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10", ApprovalProcessID,
        //        APEmployeeFeedbackID, APFeedbackID, Remarks.IsNull() ? "" : Remarks, APTypeID, ReferenceID, AppContexts.User.EmployeeID, AppContexts.User.CompanyID, AppContexts.User.UserID,
        //        AppContexts.User.IPAddress, ToAPMemberFeedbackID);

        //    return result.ToString();
        //}
        public static string UpdateApprovalProcessFeedbackForParallel(int ApprovalProcessID, int APEmployeeFeedbackID, int APFeedbackID, string Remarks, int APTypeID,
            int ReferenceID, int ToAPMemberFeedbackID = 0, bool IsCreator = false
            )
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"Approval..spUpdateApprovalProcessFeedbackForParallel @0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11", ApprovalProcessID,
                APEmployeeFeedbackID, APFeedbackID, Remarks.IsNull() ? "" : Remarks, APTypeID, ReferenceID, AppContexts.User.EmployeeID, AppContexts.User.CompanyID, AppContexts.User.UserID,
                AppContexts.User.IPAddress, ToAPMemberFeedbackID, IsCreator);

            return result.ToString();
        }

        public static string SubmitComment(int APForwardInfoID, int APEmployeeFeedbackID, string Comment
            )
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"Approval..spSubmitComment @0,@1,@2,@3,@4",
                APForwardInfoID, APEmployeeFeedbackID, Comment, AppContexts.User.UserID, AppContexts.User.IPAddress);

            return result.IsNotNull() ? result.ToString() : "";
        }
        public static string SubmitBudgetPlanRemarks(int NFAID, string BudgetPlanRemarks, int BudgetPlanCategoryID
           )
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"SECURITY..SubmitBudgetPlanRemarks @0,@1,@2,@3,@4",
                NFAID, BudgetPlanRemarks, BudgetPlanCategoryID, AppContexts.User.UserID, AppContexts.User.IPAddress);

            return result.IsNotNull() ? result.ToString() : "";
        }


        public static string SubmitAdminRemarks(int RSMID, string AdminRemarks)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"HRMS..SPSubmitAdminRemarks @0,@1,@2,@3",
                RSMID, AdminRemarks, AppContexts.User.UserID, AppContexts.User.IPAddress);

            return result.IsNotNull() ? result.ToString() : "";
        }

        public static string SubmitITRecommendation(int SRMID, string ITRecommendation)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"HRMS..SPSubmitITRecommendation @0,@1,@2,@3",
                SRMID, ITRecommendation, AppContexts.User.UserID, AppContexts.User.IPAddress);

            return result.IsNotNull() ? result.ToString() : "";
        }
        public static string UpdateVehicleInfo(int RSMID, DateTime FromDateChange, DateTime ToDateChange, string InTime, string OutTime, string Duration, int TransportTypeID, decimal TransportQuantity, bool IsOthers, string Vehicle, string Driver, string ContactNumber)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"HRMS..SPUpdateSupportVehicleInfo @0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13",
                RSMID, FromDateChange, ToDateChange, InTime, OutTime, Duration, TransportTypeID, TransportQuantity, IsOthers, Vehicle, Driver, ContactNumber, AppContexts.User.UserID, AppContexts.User.IPAddress);

            return result.IsNotNull() ? result.ToString() : "";
        }
        public static string UpdateFacilitiesDetailsInfo(int RSMID, DateTime NeededByDateChng)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"HRMS..SPUpdateFacilitiesDetailsInfo @0,@1,@2,@3",
                RSMID, NeededByDateChng, AppContexts.User.UserID, AppContexts.User.IPAddress);

            return result.IsNotNull() ? result.ToString() : "";
        }

        public static string UpdateItemQuantity(int RSIDID, decimal Quantity, int ItemId)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var result = context.ExecuteScalar(@$"HRMS..UpdateItemQuantity @0,@1,@2,@3,@4",
            RSIDID, Quantity, AppContexts.User.UserID, AppContexts.User.IPAddress, ItemId);

            return result.IsNotNull() ? result.ToString() : "";
        }

        public static string SubmitSCMBugetPlanRemarks(int APTypeID, int MasterID, string BudgetPlanRemarks = "", string SCMRemarks = "", bool IsSingleQuotation = false
           )
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"SCM..SubmitBudgetPlanRemarks @0,@1,@2,@3,@4,@5,@6",
            APTypeID, MasterID, BudgetPlanRemarks.IsNullOrEmpty() ? "" : BudgetPlanRemarks, SCMRemarks.IsNullOrEmpty() ? "" : SCMRemarks, IsSingleQuotation, AppContexts.User.UserID, AppContexts.User.IPAddress);

            return result.IsNotNull() ? result.ToString() : "";
        }
        public static string SubmitSCMBugetPlanRemarksPR(int APTypeID, int MasterID, string BudgetPlanRemarks = "", int BudgetPlanCategoryID = 0, string SCMRemarks = "", bool IsSingleQuotation = false
           )
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"SCM..SubmitBudgetPlanRemarksForPR @0,@1,@2,@3,@4,@5,@6,@7",
            APTypeID, MasterID, BudgetPlanRemarks.IsNullOrEmpty() ? "" : BudgetPlanRemarks, BudgetPlanCategoryID, SCMRemarks.IsNullOrEmpty() ? "" : SCMRemarks, IsSingleQuotation, AppContexts.User.UserID, AppContexts.User.IPAddress);

            return result.IsNotNull() ? result.ToString() : "";
        }
        public static string SubmitSCCUserData(int APTypeID, int MasterID, DateTime? ServicePeriodFrom = null, DateTime? ServicePeriodTo = null
            , int PaymentType = (int)Util.SCCPaymentType.FullPayment, int PaymentFixedOrPercent = 2, decimal PaymentFixedOrPercentAmount = 0
            , decimal PaymentFixedOrPercentTotalAmount = 0, decimal SCCAmount = 0, int Lifecycle = 0, string LifecycleComment = "", bool PerformanceAssessment1 = false, bool PerformanceAssessment2 = false, bool PerformanceAssessment3 = false, bool PerformanceAssessment4 = false, bool PerformanceAssessment5 = false, bool PerformanceAssessment6 = false, string PerformanceAssessmentComment = "", decimal TotalReceivedQty = 0)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"SCM..SP_UpdateSCCMasterWithUserData @0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16,@17,@18,@19,@20",
            APTypeID, MasterID
            , ServicePeriodFrom.IsNull() ? null : ServicePeriodFrom
            , ServicePeriodTo.IsNull() ? null : ServicePeriodTo
            , PaymentType, PaymentFixedOrPercent, PaymentFixedOrPercentAmount
            , PaymentFixedOrPercentTotalAmount, SCCAmount, Lifecycle, LifecycleComment.IsNullOrEmpty() ? "" : LifecycleComment
            , PerformanceAssessment1, PerformanceAssessment2, PerformanceAssessment3, PerformanceAssessment4, PerformanceAssessment5, PerformanceAssessment6, PerformanceAssessmentComment, TotalReceivedQty
            , AppContexts.User.UserID, AppContexts.User.IPAddress);

            return result.IsNotNull() ? result.ToString() : "";
        }

        public static string SubmitTemplateBodyForExitInterview(int APTypeID, int MasterID, string TemplateBody = "")
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"HRMS..UpdateTemplateBodyExitInterview @0,@1,@2,@3,@4",
            APTypeID, MasterID, TemplateBody.IsNullOrEmpty() ? "" : TemplateBody, AppContexts.User.UserID, AppContexts.User.IPAddress);

            return result.IsNotNull() ? result.ToString() : "";
        }
        public static string SubmitFunctionalForAccessDeactivation(int MasterID, bool IsCoreFunctional)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"HRMS..SPUpdateAccessDeactivation @0,@1,@2,@3",
             MasterID, IsCoreFunctional, AppContexts.User.UserID, AppContexts.User.IPAddress);

            return result.IsNotNull() ? result.ToString() : "";
        }

        public static void UpdateCurrentFinancialYear(int year)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            context.ExecuteScalar("spUpdateCurrentFinancialYear @0", year);
        }
        public static async Task<SmtpClient> SetMailServerConfiguration(MailConfigurationDtoCore configuration)
        {
            SmtpClient client = new SmtpClient();
            client.Host = configuration.Host;
            client.Port = configuration.Port;
            client.EnableSsl = configuration.EnableSsl;
            client.UseDefaultCredentials = false;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            //client.PickupDirectoryLocation=
            //var securePassword = new SecureString();
            //"%NGD#$nagaderp@020".ToCharArray().ToList().ForEach(securePassword.AppendChar);
            //securePassword.MakeReadOnly();
            var securePassword = configuration.Password;
            client.Credentials = new NetworkCredential(configuration.UserName, securePassword);
            return client;
        }
        public static async Task SendEmail(EmailDtoCore emailData, SmtpClient client)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(emailData.FromEmailAddress, emailData.FromEmailAddressDisplayName);
            if (emailData.ToEmailAddress.Count > 0 && emailData.ToEmailAddress.FindAll(x => (!string.IsNullOrEmpty(x)) && (new EmailAddressAttribute().IsValid(x))).Count > 0)
            {
                mail.To.Add(string.Join(',', emailData.ToEmailAddress.FindAll(x => (!string.IsNullOrEmpty(x)) && (new EmailAddressAttribute().IsValid(x))).ToArray()));
            }
            if (emailData.CCEmailAddress.Count > 0 && emailData.CCEmailAddress.FindAll(x => (!string.IsNullOrEmpty(x)) && (new EmailAddressAttribute().IsValid(x))).Count > 0)
            {
                mail.CC.Add(string.Join(',', emailData.CCEmailAddress.FindAll(x => (!string.IsNullOrEmpty(x)) && (new EmailAddressAttribute().IsValid(x))).ToArray()));
            }
            if (emailData.BCCEmailAddress.Count > 0 && emailData.BCCEmailAddress.FindAll(x => (!string.IsNullOrEmpty(x)) && (new EmailAddressAttribute().IsValid(x))).Count > 0)
            {
                mail.Bcc.Add(string.Join(',', emailData.BCCEmailAddress.FindAll(x => (!string.IsNullOrEmpty(x)) && (new EmailAddressAttribute().IsValid(x))).ToArray()));
            }
            mail.Subject = emailData.Subject;
            mail.Body = emailData.EmailBody;
            mail.IsBodyHtml = emailData.IsBodyHtml;
            await client.SendMailAsync(mail);
        }

        public static async Task<MailConfigurationDtoCore> GetMailConfiguration()
        {
            string sql = $@"SELECT * FROM MailConfiguration..MailConfiguration WHERE IsActive = 1";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetModelData<MailConfigurationDtoCore>(sql);
            return await Task.FromResult(model);
        }

        public static async Task<MailGroupSetupDtoCore> MailGroupSetup(int GroupId)
        {
            string sql = $@"SELECT * FROM MailConfiguration..MailGroupSetup WHERE GroupId = {GroupId}";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetModelData<MailGroupSetupDtoCore>(sql);
            return await Task.FromResult(model);
        }
        public static async Task<List<MailSetupDto>> GetMailSetup(int GroupID)
        {
            string mail = string.Empty, proxyMail = string.Empty, sql = string.Empty;

            sql = $@"SELECT * FROM MailConfiguration..MailSetup where GroupId = {GroupID}";
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var list = context.GetDataModelCollection<MailSetupDto>(sql);

            return await Task.FromResult(list);
        }
        public static async Task<List<Dictionary<string, object>>> GetApprovalComments(int approvalProcessID, int APTypeID)
        {
            string commentSql = @$"SELECT * FROM Approval..ViewApprovalCommentList 
                                   WHERE ApprovalProcessId={approvalProcessID} AND APTypeId={APTypeID}
                                   ORDER BY OrderCommentDateTime";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var list = context.GetDataDictCollection(commentSql).ToList();
            return await Task.FromResult(list);
        }
        public static async Task<List<Dictionary<string, object>>> GetApprovalComments(int approvalProcessID, string APTypeIDs)
        {
            string commentSql = @$"SELECT * FROM Approval..ViewApprovalCommentList 
                                   WHERE ApprovalProcessId={approvalProcessID} AND APTypeId IN ({APTypeIDs})
                                   ORDER BY OrderCommentDateTime";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var list = context.GetDataDictCollection(commentSql).ToList();
            return await Task.FromResult(list);
        }
        public static async Task<List<ComboModel>> GetApprovalForwardingMembers(int ReferenceID, int APTypeID, int APPanelID)
        {
            string forwardingMemberSql = @$"
                    SELECT
	                    APPanelForwardEmployeeID value,
	                    FullName label
                    FROM
                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalPanelForwardEmployee AFI 
                        LEFT JOIN (
                            SELECT EmployeeID,FullName FROM HRMS..ViewALLEmployee
                        ) Emp ON Emp.EmployeeID = AFI.EmployeeID
                        INNER JOIN (
									SELECT 
											Ap.ApprovalProcessID,AP.ReferenceID,AEF.DivisionID,AEF.DepartmentID
									FROM 
										{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP
										LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF ON AP.ApprovalProcessID = AEF.ApprovalProcessID 
										WHERE APTypeID = {APTypeID} AND ReferenceID = {ReferenceID}
										GROUP BY Ap.ApprovalProcessID,AP.ReferenceID,AEF.DivisionID,AEF.DepartmentID
							)  DivDep ON DivDep.DepartmentID = AFI.DepartmentID AND DivDep.DivisionID = AFI.DivisionID
                    WHERE APPanelID = {APPanelID} AND AFI.EmployeeID <> {AppContexts.User.EmployeeID}";


            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var list = context.GetDataModelCollection<ComboModel>(forwardingMemberSql).ToList();
            return await Task.FromResult(list);
        }
        public static async Task<List<ComboModel>> GetApprovalForwardingMembers(int ApprovalProcessID)
        {
            string forwardingMemberSql = @$"
                    SELECT
	                    APPanelForwardEmployeeID value,
	                    FullName label
                    FROM
                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalPanelForwardEmployee AFI 
                        LEFT JOIN (
                            SELECT EmployeeID,FullName FROM HRMS..ViewALLEmployee
                        ) Emp ON Emp.EmployeeID = AFI.EmployeeID
                        INNER JOIN (
									SELECT 
											Ap.ApprovalProcessID,AP.ReferenceID,AEF.DivisionID,AEF.DepartmentID,APPanelID
									FROM 
										{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP
                                        INNER JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcessPanelMap APPM ON APPM.ApprovalProcessID = AP.ApprovalProcessID
										LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF ON AP.ApprovalProcessID = AEF.ApprovalProcessID 
										WHERE AP.ApprovalProcessID = {ApprovalProcessID}
										GROUP BY Ap.ApprovalProcessID,AP.ReferenceID,AEF.DivisionID,AEF.DepartmentID,APPanelID
							)  DivDep ON DivDep.DepartmentID = AFI.DepartmentID AND DivDep.DivisionID = AFI.DivisionID
                    WHERE AFI.APPanelID = DivDep.APPanelID AND AFI.EmployeeID <> {AppContexts.User.EmployeeID}";


            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var list = context.GetDataModelCollection<ComboModel>(forwardingMemberSql).ToList();
            return await Task.FromResult(list);
        }
        public static async Task<List<ComboModel>> GetApprovalForwardingMembers(int ApprovalProcessID, string DBContextName)
        {
            string forwardingMemberSql = @$"
                    SELECT
	                    APPanelForwardEmployeeID value,
	                    FullName label
                    FROM
                            {DBContextName}..ApprovalPanelForwardEmployee AFI 
                        LEFT JOIN (
                            SELECT EmployeeID,FullName FROM HRMS..ViewALLEmployee
                        ) Emp ON Emp.EmployeeID = AFI.EmployeeID
                        INNER JOIN (
									SELECT 
											Ap.ApprovalProcessID,AP.ReferenceID,AEF.DivisionID,AEF.DepartmentID,APPanelID
									FROM 
										{DBContextName}..ApprovalProcess AP
                                        INNER JOIN {DBContextName}..ApprovalProcessPanelMap APPM ON APPM.ApprovalProcessID = AP.ApprovalProcessID
										LEFT JOIN {DBContextName}..ApprovalEmployeeFeedback AEF ON AP.ApprovalProcessID = AEF.ApprovalProcessID 
										WHERE AP.ApprovalProcessID = {ApprovalProcessID}
										GROUP BY Ap.ApprovalProcessID,AP.ReferenceID,AEF.DivisionID,AEF.DepartmentID,APPanelID
							)  DivDep ON DivDep.DepartmentID = AFI.DepartmentID AND DivDep.DivisionID = AFI.DivisionID
                    WHERE AFI.APPanelID = DivDep.APPanelID AND AFI.EmployeeID <> {AppContexts.User.EmployeeID}";


            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var list = context.GetDataModelCollection<ComboModel>(forwardingMemberSql).ToList();
            return await Task.FromResult(list);
        }
        public static async Task<List<ComboModel>> GetApprovalRejectedMembers(int approvalProcessID)
        {
            string rejectedMemberSql = @$"SELECT
                                        APEmployeeFeedbackID value,
                                        VE.FullName label
                                        FROM
                                         {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
                                        INNER JOIN  {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VE ON AEF.EmployeeID = VE.EmployeeID
                                        WHERE AEF.ApprovalProcessID ={approvalProcessID} AND AEF.EmployeeID <> {AppContexts.User.EmployeeID} AND SequenceNo < 
                                        (SELECT TOP 1 SequenceNo FROM Approval..ApprovalEmployeeFeedback AEF 
                                        LEFT JOIN 
	                                        (
		                                        SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM Approval..ApprovalMultiProxyEmployeeInfo
	                                        ) Prox ON Prox.ApprovalProcessID = AEF.ApprovalProcessID AND Prox.APEmployeeFeedbackID = AEF.APEmployeeFeedbackID
							WHERE AEF.ApprovalProcessID = {approvalProcessID} AND (AEF.EmployeeID = {AppContexts.User.EmployeeID} OR AEF.ProxyEmployeeID = {AppContexts.User.EmployeeID} 
                                    OR Prox.EmployeeID = {AppContexts.User.EmployeeID})
							)";


            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var list = context.GetDataModelCollection<ComboModel>(rejectedMemberSql).ToList();
            return await Task.FromResult(list);
        }
        public static async Task<List<ComboModel>> GetApprovalRejectedFirstMember(int approvalProcessID)
        {
            string rejectedMemberSql = @$"SELECT
                                        APEmployeeFeedbackID value,
                                        VE.FullName label
                                        FROM
                                         {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF
                                        INNER JOIN  {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VE ON AEF.EmployeeID = VE.EmployeeID
                                        WHERE AEF.ApprovalProcessID = {approvalProcessID} AND SequenceNo = 1";


            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var list = context.GetDataModelCollection<ComboModel>(rejectedMemberSql).ToList();
            return await Task.FromResult(list);
        }
        public static async Task<List<ComboModel>> GetApprovalRejectedMembers(int approvalProcessID, string DBContextName)
        {
            string rejectedMemberSql = @$"SELECT
                                        APEmployeeFeedbackID value,
                                        VE.FullName label
                                        FROM
                                         {DBContextName}..ApprovalEmployeeFeedback AEF
                                        INNER JOIN  {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..ViewALLEmployee VE ON AEF.EmployeeID = VE.EmployeeID
                                        WHERE AEF.ApprovalProcessID ={approvalProcessID} AND AEF.EmployeeID <> {AppContexts.User.EmployeeID} AND SequenceNo < 
										(SELECT TOP 1 SequenceNo FROM Approval..ApprovalEmployeeFeedback AEF WHERE ApprovalProcessID = {approvalProcessID} AND (EmployeeID = {AppContexts.User.EmployeeID} OR ProxyEmployeeID = {AppContexts.User.EmployeeID})) ";


            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var list = context.GetDataModelCollection<ComboModel>(rejectedMemberSql).ToList();
            return await Task.FromResult(list);
        }
        public static Dictionary<string, object> GetApprovalProcessFeedback(int ReferenceID, int ApprovalProcessID, int APTypeID)
        {
            string sql = $@"SELECT * FROM {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..viewAllPendingApproval WHERE EmployeeID = {AppContexts.User.EmployeeID} AND ReferenceID = {ReferenceID} AND ApprovalProcessID = {ApprovalProcessID} AND APTypeID = {APTypeID}";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetData(sql);
            return model;
        }
        public static Dictionary<string, object> GetApprovalProcess(int ReferenceID, int APTypeID)
        {
            string sql = $@"SELECT * FROM {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess WHERE  ReferenceID = {ReferenceID}  AND APTypeID = {APTypeID}";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetData(sql);
            return model;
        }

        public static Dictionary<string, object> GetApprovalProcessAndFeedbackByEmployee(int ReferenceID, int APTypeID)
        {
            string sql = $@"SELECT AP.ApprovalProcessID,APTypeID,ReferenceID,AEF.APEmployeeFeedbackID,SequenceNo,EmployeeID FROM {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalProcess AP
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID = Ap.ApprovalProcessID 
                            WHERE  ReferenceID = {ReferenceID}  AND APTypeID = {APTypeID} AND EmployeeID = {AppContexts.User.EmployeeID}";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetData(sql);
            return model;
        }
        public static async Task<Dictionary<string, object>> GetApprovalEmployees(int ReferenceID, int APTypeID, string DBName)
        {
            string sql = $@"
                    SELECT STRING_AGG(CAST(EmployeeID AS VARCHAR(MAX)), ',') EmployeeIDs
			                    FROM ( 
				                    SELECT EmployeeID 
		                    FROM {DBName}..ApprovalProcess AP
		                    LEFT JOIN {DBName}..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID = Ap.ApprovalProcessID
		                    WHERE APTypeID = {APTypeID} AND ReferenceID = {ReferenceID}
			                    ) A;
		";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = await context.GetDataAsync(sql);
            return model;
        }
        public static async Task<List<NotificationMemebers>> GetApprovalListOfEmployees(int ReferenceID, int APTypeID, string DBName)
        {
            string sql = $@"
                            SELECT EmployeeID,APFeedbackID,SequenceNo,ISNULL(ProxyEmployeeID,0) ProxyEmployeeID
		                    FROM {DBName}..ApprovalProcess AP
		                    LEFT JOIN {DBName}..ApprovalEmployeeFeedback AEF ON AEF.ApprovalProcessID = Ap.ApprovalProcessID
		                    WHERE APTypeID = {APTypeID} AND ReferenceID = {ReferenceID}
			                ORDER BY SequenceNo asc
		";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var list = await context.GetDataModelCollectionAsync<NotificationMemebers>(sql);
            return list;
        }
        public static Dictionary<string, object> GetApprovalProcessFeedback(int ReferenceID, int ApprovalProcessID, int APTypeID, string DBContextName)
        {
            string sql = $@"SELECT * FROM {DBContextName}..viewAllPendingApproval WHERE EmployeeID = {AppContexts.User.EmployeeID} AND ReferenceID = {ReferenceID} AND ApprovalProcessID = {ApprovalProcessID} AND APTypeID = {APTypeID}";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetData(sql);
            return model;
        }
        public static async Task ApprovalProcessMailByDepartment(int ApprovalProcessID, int APTypeID, int GroupID, bool IsResubmitted, List<string> ToEmailAddress, List<string> CCEmailAddress = null, List<string> BCCEmailAddress = null,
           int ReferenceID = 0, int APFeedbackID = 0)
        {
            var mailConfigurationDtoCore = GetMailConfiguration().Result;
            var mailGroupSetup = MailGroupSetup(GroupID).Result;
            if (mailConfigurationDtoCore.IsNull() || mailGroupSetup.IsNull() || ToEmailAddress.IsNull()) return;
            List<Dictionary<string, object>> collection = GetModelData(APTypeID, ReferenceID);

            var client = SetMailServerConfiguration(mailConfigurationDtoCore).Result;
            var mailSubject = GenerateMailSubject(APTypeID, mailGroupSetup, IsResubmitted, GroupID, collection, APFeedbackID);
            var mailBody = GenerateMailBody(APTypeID, mailGroupSetup, IsResubmitted, GroupID, collection, APFeedbackID);

            EmailDtoCore emailData = new EmailDtoCore
            {
                FromEmailAddress = mailConfigurationDtoCore.UserName,
                FromEmailAddressDisplayName = mailConfigurationDtoCore.DisplayName,
                ToEmailAddress = ToEmailAddress ?? new List<string>() { },
                CCEmailAddress = CCEmailAddress ?? new List<string>() { },
                BCCEmailAddress = BCCEmailAddress ?? new List<string>() { },
                EmailDate = DateTime.Now,
                Subject = mailSubject,
                EmailBody = mailBody,
                IsBodyHtml = true
            };
            await SendEmail(emailData, client);
        }
        public static async Task ApprovalProcessMail(int ApprovalProcessID, int APTypeID, int GroupID, bool IsResubmitted, List<string> ToEmailAddress, List<string> CCEmailAddress = null, List<string> BCCEmailAddress = null,
            int ReferenceID = 0, int APFeedbackID = 0)
        {
            var mailConfigurationDtoCore = GetMailConfiguration().Result;
            var mailGroupSetup = MailGroupSetup(GroupID).Result;
            if (mailConfigurationDtoCore.IsNull() || mailGroupSetup.IsNull() || ToEmailAddress.IsNull()) return;
            List<Dictionary<string, object>> collection = GetModelData(APTypeID, ReferenceID);

            var client = SetMailServerConfiguration(mailConfigurationDtoCore).Result;
            var mailSubject = GenerateMailSubject(APTypeID, mailGroupSetup, IsResubmitted, GroupID, collection, APFeedbackID);
            var mailBody = GenerateMailBody(APTypeID, mailGroupSetup, IsResubmitted, GroupID, collection, APFeedbackID);

            EmailDtoCore emailData = new EmailDtoCore
            {
                FromEmailAddress = mailConfigurationDtoCore.UserName,
                FromEmailAddressDisplayName = mailConfigurationDtoCore.DisplayName,
                ToEmailAddress = ToEmailAddress ?? new List<string>() { },
                CCEmailAddress = CCEmailAddress ?? new List<string>() { },
                BCCEmailAddress = BCCEmailAddress ?? new List<string>() { },
                EmailDate = DateTime.Now,
                Subject = mailSubject,
                EmailBody = mailBody,
                IsBodyHtml = true
            };
            await SendEmail(emailData, client);
        }

        public static async Task ApprovalProcessMailWithModel(int ApprovalProcessID, int APTypeID, int GroupID, bool IsResubmitted, List<string> ToEmailAddress, List<string> CCEmailAddress = null, List<string> BCCEmailAddress = null,
           int ReferenceID = 0, int APFeedbackID = 0, string modelData = "")
        {
            var mailConfigurationDtoCore = GetMailConfiguration().Result;
            var mailGroupSetup = MailGroupSetup(GroupID).Result;
            if (mailConfigurationDtoCore.IsNull() || mailGroupSetup.IsNull() || ToEmailAddress.IsNull()) return;
            List<Dictionary<string, object>> collection = GetModelData(APTypeID, ReferenceID);

            var client = SetMailServerConfiguration(mailConfigurationDtoCore).Result;
            var mailSubject = GenerateMailSubject(APTypeID, mailGroupSetup, IsResubmitted, GroupID, collection, APFeedbackID);
            var mailBody = GenerateMailBodyWithModelData(APTypeID, mailGroupSetup, IsResubmitted, GroupID, collection, APFeedbackID, modelData);

            EmailDtoCore emailData = new EmailDtoCore
            {
                FromEmailAddress = mailConfigurationDtoCore.UserName,
                FromEmailAddressDisplayName = mailConfigurationDtoCore.DisplayName,
                ToEmailAddress = ToEmailAddress ?? new List<string>() { },
                CCEmailAddress = CCEmailAddress ?? new List<string>() { },
                BCCEmailAddress = BCCEmailAddress ?? new List<string>() { },
                EmailDate = DateTime.Now,
                Subject = mailSubject,
                EmailBody = mailBody,
                IsBodyHtml = true
            };
            await SendEmail(emailData, client);
        }
        public static async Task ApprovalProcessMail(int ApprovalProcessID, int APTypeID, List<int> GroupIDs, bool IsResubmitted, List<string> ToEmailAddress, List<string> CCEmailAddress = null, List<string> BCCEmailAddress = null,
           int ReferenceID = 0, int APFeedbackID = 0)
        {
            var mailConfigurationDtoCore = GetMailConfiguration().Result;
            var client = SetMailServerConfiguration(mailConfigurationDtoCore).Result;
            List<EmailDtoCore> emailDataList = new List<EmailDtoCore>();
            foreach (var GroupID in GroupIDs)
            {

                var mailGroupSetup = MailGroupSetup(GroupID).Result;
                if (mailConfigurationDtoCore.IsNull() || mailGroupSetup.IsNull() || ToEmailAddress.IsNull()) return;
                List<Dictionary<string, object>> collection = GetModelData(APTypeID, ReferenceID);

                var mailSubject = GenerateMailSubject(APTypeID, mailGroupSetup, IsResubmitted, GroupID, collection, APFeedbackID);
                var mailBody = GenerateMailBody(APTypeID, mailGroupSetup, IsResubmitted, GroupID, collection, APFeedbackID);

                var listOfMail = GetMailSetup(GroupID).Result;
                if (listOfMail.IsNotNull() && listOfMail.Count > 0)
                {
                    var toEmailAddress = listOfMail.Where(x => x.To_CC_BCC.Equals("TO")).Select(y => y.Email).ToArray();
                    var ccEmailAddress = listOfMail.Where(x => x.To_CC_BCC.Equals("CC")).Select(y => y.Email).ToArray();
                    if (GroupID == (int)Util.MailGroupSetup.SendMailToSCMGroupAfterPRApproved)
                    {
                        ToEmailAddress = new List<string>();
                        CCEmailAddress = new List<string>();
                        if (toEmailAddress.Length > 0) ToEmailAddress.AddRange(toEmailAddress);
                        if (ccEmailAddress.Length > 0) CCEmailAddress.AddRange(ccEmailAddress);
                    }
                    else
                    {
                        if (toEmailAddress.Length > 0) ToEmailAddress.AddRange(toEmailAddress);
                        if (ccEmailAddress.Length > 0) CCEmailAddress.AddRange(ccEmailAddress);
                    }
                }

                EmailDtoCore emailData = new EmailDtoCore
                {
                    FromEmailAddress = mailConfigurationDtoCore.UserName,
                    FromEmailAddressDisplayName = mailConfigurationDtoCore.DisplayName,
                    ToEmailAddress = ToEmailAddress ?? new List<string>() { },
                    CCEmailAddress = CCEmailAddress ?? new List<string>() { },
                    BCCEmailAddress = BCCEmailAddress ?? new List<string>() { },
                    EmailDate = DateTime.Now,
                    Subject = mailSubject,
                    EmailBody = mailBody,
                    IsBodyHtml = true
                };
                emailDataList.Add(emailData);
            }
            foreach (var emailData in emailDataList)
            {
                await SendEmail(emailData, client);
            }

        }

        public static async Task BasicMail(int GroupID, List<string> ToEmailAddress, bool IsResubmitted, List<string> CCEmailAddress = null, List<string> BCCEmailAddress = null, List<Dictionary<string, object>> collection = null)
        {
            var mailConfigurationDtoCore = GetMailConfiguration().Result;
            var mailGroupSetup = MailGroupSetup(GroupID).Result;
            if (mailConfigurationDtoCore.IsNull() || mailGroupSetup.IsNull() || ToEmailAddress.IsNull()) return;

            var client = SetMailServerConfiguration(mailConfigurationDtoCore).Result;
            var mailSubject = GenerateBasicMailSubject(mailGroupSetup, IsResubmitted, GroupID, collection);
            var mailBody = GenerateBasicMailBody(mailGroupSetup, IsResubmitted, GroupID, collection);

            EmailDtoCore emailData = new EmailDtoCore
            {
                FromEmailAddress = mailConfigurationDtoCore.UserName,
                FromEmailAddressDisplayName = mailConfigurationDtoCore.DisplayName,
                ToEmailAddress = ToEmailAddress ?? new List<string>() { },
                CCEmailAddress = CCEmailAddress ?? new List<string>() { },
                BCCEmailAddress = BCCEmailAddress ?? new List<string>() { },
                EmailDate = DateTime.Now,
                Subject = mailSubject,
                EmailBody = mailBody,
                IsBodyHtml = true
            };
            await SendEmail(emailData, client);
        }

        private static List<Dictionary<string, object>> GetModelData(int APTypeID, int ReferenceID)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            if (APTypeID == (int)Util.ApprovalType.LeaveApplication)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VE.FullName EmployeeName,
	                                 NoOfLeaveDays,
	                                 LvType.SystemVariableCode LeaveType,
									 CASE 
									 WHEN NoOfLeaveDays = 1 AND Convert(varchar,ELA.RequestStartDate,103) = Convert(varchar,ELA.RequestEndDate,103)
									 THEN 
										 Convert(varchar,ELA.RequestStartDate,103)
									 ELSE 
										Convert(varchar,ELA.RequestStartDate,103)+' to '+ Convert(varchar,ELA.RequestEndDate,103)
									 END 
										DateRange
                                FROM 
                                HRMS..EmployeeLeaveApplication ELA
                                INNER JOIN HRMS..ViewALLEmployee VE ON VE.EmployeeID = ELA.EmployeeID
                                INNER JOIN Security..SystemVariable LvType ON LvType.SystemVariableID = ELA.LeaveCategoryID

                                WHERE ELA.EmployeeLeaveAID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.NFA)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 NFA.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                Security..NFAMaster NFA
                                LEFT JOIN Users U ON U.UserID = NFA.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON NFA.ApprovalStatusID=SV.SystemVariableID
                                WHERE NFA.NFAID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.MicroSite)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 MSM.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                MicroSite..MicroSiteMaster MSM
                                LEFT JOIN Users U ON U.UserID = MSM.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON MSM.ApprovalStatusID=SV.SystemVariableID
                                WHERE MSM.MicroSiteMasterID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.IOUClaim)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 IOU.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                Accounts..IOUMaster IOU
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.EmployeeID = IOU.EmployeeID
								LEFT JOIN Security..SystemVariable SV ON IOU.ApprovalStatusID=SV.SystemVariableID
                                WHERE IOU.IOUMasterID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.ExpenseClaim)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 ECM.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                Accounts..ExpenseClaimMaster ECM
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.EmployeeID = ECM.EmployeeID
								LEFT JOIN Security..SystemVariable SV ON ECM.ApprovalStatusID=SV.SystemVariableID
                                WHERE ECM.ECMasterID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.ExpensePayment || APTypeID == (int)Util.ApprovalType.IOUPayment)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 ECM.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                Accounts..IOUOrExpensePaymentMaster ECM
								LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON ECM.ApprovalStatusID=SV.SystemVariableID
                                WHERE ECM.PaymentMasterID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.PR)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 ECM.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                SCM..PurchaseRequisitionMaster ECM
								LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON ECM.ApprovalStatusID = SV.SystemVariableID
                                WHERE ECM.PRMasterID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.MR)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 MR.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                SCM..MaterialRequisitionMaster MR
								LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON MR.ApprovalStatusID = SV.SystemVariableID
                                WHERE MR.MRMasterID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.PO)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 ECM.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                SCM..PurchaseOrderMaster ECM
								LEFT JOIN Security..Users U ON U.UserID = ECM.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON ECM.ApprovalStatusID = SV.SystemVariableID
                                WHERE ECM.POMasterID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.GRN)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 MR.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                SCM..MaterialReceive MR
								LEFT JOIN Security..Users U ON U.UserID = MR.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON MR.ApprovalStatusID = SV.SystemVariableID
                                WHERE MR.MRID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.QC)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 QC.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                SCM..QCMaster QC
								LEFT JOIN Security..Users U ON U.UserID = QC.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON QC.ApprovalStatusID = SV.SystemVariableID
                                WHERE QC.QCMID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.Invoice)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 IM.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                SCM..InvoiceMaster IM
								LEFT JOIN Security..Users U ON U.UserID = IM.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON IM.ApprovalStatusID = SV.SystemVariableID
                                WHERE IM.InvoiceMasterID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.InvoicePayment)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 IPM.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                SCM..InvoicePaymentMaster IPM
								LEFT JOIN Security..Users U ON U.UserID = IPM.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON IPM.ApprovalStatusID = SV.SystemVariableID
                                WHERE IPM.IPaymentMasterID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.DocumentApproval)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 DM.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status,
                                     Tem.DATName
                                FROM 
                                Approval..DocumentApprovalMaster DM
								LEFT JOIN Approval..DocumentApprovalTemplate Tem On DM.TemplateID=Tem.DATID
								LEFT JOIN Security..Users U ON U.UserID = DM.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON DM.ApprovalStatusID = SV.SystemVariableID
                                WHERE DM.DAMID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.TaxationVetting)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 DM.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                Accounts..TaxationVettingMaster DM
								LEFT JOIN Security..Users U ON U.UserID = DM.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON DM.ApprovalStatusID = SV.SystemVariableID
                                WHERE DM.TVMID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.TaxationVettingPayment)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 DM.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                Accounts..TaxationVettingPayment DM
								LEFT JOIN Security..Users U ON U.UserID = DM.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON DM.ApprovalStatusID = SV.SystemVariableID
                                WHERE DM.TVPID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.EmployeeProfileApproval)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                Security..EmployeeProfileApproval DM
								LEFT JOIN Security..Users U ON U.UserID = DM.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON DM.ApprovalStatusID = SV.SystemVariableID
                                WHERE DM.EPAID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.ExitInterview)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
                                     '' ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                HRMS..EmployeeExitInterview EEI								
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.EmployeeID= EEI.EmployeeID --VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON EEI.ApprovalStatusID = SV.SystemVariableID
                                WHERE EEI.EEIID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.AccessDeactivation)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
                                     '' ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                HRMS..EmployeeAccessDeactivation EAD
								
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.EmployeeID = EAD.EmployeeID
								LEFT JOIN Security..SystemVariable SV ON EAD.ApprovalStatusID = SV.SystemVariableID
                                WHERE EAD.EADID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.SCC)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 SCC.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                SCM..SCCMaster SCC
								LEFT JOIN Security..Users U ON U.UserID = SCC.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON SCC.ApprovalStatusID = SV.SystemVariableID
                                WHERE SCC.SCCMID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.EmployeeeDocumentUpload)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 DU.TINNumber,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status,
                                     Ftype.SystemVariableCode DocType
                                FROM 
                                HRMS..DocumentUpload DU
								LEFT JOIN Security..Users U ON U.UserID = DU.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON DU.ApprovalStatusID = SV.SystemVariableID
                                LEFT JOIN Security..SystemVariable Ftype ON DU.DocumentTypeID = Ftype.SystemVariableID
                                WHERE DU.DUID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.AdminSupportRequest)
            {
                string sql = $@"SELECT 
	                                 VA.EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 DU.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status,
                                     Ftype.SystemVariableCode DocType,
                                     CASE WHEN ISNULL(c.IsOthers,0)=1 THEN c.Vehicle COLLATE SQL_Latin1_General_CP1_CI_AS ELSE vd.VehicleRegNo COLLATE SQL_Latin1_General_CP1_CI_AS END AS VehicleDetl,
									 CASE WHEN ISNULL(c.IsOthers,0)=1 THEN c.Driver COLLATE SQL_Latin1_General_CP1_CI_AS ELSE dd.DriverName COLLATE SQL_Latin1_General_CP1_CI_AS END AS DriverDetl,
									 c.ContactNumber,
                                     DU.SupportTypeID,
                                     DU.AdminRemarks,
									 I.ItemName,
									 RSID.Quantity,
                                     UN.UnitCode UOM,
									 RSID.Remarks,
									 DU.RSMID,
                                     VA1.EmployeeCode+' - '+VA1.FullName SupportRequesterName,
                                     ICat.RenovationName,
									 RSRM.NeededByDate,
									 RSRM.Remarks RenovationDescription
                                FROM 
                                HRMS..RequestSupportMaster DU
								LEFT JOIN Security..Users U ON U.UserID = DU.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
                                LEFT JOIN HRMS..ViewALLEmployee VA1 ON VA1.EmployeeID = DU.EmployeeID
								LEFT JOIN Security..SystemVariable SV ON DU.ApprovalStatusID = SV.SystemVariableID
                                LEFT JOIN Security..SystemVariable Ftype ON DU.SupportTypeID = Ftype.SystemVariableID
                                LEFT JOIN hrms..RequestSupportVehicleDetails c ON c.RSMID = DU.RSMID
                                LEFT JOIN Security..VehicleDetails vd ON (ISNULL(c.IsOthers, 0) = 0 AND c.Vehicle COLLATE Latin1_General_CI_AS = CAST(vd.VehicleID AS NVARCHAR(MAX)))
                                       OR (ISNULL(c.IsOthers, 0) = 1 AND c.Vehicle = vd.VehicleRegNo COLLATE Latin1_General_CI_AS)
								LEFT JOIN Security..DriverDetails dd ON (ISNULL(c.IsOthers, 0) = 0 AND c.Driver COLLATE Latin1_General_CI_AS = CAST(dd.DriverID AS NVARCHAR(MAX)))
                                       OR (ISNULL(c.IsOthers, 0) = 1 AND c.Driver = dd.DriverName COLLATE Latin1_General_CI_AS)
								LEFT JOIN HRMS..RequestSupportItemDetails RSID ON RSID.RSMID = Du.RSMID
								--LEFT JOIN SCM..Item I ON I.ItemID = RSID.ItemID
                                LEFT JOIN Security..businessSupportItem I ON I.ItemID = RSID.ItemID
                                LEFT JOIN Security..Unit UN ON I.UnitID=UN.UnitID
                                LEFT JOIN HRMS..RequestSupportRenovationORMaintenanceDetails RSRM ON RSRM.RSMID = Du.RSMID
								LEFT JOIN hrms..RenovationORMaintenanceCategory ICat ON ICat.ROMID = RSRM.RenoOrMainCategoryID
                                WHERE DU.RSMID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.PettyCashExpenseClaim)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 ECM.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                Accounts..PettyCashExpenseMaster ECM
                                LEFT JOIN Users U ON U.UserID = ECM.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON ECM.ApprovalStatusID=SV.SystemVariableID
                                WHERE ECM.PCEMID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.PettyCashAdvanceClaim)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 PCA.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                Accounts..PettyCashAdvanceMaster PCA
                                LEFT JOIN Users U ON U.UserID = PCA.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON PCA.ApprovalStatusID=SV.SystemVariableID
                                WHERE PCA.PCAMID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 PCA.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                Accounts..PettyCashAdvanceMaster PCA
                                LEFT JOIN Users U ON U.UserID = PCA.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON PCA.ApprovalStatusID=SV.SystemVariableID
                                WHERE PCA.PCAMID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }
            else if (APTypeID == (int)Util.ApprovalType.PettyCashReimburseClaim)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 PCA.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                Accounts..PettyCashReimburseMaster PCA
                                LEFT JOIN Users U ON U.UserID = PCA.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON PCA.ApprovalStatusID=SV.SystemVariableID
                                WHERE PCA.PCRMID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }

            else if (APTypeID == (int)Util.ApprovalType.PettyCashPaymentClaim)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 PCA.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
                                FROM 
                                Accounts..PettyCashPaymentMaster PCA
                                LEFT JOIN Users U ON U.UserID = PCA.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON PCA.ApprovalStatusID=SV.SystemVariableID
                                WHERE PCA.PCPMID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }

            else if (APTypeID == (int)Util.ApprovalType.ExternalAudit)
            {
                string sql = $@"SELECT 
	                                 EmployeeCode+' - '+VA.FullName EmployeeName,
	                                 PCA.ReferenceNo,
	                                 VA.DepartmentName,
									 VA.DivisionName,
                                     SV.SystemVariableCode Status
									 --Feedback.Name Status
                                FROM 
                                HRMS..ExternalAuditMaster PCA
                                LEFT JOIN Users U ON U.UserID = PCA.CreatedBy
								LEFT JOIN HRMS..ViewALLEmployee VA ON VA.PersonID = U.PersonID
								LEFT JOIN Security..SystemVariable SV ON PCA.ApprovalStatusID=SV.SystemVariableID
                                LEFT JOIN Approval..ApprovalProcess AppProcess ON AppProcess.ReferenceID = PCA.EAMID
								LEFT JOIN Approval..ApprovalEmployeeFeedback EmpFeedback ON EmpFeedback.ApprovalProcessID = AppProcess.ApprovalProcessID
								LEFT JOIN  Approval..ApprovalFeedback Feedback ON Feedback.APFeedbackID = EmpFeedback.APFeedbackID
                                WHERE PCA.EAMID = {ReferenceID}";
                var list = context.GetDataDictCollection(sql).ToList();
                return list;
            }

            return new List<Dictionary<string, object>>();
        }

        private static string GenerateMailBody(int aPTypeID, MailGroupSetupDtoCore mailGroupSetup, bool IsResubmitted, int GroupID, List<Dictionary<string, object>> collection = null, int APFeedbackID = 0)
        {
            var mailBody = string.Empty;

            if (aPTypeID == (int)Util.ApprovalType.LeaveApplication)
            {
                if (GroupID == (int)Util.MailGroupSetup.LeaveApplicationMailForBackupEmployee || GroupID == (int)Util.MailGroupSetup.LeaveApprovalMail)
                {
                    string EmployeeNameAndCode = string.Empty, LeaveType = string.Empty, NoOfLeaveDays = string.Empty, DateRange = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        LeaveType = collection[0]["LeaveType"].ToString();
                        NoOfLeaveDays = collection[0]["NoOfLeaveDays"].ToString();
                        DateRange = collection[0]["DateRange"].ToString();

                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{LeaveType}}", LeaveType).Replace("{{NoOfLeaveDays}}", NoOfLeaveDays).Replace("{{Date}}", DateRange);
                }

            }
            if (aPTypeID == (int)Util.ApprovalType.NFA)
            {
                if (GroupID == (int)Util.MailGroupSetup.NFAInitiatedMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department);
                }
                else if (GroupID == (int)Util.MailGroupSetup.FinalNFAAPprovalStatusToInitiator)
                {
                    string ReferenceNo = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.NFAForwardedOrRejectionMail)
                {
                    string ReferenceNo = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.NFAForwardFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = $@"{AppContexts.User.EmployeeCode} - {AppContexts.User.FullName}"; //collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department);
                }

            }

            if (aPTypeID == (int)Util.ApprovalType.MicroSite)
            {
                if (GroupID == (int)Util.MailGroupSetup.MicroSiteInitiatedMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department);
                }
                else if (GroupID == (int)Util.MailGroupSetup.FinalMicroSiteApprovalStatusToInitiator)
                {
                    string ReferenceNo = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.MicroSiteForwardedOrRejectionMail)
                {
                    string ReferenceNo = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.MicroSiteForwardFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = $@"{AppContexts.User.EmployeeCode} - {AppContexts.User.FullName}"; //collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department);
                }

            }

            if (aPTypeID == (int)Util.ApprovalType.EmployeeProfileApproval)
            {
                if (GroupID == (int)Util.MailGroupSetup.EmployeeProfileInitiatedMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["EmployeeName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department);
                }
                else if (GroupID == (int)Util.MailGroupSetup.FinalEmployeeProfileApprovalStatusToInitiator)
                {
                    string ReferenceNo = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        ReferenceNo = collection[0]["EmployeeName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.EmployeeProfileForwardedOrRejectionMail)
                {
                    string ReferenceNo = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        ReferenceNo = collection[0]["EmployeeName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.EmployeeProfileForwardFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = $@"{AppContexts.User.EmployeeCode} - {AppContexts.User.FullName}"; //collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["EmployeeName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department);
                }

            }

            if (aPTypeID == (int)Util.ApprovalType.EmployeeeDocumentUpload)
            {
                if (GroupID == (int)Util.MailGroupSetup.EmployeeDocumentUploadInitiatedMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["DocType"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department);
                }
                else if (GroupID == (int)Util.MailGroupSetup.FinalEmployeeDocumentUploadApprovalStatusToInitiator)
                {
                    string ReferenceNo = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        ReferenceNo = collection[0]["DocType"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.EmployeeDocumentUploadForwardedOrRejectionMail)
                {
                    string ReferenceNo = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        ReferenceNo = collection[0]["DocType"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.EmployeeDocumentUploadForwardFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = $@"{AppContexts.User.EmployeeCode} - {AppContexts.User.FullName}"; //collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["DocType"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department);
                }

            }

            if (aPTypeID == (int)Util.ApprovalType.AdminSupportRequest)
            {
                if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportInitiatedMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty; string RaisedByEmployee = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        RaisedByEmployee = collection[0]["EmployeeName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department).Replace("{{RaisedByEmployee}}", RaisedByEmployee);
                }
                else if (GroupID == (int)Util.MailGroupSetup.FinalAdminRequestSupportApprovalStatusToInitiator)
                {
                    string ReferenceNo = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportForwardedOrRejectionMail)
                {
                    string ReferenceNo = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportForwardFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = $@"{AppContexts.User.EmployeeCode} - {AppContexts.User.FullName}"; //collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department);
                }
                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportSettlementFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty; string VehicleDetl = string.Empty; string DriverDetl = string.Empty; string ContactNumber = string.Empty; string RaisedByEmployee = string.Empty; string AdminRemarks = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = $@"{AppContexts.User.EmployeeCode} - {AppContexts.User.FullName}"; //collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        VehicleDetl = collection[0]["VehicleDetl"].ToString();
                        DriverDetl = collection[0]["DriverDetl"].ToString();
                        ContactNumber = collection[0]["ContactNumber"].ToString();
                        RaisedByEmployee = collection[0]["EmployeeName"].ToString();
                        AdminRemarks = collection[0]["AdminRemarks"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department).Replace("{{VehicleDetl}}", VehicleDetl).Replace("{{DriverDetl}}", DriverDetl).Replace("{{ContactNumber}}", ContactNumber).Replace("{{RaisedByEmployee}}", RaisedByEmployee).Replace("{{AdminRemarks}}", AdminRemarks).Replace("{{AdminRemarks}}", AdminRemarks);
                }
                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportWithoutVehicleSettlementFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty; string RaisedByEmployee = string.Empty; string AdminRemarks = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = $@"{AppContexts.User.EmployeeCode} - {AppContexts.User.FullName}"; //collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        RaisedByEmployee = collection[0]["EmployeeName"].ToString();
                        AdminRemarks = collection[0]["AdminRemarks"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department).Replace("{{RaisedByEmployee}}", RaisedByEmployee).Replace("{{AdminRemarks}}", AdminRemarks).Replace("{{AdminRemarks}}", AdminRemarks);
                }
                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportConsumbleGoodsFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty; string RaisedByEmployee = string.Empty; string AdminRemarks = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = $@"{AppContexts.User.EmployeeCode} - {AppContexts.User.FullName}"; //collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        RaisedByEmployee = collection[0]["EmployeeName"].ToString();
                        AdminRemarks = collection[0]["AdminRemarks"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).
                        Replace("{{Department}}", Department).Replace("{{RaisedByEmployee}}", RaisedByEmployee).Replace("{{AdminRemarks}}", AdminRemarks).
                        Replace("{{AdminRemarks}}", AdminRemarks);
                    string tableRow = "";
                    foreach (var item in collection)
                    {
                        tableRow += @$"<tr><td>{item["ItemName"]}</td><td>{item["Quantity"]}</td><td>{item["UOM"]}</td><td>{item["Remarks"]}</td></tr>";
                    }
                    mailBody = mailBody.Replace("{{ConsumerDetails}}", tableRow);
                }

                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportVehicleSupportRejectMail)
                {
                    string ReferenceNo = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Status}}", Status);
                }
                // Mail Body Create for Employee
                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportEmployeeInitiatedMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty; string RaisedByEmployee = string.Empty; string SupportRequesterName = string.Empty; string DocType = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        RaisedByEmployee = collection[0]["EmployeeName"].ToString();
                        SupportRequesterName = collection[0]["SupportRequesterName"].ToString();
                        DocType = collection[0]["DocType"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department).Replace("{{RaisedByEmployee}}", RaisedByEmployee).Replace("{{SupportRequesterName}}", SupportRequesterName).Replace("{{DocType}}", DocType);
                }
                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportSettlementEmployeeFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty; string VehicleDetl = string.Empty; string DriverDetl = string.Empty; string ContactNumber = string.Empty; string RaisedByEmployee = string.Empty; string AdminRemarks = string.Empty; string SupportRequesterName = string.Empty; string DocType = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = $@"{AppContexts.User.EmployeeCode} - {AppContexts.User.FullName}"; //collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        VehicleDetl = collection[0]["VehicleDetl"].ToString();
                        DriverDetl = collection[0]["DriverDetl"].ToString();
                        ContactNumber = collection[0]["ContactNumber"].ToString();
                        RaisedByEmployee = collection[0]["EmployeeName"].ToString();
                        AdminRemarks = collection[0]["AdminRemarks"].ToString();
                        SupportRequesterName = collection[0]["SupportRequesterName"].ToString();
                        DocType = collection[0]["DocType"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department).Replace("{{VehicleDetl}}", VehicleDetl).Replace("{{DriverDetl}}", DriverDetl).Replace("{{ContactNumber}}", ContactNumber).Replace("{{RaisedByEmployee}}", RaisedByEmployee).Replace("{{AdminRemarks}}", AdminRemarks).Replace("{{AdminRemarks}}", AdminRemarks).Replace("{{SupportRequesterName}}", SupportRequesterName).Replace("{{DocType}}", DocType);
                }
                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportEmployeeConsumbleGoodsFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty; string RaisedByEmployee = string.Empty; string AdminRemarks = string.Empty; string SupportRequesterName = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = $@"{AppContexts.User.EmployeeCode} - {AppContexts.User.FullName}"; //collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        RaisedByEmployee = collection[0]["EmployeeName"].ToString();
                        AdminRemarks = collection[0]["AdminRemarks"].ToString();
                        SupportRequesterName = collection[0]["SupportRequesterName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).
                        Replace("{{Department}}", Department).Replace("{{RaisedByEmployee}}", RaisedByEmployee).Replace("{{AdminRemarks}}", AdminRemarks).
                        Replace("{{AdminRemarks}}", AdminRemarks).Replace("{{SupportRequesterName}}", SupportRequesterName);
                    string tableRow = "";
                    foreach (var item in collection)
                    {
                        tableRow += @$"<tr><td>{item["ItemName"]}</td><td>{item["Quantity"]}</td><td>{item["UOM"]}</td><td>{item["Remarks"]}</td></tr>";
                    }
                    mailBody = mailBody.Replace("{{ConsumerDetails}}", tableRow);
                }
                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportEmployeeWithoutVehicleSettlementFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty; string RaisedByEmployee = string.Empty; string AdminRemarks = string.Empty; string SupportRequesterName = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = $@"{AppContexts.User.EmployeeCode} - {AppContexts.User.FullName}"; //collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        RaisedByEmployee = collection[0]["EmployeeName"].ToString();
                        AdminRemarks = collection[0]["AdminRemarks"].ToString();
                        SupportRequesterName = collection[0]["SupportRequesterName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department).Replace("{{RaisedByEmployee}}", RaisedByEmployee).Replace("{{AdminRemarks}}", AdminRemarks).Replace("{{AdminRemarks}}", AdminRemarks).Replace("{{SupportRequesterName}}", SupportRequesterName);
                }
                else if (GroupID == (int)Util.MailGroupSetup.FinalAdminRequestSupportEmployeeApprovalStatusToInitiator)
                {
                    string ReferenceNo = string.Empty, Status = string.Empty; string RaisedByEmployee = string.Empty; string SupportRequesterName = string.Empty;
                    if (collection.Count > 0)
                    {
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                        RaisedByEmployee = collection[0]["EmployeeName"].ToString();
                        SupportRequesterName = collection[0]["SupportRequesterName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Status}}", Status).Replace("{{RaisedByEmployee}}", RaisedByEmployee).Replace("{{SupportRequesterName}}", SupportRequesterName);
                }
                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportEmployeeVehicleSupportRejectMail)
                {
                    string ReferenceNo = string.Empty, Status = string.Empty; string RaisedByEmployee = string.Empty; string SupportRequesterName = string.Empty;
                    if (collection.Count > 0)
                    {
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                        RaisedByEmployee = collection[0]["EmployeeName"].ToString();
                        SupportRequesterName = collection[0]["SupportRequesterName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Status}}", Status).Replace("{{RaisedByEmployee}}", RaisedByEmployee).Replace("{{SupportRequesterName}}", SupportRequesterName);
                }

                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportRenovationOrMaintenanceFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty; string RaisedByEmployee = string.Empty; string AdminRemarks = string.Empty; string SupportRequesterName = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = $@"{AppContexts.User.EmployeeCode} - {AppContexts.User.FullName}"; //collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        RaisedByEmployee = collection[0]["EmployeeName"].ToString();
                        AdminRemarks = collection[0]["AdminRemarks"].ToString();
                        //SupportRequesterName = string.IsNullOrEmpty(collection[0]["SupportRequesterName"].ToString()) ? string.Empty : collection[0]["SupportRequesterName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).
                        Replace("{{Department}}", Department).Replace("{{RaisedByEmployee}}", RaisedByEmployee).Replace("{{AdminRemarks}}", AdminRemarks).
                        Replace("{{AdminRemarks}}", AdminRemarks);
                    string tableRow = "";

                    foreach (var item in collection)
                    {
                        tableRow += @$"<tr><td>{item["RenovationName"]}</td><td>{item["RenovationDescription"]}</td><td>{item["NeededByDate"]}</td> </tr>";
                    }
                    mailBody = mailBody.Replace("{{ConsumerDetails}}", tableRow);
                }

                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportEmployeeRenovationOrMaintenanceFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty; string RaisedByEmployee = string.Empty; string AdminRemarks = string.Empty; string SupportRequesterName = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = $@"{AppContexts.User.EmployeeCode} - {AppContexts.User.FullName}"; //collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        RaisedByEmployee = collection[0]["EmployeeName"].ToString();
                        AdminRemarks = collection[0]["AdminRemarks"].ToString();
                        SupportRequesterName = collection[0]["SupportRequesterName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).
                        Replace("{{Department}}", Department).Replace("{{RaisedByEmployee}}", RaisedByEmployee).Replace("{{AdminRemarks}}", AdminRemarks).
                        Replace("{{AdminRemarks}}", AdminRemarks).Replace("{{SupportRequesterName}}", SupportRequesterName);
                    string tableRow = "";
                    foreach (var item in collection)
                    {
                        tableRow += @$"<tr><td>{item["RenovationName"]}</td><td>{item["RenovationDescription"]}</td><td>{item["NeededByDate"]}</td> </tr>";
                    }
                    mailBody = mailBody.Replace("{{ConsumerDetails}}", tableRow);
                }


            }

            if (aPTypeID == (int)Util.ApprovalType.ExternalAudit)
            {
                if (GroupID == (int)Util.MailGroupSetup.ExternalAuditInitiatedMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty; string RaisedByEmployee = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        RaisedByEmployee = collection[0]["EmployeeName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department).Replace("{{RaisedByEmployee}}", RaisedByEmployee);
                }
                else if (GroupID == (int)Util.MailGroupSetup.FinalExternalAuditApprovalStatusToInitiator)
                {
                    string ReferenceNo = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Status}}", "Approved");
                }
                else if (GroupID == (int)Util.MailGroupSetup.ExternalAuditForwardedOrRejectionMail)
                {
                    string ReferenceNo = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.ExternalAuditForwardFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = $@"{AppContexts.User.EmployeeCode} - {AppContexts.User.FullName}"; //collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department);
                }
            }

            if (aPTypeID == (int)Util.ApprovalType.EmailNotification)
            {
                string EmployeeName = string.Empty, NameOfMonth = string.Empty, DateOfAbsence = string.Empty, TotalAbsentDays = string.Empty;
                if (collection.Count > 0)
                {
                    EmployeeName = collection[0]["EmployeeName"].ToString();
                    NameOfMonth = collection[0]["NameOfMonth"].ToString();
                    DateOfAbsence = collection[0]["DateOfAbsence"].ToString();
                    TotalAbsentDays = collection[0]["TotalAbsentDays"].ToString();

                }

                string[] absenceDates = DateOfAbsence.Split('|');

                // Create a string to store the formatted dates
                StringBuilder formattedDates = new StringBuilder();

                foreach (string date in absenceDates)
                {
                    if (DateTime.TryParse(date.Trim(), out DateTime parsedDate))
                    {
                        string formattedDate = parsedDate.ToString("dddd, MMMM d, yyyy");
                        formattedDates.Append("<strong>Date Of Absence: ").Append(formattedDate).Append("</strong><br />\n");
                    }
                    //formattedDates.Append("<strong>Date Of Absence: ").Append(date.Trim()).Append("</strong><br />\n");
                }
                mailBody = mailGroupSetup.Body.Replace("{{EmployeeName}}", EmployeeName).Replace("{{NameOfMonth}}", NameOfMonth).Replace("{{DateOfAbsence}}", formattedDates.ToString()).Replace("{{TotalAbsentDays}}", TotalAbsentDays);
            }


            if (aPTypeID == (int)Util.ApprovalType.IOUClaim || aPTypeID == (int)Util.ApprovalType.ExpenseClaim
           || aPTypeID == (int)Util.ApprovalType.IOUPayment || aPTypeID == (int)Util.ApprovalType.ExpensePayment
           || aPTypeID == (int)Util.ApprovalType.PR || aPTypeID == (int)Util.ApprovalType.PO
           || aPTypeID == (int)Util.ApprovalType.GRN
           || aPTypeID == (int)Util.ApprovalType.DocumentApproval
           || aPTypeID == (int)Util.ApprovalType.QC
           || aPTypeID == (int)Util.ApprovalType.MR
           || aPTypeID == (int)Util.ApprovalType.TaxationVetting
           || aPTypeID == (int)Util.ApprovalType.Invoice
           || aPTypeID == (int)Util.ApprovalType.InvoicePayment
           || aPTypeID == (int)Util.ApprovalType.TaxationVettingPayment
           || aPTypeID == (int)Util.ApprovalType.ExitInterview
           || aPTypeID == (int)Util.ApprovalType.AccessDeactivation
           || aPTypeID == (int)Util.ApprovalType.DivisionClearance
           || aPTypeID == (int)Util.ApprovalType.SCC
           || aPTypeID == (int)Util.ApprovalType.PettyCashExpenseClaim
           || aPTypeID == (int)Util.ApprovalType.PettyCashAdvanceClaim
           || aPTypeID == (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim
           || aPTypeID == (int)Util.ApprovalType.PettyCashReimburseClaim
           || aPTypeID == (int)Util.ApprovalType.PettyCashPaymentClaim
           || aPTypeID == (int)Util.ApprovalType.SupportRequisition
           || aPTypeID == (int)Util.ApprovalType.ExternalAudit
            //|| aPTypeID == (int)Util.ApprovalType.EmployeeeDocumentUpload
            )
            {
                if (
                       GroupID == (int)Util.MailGroupSetup.IOUClaimInitiatedMail || GroupID == (int)Util.MailGroupSetup.IOUClaimForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.ExpenseClaimInitiatedMail || GroupID == (int)Util.MailGroupSetup.ExpenseClaimForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.ExpensePaymentInitiatedMail || GroupID == (int)Util.MailGroupSetup.ExpensePaymentForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.IOUPaymentInitiatedMail || GroupID == (int)Util.MailGroupSetup.IOUPaymentForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.PRInitiatedMail || GroupID == (int)Util.MailGroupSetup.PRForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.POInitiatedMail || GroupID == (int)Util.MailGroupSetup.POForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.GRNInitiatedMail || GroupID == (int)Util.MailGroupSetup.GRNForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.DocumentApprovalInitiatedMail || GroupID == (int)Util.MailGroupSetup.DocumentApprovalForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.QCInitiatedMail || GroupID == (int)Util.MailGroupSetup.QCForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.InvoiceInitiatedMail || GroupID == (int)Util.MailGroupSetup.InvoiceForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.MRInitiatedMail || GroupID == (int)Util.MailGroupSetup.MRForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.InvoicePaymentInitiatedMail || GroupID == (int)Util.MailGroupSetup.InvoicePaymentForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.TaxationVettingInitiatedMail || GroupID == (int)Util.MailGroupSetup.TaxationVettingForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.TaxationPaymentInitiatedMail || GroupID == (int)Util.MailGroupSetup.TaxationPaymentForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.ExitInterviewInitiatedMail || GroupID == (int)Util.MailGroupSetup.ExitInterviewForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.EmployeeAccessDeactivationInitiatedMail || GroupID == (int)Util.MailGroupSetup.EmployeeAccessDeactivationForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.DivisionClearenceInitiatedMail || GroupID == (int)Util.MailGroupSetup.DivisionClearenceForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.SCCInitiatedMail || GroupID == (int)Util.MailGroupSetup.SCCForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.PettyCashExpenseClaimInitiatedMail || GroupID == (int)Util.MailGroupSetup.PettyCashExpenseClaimForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.PettyCashAdvanceClaimInitiatedMail || GroupID == (int)Util.MailGroupSetup.PettyCashAdvanceClaimForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.PettyCashAdvanceResubmitClaimInitiatedMail || GroupID == (int)Util.MailGroupSetup.PettyCashAdvanceResubmitClaimForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.PettyCashReimburseClaimInitiatedMail || GroupID == (int)Util.MailGroupSetup.PettyCashReimburseClaimForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.PettyCashPaymentInitiatedMail || GroupID == (int)Util.MailGroupSetup.PettyCashPaymentForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.RequisitionSupportInitiatedMail || GroupID == (int)Util.MailGroupSetup.RequisitionSupportForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.ExternalAuditInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.ExternalAuditForwardFeedbackReceieve
                    //|| GroupID == (int)Util.MailGroupSetup.EmployeeDocumentUploadInitiatedMail || GroupID == (int)Util.MailGroupSetup.EmployeeDocumentUploadForwardFeedbackReceieve
                    )
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department);
                }
                else if (
                       GroupID == (int)Util.MailGroupSetup.FinalIOUCliamApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.IOUClaimForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalExpenseCliamApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.ExpenseClaimForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalExpensePaymentApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.ExpensePaymentForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalIOUPaymentApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.IOUPaymentForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalPRApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.PRForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalPOApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.POForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalGRNApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.GRNForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalDocumentApprovalApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.DocumentApprovalForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalQCApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.QCForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalInvoiceApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.InvoiceForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalMRApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.MRForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalInvoicePaymentApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.InvoicePaymentForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalTaxationVettingApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.TaxationVettingForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalTaxationPaymentApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.TaxationPaymentForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalExitInterviewApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.ExitInterviewForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalEmployeeAccessDeactivationApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.EmployeeAccessDeactivationForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalDivisionClearenceApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.DivisionClearenceForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalSCCApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.SCCForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalPettyCashExpenseCliamApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.PettyCashExpenseClaimForwardedOrRejectionMail

                    || GroupID == (int)Util.MailGroupSetup.FinalPettyCashAdvanceClaimApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.PettyCashAdvanceClaimForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalPettyCashAdvanceResubmitClaimApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.PettyCashAdvanceResubmitClaimForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalPettyCashReimburseClaimApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.PettyCashReimburseClaimForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalPettyCashPaymentApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.PettyCashPaymentForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.FinalRequisitionSupportApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.RequisitionSupportForwardedOrRejectionMail
                    || GroupID == (int)Util.MailGroupSetup.ExternalAuditForwardedOrRejectionMail

                    //|| GroupID == (int)Util.MailGroupSetup.FinalEmployeeDocumentUploadApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.EmployeeDocumentUploadForwardedOrRejectionMail

                    )
                {
                    string ReferenceNo = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.SendMailToSCMGroupAfterPRApproved)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }

                else if (GroupID == (int)Util.MailGroupSetup.EmployeeAccessDeactivationInitiatedMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }

                else if (GroupID == (int)Util.MailGroupSetup.DivisionClearenceInitiatedMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.EmployeeDocumentUploadInitiatedMail)
                {
                    string EmployeeNameAndCode = string.Empty, TINNumber = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        TINNumber = collection[0]["TINNumber"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{TINNumber}}", TINNumber).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
            }
            return mailBody;
        }

        private static string GenerateMailBodyWithModelData(int aPTypeID, MailGroupSetupDtoCore mailGroupSetup, bool IsResubmitted, int GroupID, List<Dictionary<string, object>> collection = null, int APFeedbackID = 0, string modelTbl = "")
        {
            var mailBody = string.Empty;
            if (aPTypeID == (int)Util.ApprovalType.ExternalAudit)
            {
                if (GroupID == (int)Util.MailGroupSetup.ExternalAuditInitiatedMailDepartmentWise)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Department = string.Empty; string RaisedByEmployee = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        RaisedByEmployee = collection[0]["EmployeeName"].ToString();
                    }
                    mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Department}}", Department).Replace("{{RaisedByEmployee}}", RaisedByEmployee).Replace("{{Model}}", modelTbl);
                }
            }
            return mailBody;
        }

        private static string GenerateMailSubject(int aPTypeID, MailGroupSetupDtoCore mailGroupSetup, bool IsResubmitted, int GroupID, List<Dictionary<string, object>> collection = null, int APFeedbackID = 0)
        {
            var mailSubject = string.Empty;
            if (aPTypeID == (int)Util.ApprovalType.LeaveApplication)
            {
                if (GroupID == (int)Util.MailGroupSetup.LeaveApplicationMailForBackupEmployee || GroupID == (int)Util.MailGroupSetup.LeaveApprovalMail)
                {
                    string EmployeeNameAndCode = string.Empty, LeaveType = string.Empty, NoOfLeaveDays = string.Empty, DateRange = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        LeaveType = collection[0]["LeaveType"].ToString();
                        NoOfLeaveDays = collection[0]["NoOfLeaveDays"].ToString();
                        DateRange = collection[0]["DateRange"].ToString();
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{LeaveType}}", LeaveType).Replace("{{NoOfLeaveDays}}", NoOfLeaveDays).Replace("{{Date}}", DateRange);
                }
            }
            if (aPTypeID == (int)Util.ApprovalType.NFA)
            {
                if (GroupID == (int)Util.MailGroupSetup.NFAInitiatedMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department);
                }
                else if (GroupID == (int)Util.MailGroupSetup.FinalNFAAPprovalStatusToInitiator)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.NFAForwardedOrRejectionMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.NFAForwardFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
            }
            if (aPTypeID == (int)Util.ApprovalType.MicroSite)
            {
                if (GroupID == (int)Util.MailGroupSetup.MicroSiteInitiatedMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department);
                }
                else if (GroupID == (int)Util.MailGroupSetup.FinalMicroSiteApprovalStatusToInitiator)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.MicroSiteForwardedOrRejectionMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.MicroSiteForwardFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
            }

            if (aPTypeID == (int)Util.ApprovalType.EmployeeProfileApproval)
            {
                if (GroupID == (int)Util.MailGroupSetup.EmployeeProfileInitiatedMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["EmployeeName"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department);
                }
                else if (GroupID == (int)Util.MailGroupSetup.FinalEmployeeProfileApprovalStatusToInitiator)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["EmployeeName"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.EmployeeProfileForwardedOrRejectionMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["EmployeeName"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.EmployeeProfileForwardFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["EmployeeName"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
            }


            //DocumentApproval NOC/Visa/Imigration Start
            if (aPTypeID == (int)Util.ApprovalType.DocumentApproval)
            {
                if (GroupID == (int)Util.MailGroupSetup.DocumentApprovalInitiatedMail)
                {
                    string EmployeeNameAndCode = string.Empty, DocumentType = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        DocumentType = collection[0]["DATName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{DocumentType}}", DocumentType).Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department);
                }
                else if (GroupID == (int)Util.MailGroupSetup.FinalDocumentApprovalApprovalStatusToInitiator)
                {
                    string EmployeeNameAndCode = string.Empty, DocumentType = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        DocumentType = collection[0]["DATName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{DocumentType}}", DocumentType).Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.DocumentApprovalForwardedOrRejectionMail)
                {
                    string EmployeeNameAndCode = string.Empty, DocumentType = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        DocumentType = collection[0]["DATName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{DocumentType}}", DocumentType).Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.DocumentApprovalForwardFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, DocumentType = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        DocumentType = collection[0]["DATName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{DocumentType}}", DocumentType).Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
            }
            //End

            if (aPTypeID == (int)Util.ApprovalType.EmployeeeDocumentUpload)
            {
                if (GroupID == (int)Util.MailGroupSetup.EmployeeDocumentUploadInitiatedMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["DocType"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department);
                }
                else if (GroupID == (int)Util.MailGroupSetup.FinalEmployeeDocumentUploadApprovalStatusToInitiator)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["DocType"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.EmployeeDocumentUploadForwardedOrRejectionMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["DocType"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.EmployeeDocumentUploadForwardFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["DocType"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
            }

            if (aPTypeID == (int)Util.ApprovalType.AdminSupportRequest)
            {
                if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportInitiatedMail || GroupID == (int)Util.MailGroupSetup.AdminRequestSupportEmployeeInitiatedMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department);
                }
                else if (GroupID == (int)Util.MailGroupSetup.FinalAdminRequestSupportApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.FinalAdminRequestSupportEmployeeApprovalStatusToInitiator)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["DocType"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportForwardedOrRejectionMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["DocType"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportForwardFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["DocType"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportSettlementFeedbackReceieve || GroupID == (int)Util.MailGroupSetup.AdminRequestSupportWithoutVehicleSettlementFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.AdminRequestSupportConsumbleGoodsFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.AdminRequestSupportSettlementEmployeeFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.AdminRequestSupportEmployeeConsumbleGoodsFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.AdminRequestSupportEmployeeWithoutVehicleSettlementFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.AdminRequestSupportEmployeeRenovationOrMaintenanceFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.AdminRequestSupportRenovationOrMaintenanceFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["DocType"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.AdminRequestSupportVehicleSupportRejectMail || GroupID == (int)Util.MailGroupSetup.AdminRequestSupportEmployeeVehicleSupportRejectMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["DocType"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }

            }

            if (aPTypeID == (int)Util.ApprovalType.ExternalAudit)
            {
                if (GroupID == (int)Util.MailGroupSetup.ExternalAuditInitiatedMail || GroupID == (int)Util.MailGroupSetup.ExternalAuditInitiatedMailDepartmentWise)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department);
                }
                else if (GroupID == (int)Util.MailGroupSetup.FinalExternalAuditApprovalStatusToInitiator)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.ExternalAuditForwardedOrRejectionMail)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
                else if (GroupID == (int)Util.MailGroupSetup.ExternalAuditForwardFeedbackReceieve)
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["DocType"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }

            }

            if (aPTypeID == (int)Util.ApprovalType.EmailNotification)
            {

                string EmployeeName = string.Empty, LeaveType = string.Empty, NoOfLeaveDays = string.Empty, DateRange = string.Empty;
                if (collection.Count > 0)
                {
                    EmployeeName = collection[0]["EmployeeName"].ToString();
                }
                mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeName}}", EmployeeName).Replace("{{LeaveType}}", LeaveType).Replace("{{NoOfLeaveDays}}", NoOfLeaveDays).Replace("{{Date}}", DateRange);

            }

            if (
               aPTypeID == (int)Util.ApprovalType.IOUClaim
            || aPTypeID == (int)Util.ApprovalType.ExpenseClaim
            || aPTypeID == (int)Util.ApprovalType.IOUPayment
            || aPTypeID == (int)Util.ApprovalType.ExpensePayment
            || aPTypeID == (int)Util.ApprovalType.PR
            || aPTypeID == (int)Util.ApprovalType.PO
            || aPTypeID == (int)Util.ApprovalType.GRN
            //|| aPTypeID == (int)Util.ApprovalType.DocumentApproval
            || aPTypeID == (int)Util.ApprovalType.QC
            || aPTypeID == (int)Util.ApprovalType.MR
            || aPTypeID == (int)Util.ApprovalType.TaxationVetting
            || aPTypeID == (int)Util.ApprovalType.Invoice
            || aPTypeID == (int)Util.ApprovalType.InvoicePayment
            || aPTypeID == (int)Util.ApprovalType.TaxationVettingPayment
            || aPTypeID == (int)Util.ApprovalType.ExitInterview
            || aPTypeID == (int)Util.ApprovalType.AccessDeactivation
            || aPTypeID == (int)Util.ApprovalType.DivisionClearance
            || aPTypeID == (int)Util.ApprovalType.SCC
            || aPTypeID == (int)Util.ApprovalType.PettyCashExpenseClaim
            || aPTypeID == (int)Util.ApprovalType.PettyCashAdvanceClaim
            || aPTypeID == (int)Util.ApprovalType.PettyCashAdvanceResubmitClaim
            || aPTypeID == (int)Util.ApprovalType.PettyCashReimburseClaim
            || aPTypeID == (int)Util.ApprovalType.PettyCashPaymentClaim
            || aPTypeID == (int)Util.ApprovalType.SupportRequisition
            //|| aPTypeID == (int)Util.ApprovalType.EmployeeeDocumentUpload
            )
            {
                if (GroupID == (int)Util.MailGroupSetup.IOUClaimInitiatedMail || GroupID == (int)Util.MailGroupSetup.ExpenseClaimInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.IOUPaymentInitiatedMail || GroupID == (int)Util.MailGroupSetup.ExpensePaymentInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.PRInitiatedMail || GroupID == (int)Util.MailGroupSetup.POInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.GRNInitiatedMail
                    //|| GroupID == (int)Util.MailGroupSetup.DocumentApprovalInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.QCInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.MRInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.InvoiceInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.InvoicePaymentInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.TaxationVettingInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.TaxationPaymentInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.ExitInterviewInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.EmployeeAccessDeactivationInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.DivisionClearenceInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.SCCInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.PettyCashExpenseClaimInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.PettyCashAdvanceClaimInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.PettyCashAdvanceResubmitClaimInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.PettyCashReimburseClaimInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.PettyCashPaymentInitiatedMail
                    || GroupID == (int)Util.MailGroupSetup.RequisitionSupportInitiatedMail
                    //|| GroupID == (int)Util.MailGroupSetup.EmployeeDocumentUploadInitiatedMail
                    )
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).Replace("{{Division}}", Division).Replace("{{Department}}", Department);
                }
                else if (
                       GroupID == (int)Util.MailGroupSetup.FinalIOUCliamApprovalStatusToInitiator || GroupID == (int)Util.MailGroupSetup.IOUClaimForwardedOrRejectionMail
                       || GroupID == (int)Util.MailGroupSetup.IOUClaimForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalExpenseCliamApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.ExpenseClaimForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.ExpenseClaimForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalExpensePaymentApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.ExpensePaymentForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.ExpensePaymentForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalIOUPaymentApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.IOUPaymentForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.IOUPaymentForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalPRApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.PRForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.PRForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalPOApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.POForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.POForwardFeedbackReceieve
                    || GroupID == (int)Util.MailGroupSetup.SendMailToSCMGroupAfterPRApproved

                    || GroupID == (int)Util.MailGroupSetup.FinalGRNApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.GRNForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.GRNForwardFeedbackReceieve

                    //|| GroupID == (int)Util.MailGroupSetup.FinalDocumentApprovalApprovalStatusToInitiator
                    //|| GroupID == (int)Util.MailGroupSetup.DocumentApprovalForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.DocumentApprovalForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalQCApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.QCForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.QCForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalInvoiceApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.InvoiceForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.InvoiceForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalInvoicePaymentApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.InvoicePaymentForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.InvoicePaymentForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalMRApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.MRForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.PRForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalTaxationVettingApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.TaxationVettingForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.TaxationVettingForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalTaxationPaymentApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.TaxationPaymentForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.TaxationPaymentForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalExitInterviewApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.ExitInterviewForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.ExitInterviewForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalEmployeeAccessDeactivationApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.EmployeeAccessDeactivationForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.EmployeeAccessDeactivationForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalDivisionClearenceApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.DivisionClearenceForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.DivisionClearenceForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalSCCApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.SCCForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.SCCForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalPettyCashExpenseCliamApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.PettyCashExpenseClaimForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.PettyCashExpenseClaimForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalPettyCashAdvanceClaimApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.PettyCashAdvanceClaimForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.PettyCashAdvanceClaimForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalPettyCashAdvanceResubmitClaimApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.PettyCashAdvanceResubmitClaimForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.PettyCashAdvanceResubmitClaimForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalPettyCashReimburseClaimApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.PettyCashReimburseClaimForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.PettyCashReimburseClaimForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalPettyCashPaymentApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.PettyCashPaymentForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.PettyCashPaymentForwardFeedbackReceieve

                    || GroupID == (int)Util.MailGroupSetup.FinalRequisitionSupportApprovalStatusToInitiator
                    || GroupID == (int)Util.MailGroupSetup.RequisitionSupportForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.RequisitionSupportForwardFeedbackReceieve


                    //|| GroupID == (int)Util.MailGroupSetup.FinalEmployeeDocumentUploadApprovalStatusToInitiator
                    //|| GroupID == (int)Util.MailGroupSetup.EmployeeDocumentUploadForwardedOrRejectionMail || GroupID == (int)Util.MailGroupSetup.EmployeeDocumentUploadForwardFeedbackReceieve


                    )
                {
                    string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty, Division = string.Empty, Department = string.Empty, Status = string.Empty;
                    if (collection.Count > 0)
                    {
                        EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                        ReferenceNo = collection[0]["ReferenceNo"].ToString();
                        Division = collection[0]["DivisionName"].ToString();
                        Department = collection[0]["DepartmentName"].ToString();
                        Status = Util.ToEnumString((Util.ApprovalFeedback)(APFeedbackID));
                    }
                    mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo).
                        Replace("{{Division}}", Division).Replace("{{Department}}", Department).Replace("{{Status}}", Status);
                }
            }
            return mailSubject;
        }

        public static async Task<Tuple<string, string>> GetAPEmployeeEmailsWithProxy(int ApprovalProcessID, int employeeType = 0)
        {
            string mail = string.Empty, proxyMail = string.Empty, sql = string.Empty;

            if (employeeType == 0)
            {
                sql = $@"SELECT TOP 1 ---A.EmployeeID ,
                                B.WorkEmail Mail,
                                --A.ProxyEmployeeID,
                                ProxyE.WorkEmail ProxyMail
                            FROM 
	                            Approval..ApprovalEmployeeFeedback A
	                            INNER JOIN HRMS..ViewALLEmployee B ON A.EmployeeID = B.EmployeeID
	                            LEFT JOIN HRMS..ViewALLEmployee ProxyE ON A.ProxyEmployeeID = ProxyE.EmployeeID
	                            WHERE ApprovalProcessID = {ApprovalProcessID} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}";
            }
            else
            {
                sql = $@"SELECT TOP 1 ---A.EmployeeID ,
                                B.WorkEmail Mail,
                                --A.ProxyEmployeeID,
                                ProxyE.WorkEmail ProxyMail
                            FROM 
	                            Approval..ApprovalEmployeeFeedback A
	                            INNER JOIN HRMS..ViewALLEmployee B ON A.EmployeeID = B.EmployeeID
	                            LEFT JOIN HRMS..ViewALLEmployee ProxyE ON A.ProxyEmployeeID = ProxyE.EmployeeID
	                            WHERE ApprovalProcessID = {ApprovalProcessID} AND SequenceNo = 2";
            }

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetData(sql);

            if (model.Count > 0)
            {
                mail = model["Mail"].ToString();
                proxyMail = model["ProxyMail"].ToString();

            }
            return await Task.FromResult(Tuple.Create(mail, proxyMail));
        }

        public static async Task<(string, List<string>)> GetAPEmployeeEmailsWithMultiProxy(int ApprovalProcessID)
        {
            string mail = string.Empty, sql = string.Empty;
            var proxyMail = new List<string>();

            sql = $@"SELECT 
                                B.WorkEmail Mail,
                                ISNULL(ProxyE.WorkEmail,ProxE.WorkEmail) ProxyMail
                            FROM 
	                            Approval..ApprovalEmployeeFeedback A
								LEFT JOIN 
								(
									SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM Approval..ApprovalMultiProxyEmployeeInfo
								) Prox ON Prox.ApprovalProcessID = A.ApprovalProcessID AND Prox.APEmployeeFeedbackID = A.APEmployeeFeedbackID
	                            INNER JOIN HRMS..ViewALLEmployee B ON A.EmployeeID = B.EmployeeID
	                            LEFT JOIN HRMS..ViewALLEmployee ProxyE ON A.ProxyEmployeeID = ProxyE.EmployeeID
								LEFT JOIN HRMS..ViewALLEmployee ProxE ON Prox.EmployeeID = ProxE.EmployeeID
	                            WHERE A.ApprovalProcessID = {ApprovalProcessID} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetDataDictCollection(sql);

            if (model.Count() > 0)
            {
                mail = model.ToList()[0]["Mail"].ToString();
                model.ToList().ForEach(x =>
                {
                    proxyMail.Add(x["ProxyMail"].ToString());
                });
            }
            return await Task.FromResult((mail, proxyMail.Distinct().ToList()));
        }

        public static async Task<(List<string>, List<string>)> GetAPEmployeeEmailsWithMultiProxyAndSupervisorWithDepartment(int ApprovalProcessID, int DepartmentID)
        {
            //string mail = string.Empty;
            string sql = string.Empty;
            var mail = new List<string>();
            var proxyMail = new List<string>();
            sql = $@"SELECT 
                                     B.WorkEmail Mail,  B.DepartmentID,
									 B.DepartmentName,
                                     ISNULL(ProxyE.WorkEmail,ProxE.WorkEmail) ProxyMail
                                 FROM 
                                  Approval..ApprovalEmployeeFeedback A
            	LEFT JOIN 
            	(
            		SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM Approval..ApprovalMultiProxyEmployeeInfo
            	) Prox ON Prox.ApprovalProcessID = A.ApprovalProcessID AND Prox.APEmployeeFeedbackID = A.APEmployeeFeedbackID
                                  INNER JOIN HRMS..ViewALLEmployee B ON A.EmployeeID = B.EmployeeID AND B.DepartmentID = {DepartmentID}
                                  LEFT JOIN HRMS..ViewALLEmployee ProxyE ON A.ProxyEmployeeID = ProxyE.EmployeeID
            	LEFT JOIN HRMS..ViewALLEmployee ProxE ON Prox.EmployeeID = ProxE.EmployeeID
                                  WHERE A.ApprovalProcessID = {ApprovalProcessID} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
                        UNION ALL 
						SELECT '' Mail, B.DepartmentID,
									 B.DepartmentName,
								ISNULL(B.WorkEmail,'') ProxyMail
							FROM    
							Approval..ApprovalEmployeeFeedback A
							INNER JOIN HRMS..ViewALLEmployee B ON A.EmployeeID = B.EmployeeID AND B.DepartmentID = {DepartmentID}
							where  A.ApprovalProcessID = {ApprovalProcessID} and SequenceNo = 1
                        UNION ALL 
						     select top 1 config.DepartmentEmails Mail, dept.DepartmentID,dept.DepartmentName, '' ProxyMail
								from HRMS..AuditApprovalConfig config 
								LEFT JOIN HRMS..Department dept ON dept.DepartmentID = {DepartmentID}
								where DepartmentIDs = '{DepartmentID}'";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetDataDictCollection(sql);

            if (model.Count() > 0)
            {
                //mail = model.ToList()[0]["Mail"].ToString();
                model.ToList().ForEach(x =>
                {
                    mail.Add(x["Mail"].ToString());
                });
                model.ToList().ForEach(x =>
                {
                    proxyMail.Add(x["ProxyMail"].ToString());
                });
            }
            return await Task.FromResult((mail.Distinct().ToList(), proxyMail.Distinct().ToList()));
        }

        public static async Task<(List<string>, List<string>)> GetAPEmployeeEmailsWithMultiProxyAndSupervisor(int ApprovalProcessID)
        {
            //string mail = string.Empty;
            string sql = string.Empty;
            var mail = new List<string>();
            var proxyMail = new List<string>();

            //sql = $@"SELECT 
            //                         B.WorkEmail Mail,
            //                         ISNULL(ProxyE.WorkEmail,ProxE.WorkEmail) ProxyMail
            //                     FROM 
            //                      Approval..ApprovalEmployeeFeedback A
            //	LEFT JOIN 
            //	(
            //		SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM Approval..ApprovalMultiProxyEmployeeInfo
            //	) Prox ON Prox.ApprovalProcessID = A.ApprovalProcessID AND Prox.APEmployeeFeedbackID = A.APEmployeeFeedbackID
            //                      INNER JOIN HRMS..ViewALLEmployee B ON A.EmployeeID = B.EmployeeID
            //                      LEFT JOIN HRMS..ViewALLEmployee ProxyE ON A.ProxyEmployeeID = ProxyE.EmployeeID
            //	LEFT JOIN HRMS..ViewALLEmployee ProxE ON Prox.EmployeeID = ProxE.EmployeeID
            //                      WHERE A.ApprovalProcessID = {ApprovalProcessID} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}

            //                     UNION ALL

            //SELECT  sup.SupervisorEmail Mail,
            //	'' ProxyMail FROM 
            //	Approval..ApprovalEmployeeFeedback A
            //	LEFT JOIN HRMS..ViewEmployeeSupervisorMap SUP ON SUP.EmployeeID = A.EmployeeID
            //	where ApprovalProcessID = {ApprovalProcessID}
            //	and SequenceNo = 1";

            //         sql = $@"SELECT 
            //                                  B.WorkEmail Mail,
            //                                  ISNULL(ProxyE.WorkEmail,ProxE.WorkEmail) ProxyMail
            //                              FROM 
            //                               Approval..ApprovalEmployeeFeedback A
            //         	LEFT JOIN 
            //         	(
            //         		SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM Approval..ApprovalMultiProxyEmployeeInfo
            //         	) Prox ON Prox.ApprovalProcessID = A.ApprovalProcessID AND Prox.APEmployeeFeedbackID = A.APEmployeeFeedbackID
            //                               INNER JOIN HRMS..ViewALLEmployee B ON A.EmployeeID = B.EmployeeID
            //                               LEFT JOIN HRMS..ViewALLEmployee ProxyE ON A.ProxyEmployeeID = ProxyE.EmployeeID
            //         	LEFT JOIN HRMS..ViewALLEmployee ProxE ON Prox.EmployeeID = ProxE.EmployeeID
            //                               WHERE A.ApprovalProcessID = {ApprovalProcessID} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}

            //                             UNION ALL

            //SELECT ISNULL(SUPEmp.SupervisorEmail, sup.SupervisorEmail) Mail,
            //			ISNULL(OBE.WorkEmail, '') ProxyMail 
            //		FROM 
            //			Approval..ApprovalEmployeeFeedback A
            //			LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = A.ApprovalProcessID
            //			LEFT JOIN HRMS..RequestSupportMaster RSM ON RSM.RSMID = AP.ReferenceID
            //			LEFT JOIN HRMS..ViewALLEmployee OBE ON OBE.EmployeeID = RSM.EmployeeID
            //			LEFT JOIN HRMS..ViewEmployeeSupervisorMap SUP ON SUP.EmployeeID = A.EmployeeID
            //                     LEFT JOIN HRMS..ViewEmployeeSupervisorMap SUPEmp ON SUPEmp.EmployeeID = RSM.EmployeeID
            //			where  A.ApprovalProcessID = {ApprovalProcessID}
            //			and SequenceNo = 1

            //                     UNION ALL 
            //			SELECT '' Mail,
            //					ISNULL(B.WorkEmail,'') ProxyMail
            //				FROM 
            //				Approval..ApprovalEmployeeFeedback A
            //				INNER JOIN HRMS..ViewALLEmployee B ON A.EmployeeID = B.EmployeeID
            //				where  A.ApprovalProcessID = {ApprovalProcessID} and SequenceNo = 1";

            sql = $@"SELECT 
                                     B.WorkEmail Mail,
                                     ISNULL(ProxyE.WorkEmail,ProxE.WorkEmail) ProxyMail
                                 FROM 
                                  Approval..ApprovalEmployeeFeedback A
            	LEFT JOIN 
            	(
            		SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM Approval..ApprovalMultiProxyEmployeeInfo
            	) Prox ON Prox.ApprovalProcessID = A.ApprovalProcessID AND Prox.APEmployeeFeedbackID = A.APEmployeeFeedbackID
                                  INNER JOIN HRMS..ViewALLEmployee B ON A.EmployeeID = B.EmployeeID
                                  LEFT JOIN HRMS..ViewALLEmployee ProxyE ON A.ProxyEmployeeID = ProxyE.EmployeeID
            	LEFT JOIN HRMS..ViewALLEmployee ProxE ON Prox.EmployeeID = ProxE.EmployeeID
                                  WHERE A.ApprovalProcessID = {ApprovalProcessID} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
                        UNION ALL 
						SELECT '' Mail,
								ISNULL(B.WorkEmail,'') ProxyMail
							FROM    
							Approval..ApprovalEmployeeFeedback A
							INNER JOIN HRMS..ViewALLEmployee B ON A.EmployeeID = B.EmployeeID
							where  A.ApprovalProcessID = {ApprovalProcessID} and SequenceNo = 1";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetDataDictCollection(sql);

            if (model.Count() > 0)
            {
                //mail = model.ToList()[0]["Mail"].ToString();
                model.ToList().ForEach(x =>
                {
                    mail.Add(x["Mail"].ToString());
                });
                model.ToList().ForEach(x =>
                {
                    proxyMail.Add(x["ProxyMail"].ToString());
                });
            }
            return await Task.FromResult((mail.Distinct().ToList(), proxyMail.Distinct().ToList()));
        }


        public static async Task<(List<string>, List<string>)> GetAPEmployeeEmailsWithMultiProxyParallal(int ApprovalProcessID)
        {
            var mail = new List<string>();
            string sql = string.Empty;
            //string mail = string.Empty, sql = string.Empty;
            var proxyMail = new List<string>();
            sql = $@"SELECT 
                                B.WorkEmail Mail,
                                ISNULL(ProxyE.WorkEmail,ProxE.WorkEmail) ProxyMail
                            FROM 
	                            Approval..ApprovalEmployeeFeedback A
								LEFT JOIN 
								(
									SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM Approval..ApprovalMultiProxyEmployeeInfo
								) Prox ON Prox.ApprovalProcessID = A.ApprovalProcessID AND Prox.APEmployeeFeedbackID = A.APEmployeeFeedbackID
	                            INNER JOIN HRMS..ViewALLEmployee B ON A.EmployeeID = B.EmployeeID
	                            LEFT JOIN HRMS..ViewALLEmployee ProxyE ON A.ProxyEmployeeID = ProxyE.EmployeeID
								LEFT JOIN HRMS..ViewALLEmployee ProxE ON Prox.EmployeeID = ProxE.EmployeeID
	                            WHERE A.ApprovalProcessID = {ApprovalProcessID} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetDataDictCollection(sql);

            if (model.Count() > 0)
            {
                model.ToList().ForEach(x =>
                {
                    mail.Add(x["Mail"].ToString());
                });
                model.ToList().ForEach(x =>
                {
                    proxyMail.Add(x["ProxyMail"].ToString());
                });
            }
            return await Task.FromResult((mail.Distinct().ToList(), proxyMail.Distinct().ToList()));
        }
        public static async Task<List<string>> GetInitiatorEmployeeEmail(int ApprovalProcessID)
        {
            var mail = new List<string>();
            string sql = string.Empty;
            //string mail = string.Empty, sql = string.Empty;
            var proxyMail = new List<string>();
            sql = $@"SELECT TOP 1
                                B.WorkEmail Mail
                            FROM 
	                            Approval..ApprovalEmployeeFeedback A
								INNER JOIN HRMS..ViewALLEmployee B ON A.EmployeeID = B.EmployeeID
	                            WHERE A.ApprovalProcessID = {ApprovalProcessID} and SequenceNo = 1
								ORDER BY SequenceNo ASC";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetDataDictCollection(sql);

            if (model.Count() > 0)
            {
                model.ToList().ForEach(x =>
                {
                    mail.Add(x["Mail"].ToString());
                });

            }
            return await Task.FromResult((mail.Distinct().ToList()));
        }

        public static async Task<(List<string>, List<string>)> GetAPEmployeeEmailsForAllPanelMembers(int ApprovalProcessID)
        {
            var mail = new List<string>();
            string sql = string.Empty;
            //string mail = string.Empty, sql = string.Empty;
            var proxyMail = new List<string>();
            //sql = $@"SELECT 
            //                         B.WorkEmail Mail,
            //                         ISNULL(ProxyE.WorkEmail,ISNULL(ProxE.WorkEmail,'')) ProxyMail

            //                     FROM 
            //                      Approval..ApprovalEmployeeFeedback A
            //	LEFT JOIN 
            //	(
            //		SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM Approval..ApprovalMultiProxyEmployeeInfo
            //	) Prox ON Prox.ApprovalProcessID = A.ApprovalProcessID AND Prox.APEmployeeFeedbackID = A.APEmployeeFeedbackID
            //                      INNER JOIN HRMS..ViewALLEmployee B ON A.EmployeeID = B.EmployeeID
            //                      LEFT JOIN HRMS..ViewALLEmployee ProxyE ON A.ProxyEmployeeID = ProxyE.EmployeeID
            //	LEFT JOIN HRMS..ViewALLEmployee ProxE ON Prox.EmployeeID = ProxE.EmployeeID
            //                      WHERE A.ApprovalProcessID = {ApprovalProcessID}

            //           UNION ALL

            //SELECT  sup.SupervisorEmail Mail,
            //	'' ProxyMail FROM 

            //	Approval..ApprovalEmployeeFeedback A
            //	--LEFT JOIN HRMS..ViewALLEmployee VA ON VA.EmployeeID = A.EmployeeID
            //	LEFT JOIN HRMS..ViewEmployeeSupervisorMap SUP ON SUP.EmployeeID = A.EmployeeID
            //	where ApprovalProcessID = {ApprovalProcessID} 
            //	and SequenceNo = 1                                
            //         ";

            sql = $@"SELECT 
                                     B.WorkEmail Mail,
                                     ISNULL(ProxyE.WorkEmail,ISNULL(ProxE.WorkEmail,'')) ProxyMail

                                 FROM 
                                  Approval..ApprovalEmployeeFeedback A
            	                LEFT JOIN 
            	                (
            		                SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM Approval..ApprovalMultiProxyEmployeeInfo
            	                ) Prox ON Prox.ApprovalProcessID = A.ApprovalProcessID AND Prox.APEmployeeFeedbackID = A.APEmployeeFeedbackID
                                                  INNER JOIN HRMS..ViewALLEmployee B ON A.EmployeeID = B.EmployeeID
                                                  LEFT JOIN HRMS..ViewALLEmployee ProxyE ON A.ProxyEmployeeID = ProxyE.EmployeeID
            	                LEFT JOIN HRMS..ViewALLEmployee ProxE ON Prox.EmployeeID = ProxE.EmployeeID
                                                  WHERE A.ApprovalProcessID = {ApprovalProcessID}

                                       UNION ALL


					SELECT  
                                ISNULL(SUPEmp.SupervisorEmail, sup.SupervisorEmail) Mail,
								ISNULL(OBE.WorkEmail, '') ProxyMail 
                            FROM 
								Approval..ApprovalEmployeeFeedback A
                                LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = A.ApprovalProcessID
								LEFT JOIN HRMS..RequestSupportMaster RSM ON RSM.RSMID = AP.ReferenceID
								LEFT JOIN HRMS..ViewALLEmployee OBE ON OBE.EmployeeID = RSM.EmployeeID
                                LEFT JOIN HRMS..ViewEmployeeSupervisorMap SUP ON SUP.EmployeeID = A.EmployeeID
								LEFT JOIN HRMS..ViewEmployeeSupervisorMap SUPEmp ON SUPEmp.EmployeeID = RSM.EmployeeID
								where  A.ApprovalProcessID = {ApprovalProcessID}
								and SequenceNo = 1
                ";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetDataDictCollection(sql);

            if (model.Count() > 0)
            {
                model.ToList().ForEach(x =>
                {
                    mail.Add(x["Mail"].ToString());
                });
                model.ToList().ForEach(x =>
                {
                    proxyMail.Add(x["ProxyMail"].ToString());
                });
            }
            return await Task.FromResult((mail.Distinct().ToList(), proxyMail.Distinct().ToList()));
        }

        public static async Task<(List<string>, List<string>)> GetAPEmployeeEmailsForRejectPanelMembers(int ApprovalProcessID)
        {
            var mail = new List<string>();
            string sql = string.Empty;
            var proxyMail = new List<string>();

            sql = $@"SELECT 
                                B.WorkEmail Mail,
                                ISNULL(ProxyE.WorkEmail,ISNULL(ProxE.WorkEmail,'')) ProxyMail
								
                            FROM 
	                            Approval..ApprovalEmployeeFeedback A
								LEFT JOIN 
								(
									SELECT ApprovalProcessID,EmployeeID,APEmployeeFeedbackID FROM Approval..ApprovalMultiProxyEmployeeInfo
								) Prox ON Prox.ApprovalProcessID = A.ApprovalProcessID AND Prox.APEmployeeFeedbackID = A.APEmployeeFeedbackID
	                            INNER JOIN HRMS..ViewALLEmployee B ON A.EmployeeID = B.EmployeeID
	                            LEFT JOIN HRMS..ViewALLEmployee ProxyE ON A.ProxyEmployeeID = ProxyE.EmployeeID
								LEFT JOIN HRMS..ViewALLEmployee ProxE ON Prox.EmployeeID = ProxE.EmployeeID
	                            WHERE A.ApprovalProcessID = {ApprovalProcessID}

                    UNION ALL

					SELECT  
                                OBE.WorkEmail Mail,
								ISNULL(SUP.SupervisorEmail, '') ProxyMail 
                            FROM 
								Approval..ApprovalEmployeeFeedback A
                                LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = A.ApprovalProcessID
								LEFT JOIN HRMS..RequestSupportMaster RSM ON RSM.RSMID = AP.ReferenceID
								LEFT JOIN HRMS..ViewALLEmployee OBE ON OBE.EmployeeID = RSM.EmployeeID
                                LEFT JOIN HRMS..ViewEmployeeSupervisorMap SUP ON SUP.EmployeeID = RSM.EmployeeID
								where  A.ApprovalProcessID = {ApprovalProcessID}
								and SequenceNo = 1
                ";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetDataDictCollection(sql);

            if (model.Count() > 0)
            {
                model.ToList().ForEach(x =>
                {
                    mail.Add(x["Mail"].ToString());
                });
                model.ToList().ForEach(x =>
                {
                    proxyMail.Add(x["ProxyMail"].ToString());
                });
            }
            return await Task.FromResult((mail.Distinct().ToList(), proxyMail.Distinct().ToList()));
        }

        public static async Task<Tuple<string, string>> GetApprovalActionEmployeesForEmail(int ApprovalProcessID, int APEmployeeFeedbackID, int APFeedbackID, int ToAPEmployeeFeedbackID)
        {

            string mail = string.Empty, proxyMail = string.Empty;
            string sql = $@" EXEC Approval..ApprovalActionEmployeesForEmail {ApprovalProcessID},{APEmployeeFeedbackID},{APFeedbackID},{ToAPEmployeeFeedbackID}";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetData(sql);

            if (model.Count > 0)
            {
                mail = model["EmployeeMail"].ToString();
                proxyMail = model["ProxyEmployeeMail"].ToString();

            }
            return await Task.FromResult(Tuple.Create(mail, proxyMail));
        }

        public static async Task<(List<string>, List<string>)> GetApprovalActionEmployeesForMultiProxyEmail(int ApprovalProcessID, int APEmployeeFeedbackID, int APFeedbackID, int ToAPEmployeeFeedbackID)
        {

            var mail = new List<string>();
            var proxyMail = new List<string>();
            string sql = $@" EXEC Approval..ApprovalActionEmployeesForEmail {ApprovalProcessID},{APEmployeeFeedbackID},{APFeedbackID},{ToAPEmployeeFeedbackID}";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetDataDictCollection(sql);

            if (model.Count() > 0)
            {
                model.ToList().ForEach(x =>
                {
                    mail.Add(x["EmployeeMail"].ToString());
                });
                model.ToList().ForEach(x =>
                {
                    proxyMail.Add(x["ProxyEmployeeMail"].ToString());
                });

            }
            return await Task.FromResult((mail.Distinct().ToList(), proxyMail.Distinct().ToList()));
        }

        public static async Task<string> GetBackupEmployeeMail(int employeeLeaveAID)
        {
            string mail = string.Empty;
            string sql = $@"SELECT 	
	                            FullName,
	                            WorkEmail
                            FROM 
	                            HRMS..EmployeeLeaveApplication A
	                            INNER JOIN HRMS..ViewALLEmployee B ON A.BackupEmployeeID = B.EmployeeID
                            WHERE EmployeeLeaveAID = {employeeLeaveAID}";
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetData(sql);

            if (model.Count > 0)
            {
                mail = model["WorkEmail"].ToString();

            }
            return await Task.FromResult(mail);
        }

        public static string SaveSingleAttachment(int FUID, string FilePath, string FileName, string FileType, string OriginalName, int ReferenceID, string TableName, bool IsDeleted, decimal Size, int ParentFUID = 0, bool IsFolder = false, string description = "", bool IsUpdated = false)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SaveSingleAttachment @0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15", FUID, FilePath, FileName, FileType, OriginalName, ReferenceID, TableName, ParentFUID, IsFolder, AppContexts.User.CompanyID, AppContexts.User.UserID, AppContexts.User.IPAddress, IsDeleted, Size, description, IsUpdated);

            return result.ToString();
        }
        public static string SaveSingleAttachmentOracle(int FUID, string FilePath, string FileName, string FileType, string OriginalName, int ReferenceID, string TableName, bool IsDeleted, decimal Size, int ParentFUID = 0, bool IsFolder = false, string description = "", bool IsUpdated = false)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SaveSingleAttachment @0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15", FUID, FilePath, FileName, FileType, OriginalName, ReferenceID, TableName, ParentFUID, IsFolder, AppContexts.User.CompanyID, AppContexts.User.UserID, AppContexts.User.IPAddress, IsDeleted, Size, description, IsUpdated);

            return result.ToString();
        }

        private static string GenerateBasicMailBody(MailGroupSetupDtoCore mailGroupSetup, bool IsResubmitted, int GroupID, List<Dictionary<string, object>> collection = null)
        {
            var mailBody = string.Empty;
            if (GroupID == (int)Util.MailGroupSetup.ForgotPasswordRequest)
            {
                string EmployeeName = string.Empty, Token = string.Empty;
                if (collection.Count > 0)
                {
                    EmployeeName = collection[0]["EmployeeName"].ToString();
                    Token = collection[0]["Token"].ToString();
                }
                mailBody = mailGroupSetup.Body.Replace("{{EmployeeName}}", EmployeeName).Replace("{{Token}}", Token);
            }
            else if (GroupID == (int)Util.MailGroupSetup.NFARemoveMail)
            {
                string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty;
                if (collection.Count > 0)
                {
                    EmployeeNameAndCode = $@"{AppContexts.User.EmployeeCode} - {AppContexts.User.FullName}";
                    ReferenceNo = collection[0]["ReferenceNo"].ToString();
                }
                mailBody = mailGroupSetup.Body.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo);
            }
            if (GroupID == (int)Util.MailGroupSetup.RemoteAttendacneCheckInOutRequest)
            {
                string EmployeeName = string.Empty, SupervisorName = string.Empty, RequestType = string.Empty, AttendanceDate = String.Empty;
                if (collection.Count > 0)
                {
                    EmployeeName = collection[0]["EmployeeName"].ToString();
                    SupervisorName = collection[0]["SupervisorName"].ToString();
                    RequestType = collection[0]["RequestType"].ToString();
                    AttendanceDate = collection[0]["AttendanceDate"].ToString();
                }
                mailBody = mailGroupSetup.Body.Replace("{{EmployeeName}}", EmployeeName).Replace("{{SupervisorName}}", SupervisorName).Replace("{{RequestType}}", RequestType).Replace("{{AttendanceDate}}", AttendanceDate);
            }
            if (GroupID == (int)Util.MailGroupSetup.RemoteAttendacneAcceptOrRejectMail)
            {
                string EmployeeName = string.Empty, SupervisorName = string.Empty, RequestType = string.Empty, Status = string.Empty, AttendanceDate = String.Empty;
                if (collection.Count > 0)
                {
                    EmployeeName = collection[0]["EmployeeName"].ToString();
                    SupervisorName = collection[0]["SupervisorName"].ToString();
                    RequestType = collection[0]["RequestType"].ToString();
                    Status = collection[0]["Status"].ToString();
                    AttendanceDate = collection[0]["AttendanceDate"].ToString();
                }
                mailBody = mailGroupSetup.Body.Replace("{{EmployeeName}}", EmployeeName).Replace("{{SupervisorName}}", SupervisorName).Replace("{{RequestType}}", RequestType)
                    .Replace("{{Status}}", Status).Replace("{{AttendanceDate}}", AttendanceDate);
            }
            if (GroupID == (int)Util.MailGroupSetup.OTPMessageBody)
            {
                string OTP = string.Empty, Token = string.Empty;
                if (collection.Count > 0)
                {
                    OTP = collection[0]["OTP"].ToString();
                }
                mailBody = mailGroupSetup.Body.Replace("{{OTP}}", OTP);
            }
            if (GroupID == (int)Util.MailGroupSetup.OTPMessageBodyTax)
            {
                string OTP = string.Empty, Token = string.Empty;
                if (collection.Count > 0)
                {
                    OTP = collection[0]["OTP"].ToString();
                }
                mailBody = mailGroupSetup.Body.Replace("{{OTP}}", OTP);
            }
            if (GroupID == (int)Util.MailGroupSetup.LeaveEncashmentWindowMail)
            {
                string startDate = string.Empty, endDate = string.Empty, financialYear = string.Empty;
                if (collection.Count > 0)
                {
                    startDate = collection[0]["StartDate"].ToString();
                    endDate = collection[0]["EndDate"].ToString();
                    financialYear = collection[0]["FinancialYear"].ToString();
                }
                mailBody = mailGroupSetup.Body.Replace("{{StartDate}}", startDate).Replace("{{EndDate}}", endDate).Replace("{{FinancialYear}}", financialYear);
            }
            return mailBody;
        }

        private static string GenerateBasicMailSubject(MailGroupSetupDtoCore mailGroupSetup, bool IsResubmitted, int GroupID, List<Dictionary<string, object>> collection = null)
        {
            var mailSubject = string.Empty;

            if (GroupID == (int)Util.MailGroupSetup.ForgotPasswordRequest)
            {
                mailSubject = mailGroupSetup.Subject;
            }
            else if (GroupID == (int)Util.MailGroupSetup.RemoteAttendacneCheckInOutRequest || GroupID == (int)Util.MailGroupSetup.RemoteAttendacneAcceptOrRejectMail)
            {
                mailSubject = mailGroupSetup.Subject.Replace("{{AttendanceDate}}", collection[0]["AttendanceDate"].ToString());
            }
            else if (GroupID == (int)Util.MailGroupSetup.NFARemoveMail)
            {
                string EmployeeNameAndCode = string.Empty, ReferenceNo = string.Empty;
                if (collection.Count > 0)
                {
                    EmployeeNameAndCode = collection[0]["EmployeeName"].ToString();
                    ReferenceNo = collection[0]["ReferenceNo"].ToString();
                }
                mailSubject = mailGroupSetup.Subject.Replace("{{EmployeeNameAndCode}}", EmployeeNameAndCode).Replace("{{ReferenceNo}}", ReferenceNo);
            }
            else if (GroupID == (int)Util.MailGroupSetup.OTPMessageBody)
            {
                mailSubject = mailGroupSetup.Subject;
            }
            else if (GroupID == (int)Util.MailGroupSetup.OTPMessageBodyTax)
            {
                mailSubject = mailGroupSetup.Subject;
            }
            else if (GroupID == (int)Util.MailGroupSetup.LeaveEncashmentWindowMail)
            {
                mailSubject = mailGroupSetup.Subject;
            }
            return mailSubject;
        }

        public static string InsertRemoteAttendance(int RAID)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"attendanceService..InsertRemoteAttenance @0", RAID);

            return result.ToString();
        }
        public static string UpdateAttendanceSummaryByEmployeeIdAndDate(DateTime attendanceDate, int employeeId)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"HRMS..AttendanceSummaryScheduleWithEmpIDAndDate @0,@1", attendanceDate, employeeId);

            return result.ToString();
        }

        public static async Task<List<Dictionary<string, object>>> GetListOfDictionaryWithSql(string sql)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetDataDictCollectionWithTransaction(sql);
            return await Task.FromResult(model.ToList());
        }

        public static async Task<Dictionary<string, object>> GetDictionaryWithSql(string sql)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetDataDicWithTransaction(sql);
            return await Task.FromResult(model);
        }

        #region Mail

        public async Task SendMailFromManagerBase(string ApprovalProcessID, bool IsResubmitted, long MasterID, int mailGroup, int APTypeID)
        {
            var mail = GetAPEmployeeEmailsWithMultiProxy(ApprovalProcessID.ToInt()).Result;
            List<string> ToEmailAddress = new List<string>() { mail.Item1 };
            List<string> CCEmailAddress = mail.Item2;//new List<string>() { mail.Item2 };

            var count = ToEmailAddress.Where(x => x.IsNotNullOrEmpty()).ToList().Count();
            if (count > 0)
                await ApprovalProcessMail(ApprovalProcessID.ToInt(), APTypeID, mailGroup, IsResubmitted, ToEmailAddress, CCEmailAddress, null, (int)MasterID, 0);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }

        #endregion

        public static string SaveSCMRequisitionQuotation(int PRQID, int PRMasterID, int SupplierID, decimal Amount, int TaxTypeID, string CompanyID, int CreatedBy, string CreatedIP, int UpdatedBy, DateTime UpdatedDate, string UpdatedIP, int TransactionType, string description, decimal QuotedQty, decimal QuotedUnitPrice, long ItemID, int PRCID)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"SCM..SavePurchaseRequisitionQuotation @0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14,@15,@16", PRQID, PRMasterID, SupplierID, Amount, TaxTypeID, CompanyID, CreatedBy, CreatedIP, UpdatedBy, UpdatedDate, UpdatedIP, TransactionType, description, QuotedQty, QuotedUnitPrice, ItemID, PRCID);

            return result.ToString();
        }
        public static string UpdateMaterialRequisitionChild(int MRCID, int MRMasterID, decimal Price, decimal Amount, string CompanyID, int UserID, string IPAddress, int itemID, string Desctiption)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"SCM..SPUpdateMaterialRequisitionChild @0,@1,@2,@3,@4,@5,@6,@7,@8", MRCID, MRMasterID, Price, Amount, CompanyID, UserID, IPAddress, itemID, Desctiption);

            return result.ToString();
        }
        public static string UpdateMaterialRequisitionMasterSCMRemarks(int MRMasterID, string SCMRemarks, decimal GrandTotal)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"SCM..SPUpdateMaterialRequisitionMaster @0,@1,@2", MRMasterID, SCMRemarks, GrandTotal);

            return result.ToString();
        }
        public static string UpdatePurchaseRequisitionMaster(int PRMasterID, decimal GrandTotal)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"SCM..SPUpdatePurchaseRequisitionMaster @0,@1", PRMasterID, GrandTotal);

            return result.ToString();
        }
        public static string UpdatePRNFAMap(int PRNFAMapID, int PRMID, int NFAID, string NFARefNo, decimal NFAAmount, bool IsFromSystem, string CompanyID, int UserID, string IPAddress)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"SCM..SPUpdatePRNFAMap @0,@1,@2,@3,@4,@5,@6,@7,@8", PRNFAMapID, PRMID, NFAID, NFARefNo, NFAAmount, IsFromSystem, CompanyID, UserID, IPAddress);

            return result.ToString();
        }
        public static string UpdatePurchaseRequisitionChild(int PRCID, int PRMasterID, decimal Price, decimal Amount, string CompanyID, int UserID, string IPAddress, int itemID, string Desctiption)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"SCM..SPUpdatePurchaseRequisitionChild @0,@1,@2,@3,@4,@5,@6,@7,@8", PRCID, PRMasterID, Price, Amount, CompanyID, UserID, IPAddress, itemID, Desctiption);

            return result.ToString();
        }


        public static string UpdateCostCenter(int PRCID, int ForID)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var result = context.ExecuteScalar(@$"SCM..UpdateCostCenter @0,@1,@2,@3",
            PRCID, ForID, AppContexts.User.UserID, AppContexts.User.IPAddress);

            return result.IsNotNull() ? result.ToString() : "";
        }
        public static string UpdateSCCChild(int SCCCID, DateTime? DeliveryOrJobCompletionDate, decimal receiveQty, decimal invoiceAmount, decimal TotalAmount, decimal TotalAmountIncludingVat, decimal VatAmount, string Remarks)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var result = context.ExecuteScalar(@$"SCM..SP_UpdateSCCChild @0,@1,@2,@3,@4,@5,@6,@7,@8,@9",
            SCCCID, DeliveryOrJobCompletionDate, receiveQty, invoiceAmount, TotalAmount, TotalAmountIncludingVat, VatAmount, Remarks ?? "", AppContexts.User.UserID, AppContexts.User.IPAddress);

            return result.IsNotNull() ? result.ToString() : "";
        }
        public static string SavePurchaseRequisitionChildCostCenterBudget(int PRCCCBID, int PRMasterID, int ForID, DateTime? FromDate, DateTime? ToDate, decimal? AllocatedBudgetAmount, decimal? RemainingBudgetAmount, string Note, string CompanyID, int CreatedBy, string CreatedIP, int UpdatedBy, DateTime UpdatedDate, string UpdatedIP, int TransactionType)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var result = context.ExecuteScalar(@$"SCM..SavePurchaseRequisitionChildCostCenterBudget @0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10,@11,@12,@13,@14",
            PRCCCBID, PRMasterID, ForID, FromDate, ToDate, AllocatedBudgetAmount, RemainingBudgetAmount, Note, CompanyID, CreatedBy, CreatedIP, UpdatedBy, UpdatedDate, UpdatedIP, TransactionType);

            return result.IsNotNull() ? result.ToString() : "";
        }

        public async Task SendMailBase(string ApprovalProcessID, int ReferenceId, int APTypeId, List<int> GroupId, List<string> ToEmail = null, List<string> CCEmail = null, int APFeedbackID = 0)
        {
            List<string> ToEmailAddress = new List<string>();
            List<string> CCEmailAddress = new List<string>();
            if (ToEmail.IsNullOrEmpty() || ToEmail.Count().IsZero())
            {
                var mail = GetAPEmployeeEmailsWithMultiProxy(ApprovalProcessID.ToInt()).Result;
                ToEmailAddress = new List<string>() { mail.Item1 };
                CCEmailAddress = mail.Item2;//new List<string>() { mail.Item2 };
            }
            else
            {
                ToEmailAddress = ToEmail;//new List<string>() { ToEmail.Trim() };
                CCEmailAddress = CCEmail;//new List<string>() { CCEmail.Trim() };
            }

            await ApprovalProcessMail(ApprovalProcessID.ToInt(), APTypeId, GroupId, false, ToEmailAddress, CCEmailAddress, null, ReferenceId, APFeedbackID);
            //await SendMailToBackEmployee(ApprovalProcessID, IsResubmitted, EmployeeLeaveAID);
        }

        //public int CheckCurrentAPEmployee(int EmployeeID, int ApprovalProcessID)
        //{
        //    var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

        //    var result = context.ExecuteScalarWithTransaction(@$"Approval..fnValidateCurrentAPEmployee @0,@1", EmployeeID, ApprovalProcessID);

        //    return Convert.ToInt32(result);
        //}

        public static string ChequeBookLeafUpdate(int ChequeBookID, int LeafNo)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"SCM..UpdateChequeBookLeaf @0,@1,@2,@3",
            ChequeBookID, LeafNo, AppContexts.User.UserID, AppContexts.User.IPAddress);

            return result.IsNotNull() ? result.ToString() : "";
        }


        public List<Dictionary<string, object>> GetHashedTokensFromBlackList()
        {
            string commentSql = @$"SELECT * FROM {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..UserTokenBlackList";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var list = context.GetDataDictCollection(commentSql).ToList();
            return list;
        }



        public static async Task<EmailDtoCore> UnauthorizedLeaveMail(int ApprovalProcessID, int APTypeID, int GroupID, List<Dictionary<string, object>> collection, bool IsResubmitted, List<string> ToEmailAddress, List<string> CCEmailAddress = null, List<string> BCCEmailAddress = null,
            int ReferenceID = 0, int APFeedbackID = 0)
        {
            var mailConfigurationDtoCore = await GetMailConfiguration();
            var mailGroupSetup = await MailGroupSetup(GroupID);
            if (mailConfigurationDtoCore.IsNull() || mailGroupSetup.IsNull() || ToEmailAddress.IsNull()) return null;
            //List<Dictionary<string, object>> collection = GetModelData(APTypeID, ReferenceID);

            var client = await SetMailServerConfiguration(mailConfigurationDtoCore);
            var mailSubject = GenerateMailSubject(APTypeID, mailGroupSetup, IsResubmitted, GroupID, collection, APFeedbackID);
            var mailBody = GenerateMailBody(APTypeID, mailGroupSetup, IsResubmitted, GroupID, collection, APFeedbackID);

            EmailDtoCore emailData = new EmailDtoCore
            {
                FromEmailAddress = mailConfigurationDtoCore.UserName,
                FromEmailAddressDisplayName = mailConfigurationDtoCore.DisplayName,
                ToEmailAddress = ToEmailAddress ?? new List<string>() { },
                CCEmailAddress = CCEmailAddress ?? new List<string>() { },
                BCCEmailAddress = BCCEmailAddress ?? new List<string>() { },
                EmailDate = DateTime.Now,
                Subject = mailSubject,
                EmailBody = mailBody,
                IsBodyHtml = true
            };

            return emailData;
        }

        public static async Task UnauthorizedLeaveSentMail(int ApprovalProcessID, int APTypeID, int GroupID, bool IsResubmitted, string mailBody, List<string> ToEmailAddress, List<string> CCEmailAddress = null, List<string> BCCEmailAddress = null,
            int ReferenceID = 0, int APFeedbackID = 0)
        {
            var mailConfigurationDtoCore = await GetMailConfiguration();
            var mailGroupSetup = await MailGroupSetup(GroupID);
            if (mailConfigurationDtoCore.IsNull() || mailGroupSetup.IsNull() || ToEmailAddress.IsNull()) return;
            List<Dictionary<string, object>> collection = GetModelData(APTypeID, ReferenceID);

            var client = await SetMailServerConfiguration(mailConfigurationDtoCore);
            var mailSubject = GenerateMailSubject(APTypeID, mailGroupSetup, IsResubmitted, GroupID, collection, APFeedbackID);
            //var mailBody = GenerateMailBody(APTypeID, mailGroupSetup, IsResubmitted, GroupID, collection, APFeedbackID);

            EmailDtoCore emailData = new EmailDtoCore
            {
                FromEmailAddress = mailConfigurationDtoCore.UserName,
                FromEmailAddressDisplayName = mailConfigurationDtoCore.DisplayName,
                ToEmailAddress = ToEmailAddress ?? new List<string>() { },
                CCEmailAddress = CCEmailAddress ?? new List<string>() { },
                BCCEmailAddress = BCCEmailAddress ?? new List<string>() { },
                EmailDate = DateTime.Now,
                Subject = mailSubject,
                EmailBody = mailBody,
                IsBodyHtml = true
            };

            // Send the email
            await SendEmail(emailData, client);

        }




        public static async Task<(List<string>, List<string>)> GetEmailNotificationEmailAddress(int employeeID)
        {

            var mail = new List<string>();
            var proxyMail = new List<string>();
            string sql = $@"EXEC {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..spGetHierarchyForUnAuthorizedLeave {employeeID}";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var model = context.GetDataDictCollection(sql);

            if (model.Count() > 0)
            {
                model.ToList().ForEach(x =>
                {
                    mail.Add(x["Mail"].ToString());
                });
                model.ToList().ForEach(x =>
                {
                    proxyMail.Add(x["ProxyMail"].ToString());
                });

            }
            return await Task.FromResult((mail.Distinct().ToList(), proxyMail.Distinct().ToList()));
        }

        public bool GetValidatePath(int employeeID, string filePath)
        {
            bool isValid = false;
            string replacedfilePath = filePath.Replace("/", "").Replace("\\", "");

            string commentSql = $@"EXEC {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..spValidateAttachmentPath {employeeID}, '{replacedfilePath}'";
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var list = context.GetData(commentSql).ToList();

            isValid = Convert.ToInt32(list[0].Value) > 0 ? true : false;

            return isValid;
        }



        public async Task<List<Dictionary<string, object>>> GetCustodianWalletByEmployeeID(int EmployeeID)
        {
            string sql = @$"SELECT CWID, WalletName, EmployeeID, ReimbursementThreshold, OpeningBalance, CurrentBalance, Limit, IsActive
                    FROM Accounts..CustodianWallet CW
                    WHERE EXISTS (
                        SELECT 1
                        FROM HRMS..ViewALLEmployee VA
                        CROSS APPLY STRING_SPLIT(CW.DivisionIDs, ',') AS CW_Division
                        CROSS APPLY STRING_SPLIT(CW.DepartmentIDs, ',') AS CW_Department
                        WHERE VA.EmployeeID = {EmployeeID}
                        AND VA.DivisionID = TRY_CAST(CW_Division.value AS INT)
                        AND VA.DepartmentID = TRY_CAST(CW_Department.value AS INT)
                    )";

            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var result = await Task.Run(() => context.GetDataDictCollection(sql).ToList());

            return result;
        }

        public async Task<List<Dictionary<string, object>>> GetDataDictCollection(string sql)
        {
            //var context = (ExternalOraDbUtility)AppContexts.GetInstance(typeof(ExternalOraDbUtility));
            //var result = await Task.Run(() => context.GetDataDictCollection(sql).ToList());

            //return result;
            return null;
        }

        public async Task<DataTable> GetDataTable(string sql)
        {
            //var context = (ExternalOraDbUtility)AppContexts.GetInstance(typeof(ExternalOraDbUtility));
            //var result = await Task.Run(() => context.GetDataTable(sql));

            //return result;
            return null;
        }

        public async Task<List<T>> GetDataModelCollection<T>(string sql)
        {
            //var context = (ExternalOraDbUtility)AppContexts.GetInstance(typeof(ExternalOraDbUtility));
            //var result = await Task.Run(() => context.GetDataModelCollection<T>(sql).ToList());

            //return result;
            return null;
        }

        public async Task<List<Dictionary<string, object>>> GetDataDictCollectionWithTransaction(string sql)
        {
            //var context = (ExternalOraDbUtility)AppContexts.GetInstance(typeof(ExternalOraDbUtility));
            //var result = await Task.Run(() => context.GetDataDictCollectionWithTransaction(sql).ToList());

            //return result;
            return null;
        }
        public GridModel LoadGridModelOptimized(GridParameter parameters, string commandText)
        {
            //    var context = (ExternalOraDbUtility)AppContexts.GetInstance(typeof(ExternalOraDbUtility));
            //    return context.LoadGridModelOptimized(parameters, commandText);
            return null;
        }

        public GridModel LoadGridModel(GridParameter parameters, string commandText)
        {
            //var context = (ExternalOraDbUtility)AppContexts.GetInstance(typeof(ExternalOraDbUtility));
            //return context.LoadGridModel(parameters, commandText);

            return null;
        }


        #region Commoninterface Methods


        #endregion


        public static string CreateApprovalProcessForDynamic(int ReferenceID, string ApprovalProcessDescription,
            string ApprovalProcessTitle, int EmployeeID, int APTypeID, decimal Amount)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));

            var result = context.ExecuteScalar(@$"{AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..CreateApprovalProcessForDynamic @0,@1,@2,@3,@4,@5,@6,@7,@8,@9,@10", ReferenceID,
                ApprovalProcessDescription, ApprovalProcessTitle, AppContexts.User.CompanyID, AppContexts.User.UserID,
                AppContexts.User.IPAddress, EmployeeID, AppContexts.User.DivisionID, AppContexts.User.DepartmentID, Amount, APTypeID);

            return result.ToString();
        }



        //Oracle to SQL data find Start
        public async Task<List<Dictionary<string, object>>> GetDataDictCollectionFromMSSQL(string sql)
        {
            var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
            var result = await Task.Run(() => context.GetDataDictCollection(sql).ToList());
            return result;
        }

        public async Task<bool> ExecuteSQLCommand(string sql)
        {
            try
            {
                var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
                var result = await Task.Run(() => context.ExecuteSQLCommand(sql));
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<T>> GetDataModelCollectionFromMSSQL<T>(string sql)
        {
            //var context = (DbUtility)AppContexts.GetInstance(typeof(ExternalOraDbUtility));
            //var result = await Task.Run(() => context.GetDataModelCollection<T>(sql).ToList());

            //return result;
            return null;
        }
        //Oracle to SQL data find End        


        #region Hierarchy
        public async Task<List<Dictionary<string, object>>> GetHierarchy(string externalSchema)
        {
            string where = "";
            if (AppContexts.User.Role.Contains("DH"))
            {
                where = $@"WHERE DH_WALLET = '{AppContexts.User.WorkMobile}'";
            }
            else if (AppContexts.User.Role.Contains("TO") || AppContexts.User.Role.Contains("TM") || AppContexts.User.Role.Contains("PRO"))
            {
                where = $@"WHERE TO_TM_PRO_MOBILE = '{AppContexts.User.WorkMobile}'";
            }
            string sql = $@" SELECT
                                DISTINCT
                                CLUSTER_NAME,
                                CLUSTER_HEAD_NAME,
                                CLUSTER_HEAD_MOBILE,
                                REGION_NAME,
                                TO_TM_PRO,
                                TO_TM_PRO_NAME,
                                TO_TM_PRO_MOBILE,
                                DH_NAME,
                                DH_WALLET,
                                PU.USERID TO_TM_PRO_USER_ID,
                                PU.USER_NAME TO_TM_PRO_USER_NAME
                            FROM
                                {externalSchema}.NAGAD_PRISM_DASHBOARD_UI_ALL_UNIQUE_HIERARCHY_VIEW HV
                                JOIN {externalSchema}.PRISM_USER PU ON PU.MOBILE_NO = HV.TO_TM_PRO_MOBILE
                                {where}
                            ";
            return await GetDataDictCollection(sql);
        }

        #endregion Hierarchy

        //public async Task<List<Dictionary<string, object>>> GetAllAdminUsers()
        //{
        //    var sql = $@"select * FROM USER_PROFILE WHERE POSITION_ID=36";
        //    var list = await GetDataDictCollection(sql);

        //    return await Task.FromResult(list.ToList());
        //}

        // Generic method to get data from cache or fetch and cache it
        protected async Task<T> GetOrAddCachedAsync<T>(string cacheKey, Func<Task<T>> getDataAsync, ICacheManager<T> cacheManager, TimeSpan? cacheDuration = null, MemoryCacheEntryOptions options = null, Action<string> onCacheHit = null, Action<string> onCacheMiss = null, ConcurrentDictionary<string, bool> keyTracker = null) where T : class
        {
            var cachedItem = await cacheManager.GetAsync(cacheKey);
            if (cachedItem != null)
            {
                onCacheHit?.Invoke(cacheKey); // Invoke action on cache hit
                return cachedItem;
            }

            onCacheMiss?.Invoke(cacheKey); // Invoke action on cache miss
            var item = await getDataAsync();

            if (item != null)
            {
                // Use provided options, cacheDuration, or a default of 30 days
                var cacheOptions = options;
                if (cacheOptions == null && cacheDuration.HasValue)
                {
                    cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(cacheDuration.Value);
                }
                else if (cacheOptions == null) // Use default if neither options nor cacheDuration are provided
                {
                    cacheOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(30)); // Default to 30 days
                }

                if (cacheOptions != null)
                {
                    await cacheManager.SetAsync(cacheKey, item, cacheOptions);
                    keyTracker?.TryAdd(cacheKey, true); // Track key if tracker is provided and item is cached
                }
                // The else block (returning item without caching) is no longer needed if a default is always applied
            }

            return item;
        }

        // Method to generate a cache key based on name, type, and parameters
        protected string GenerateCacheKey(string cacheKeyName, string cacheType = null, GridParameter parameters = null)
        {
            string cacheKey = cacheKeyName;

            if (!string.IsNullOrEmpty(cacheType))
            {
                cacheKey += $"_{cacheType}";
            }

            // Append pagination/sorting/filtering details for grid views
            if (cacheType?.ToLower() == "gridview" && parameters != null)
            {
                cacheKey += $"_Offset_{parameters.Offset}_Limit_{parameters.Limit}";

                if (!string.IsNullOrEmpty(parameters.SortName))
                {
                    cacheKey += $"_SortName_{parameters.SortName}_SortOrder_{parameters.SortOrder}";
                }

                if (!string.IsNullOrEmpty(parameters.SearchBy))
                {
                    cacheKey += $"_SearchBy_{parameters.SearchBy}_Search_{parameters.Search}";
                }
            }
            // Add other type-specific parameters here if needed (e.g., ID for a single item)
            // else if (cacheType?.ToLower() == "itembyid" && itemId.HasValue)
            // {
            //     cacheKey += $"_Id_{itemId.Value}";
            // }

            return cacheKey;
        }

        // Generic method to clear cached keys from a tracking dictionary and cache
        protected async Task ClearCachedKeysAsync<T>(ConcurrentDictionary<string, bool> keyTracker, ICacheManager<T> cacheManager) where T : class
        {
            var keys = keyTracker.Keys.ToList();
            var removeTasks = new List<Task>();

            foreach (var key in keys)
            {
                // Redundant check/cast to potentially help compiler inference
                if (typeof(T).IsClass)
                {
                    removeTasks.Add(cacheManager.RemoveAsync(key));
                }
                else
                {
                    // This else block should ideally not be reached due to the 'where T : class' constraint
                    // Log a warning or handle appropriately if it is.
                    System.Diagnostics.Debug.WriteLine($"Attempted to clear cache for value type key: {key}");
                }

                keyTracker.TryRemove(key, out _);
            }

            await Task.WhenAll(removeTasks);
        }
    }

}