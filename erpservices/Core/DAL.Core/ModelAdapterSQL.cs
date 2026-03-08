using Core;
using Core.AppContexts;
using Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace DAL.Core
{
    public class ModelAdapterSQL : IModelAdapter
    {
        public ModelAdapterSQL()
        {
        }

        protected void CloseConnection(DbConnection connection)
        {
            if (connection.IsNull() || connection.State != ConnectionState.Open)
            {
                return;
            }
            connection.Close();
            connection.Dispose();
        }

        private async Task<DbConnection> CreateConnection()
        {
            DbUtility instance = AppContexts.GetInstance<DbUtility>();
            DbConnection dbConnection = RelationalDatabaseFacadeExtensions.GetDbConnection(instance.Database);
            //await dbConnection.OpenAsync().ConfigureAwait(true);
            try
            {
                if (dbConnection.State != ConnectionState.Open)
                {
                    await dbConnection.OpenAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return dbConnection;
        }

        #region Generic/Raw Query Execution

        public async Task<Dictionary<string, object>> GetDataDictionary(string cmdText, CommandType cmdType, bool pascalCase = false, Dictionary<string, object> parameters = null)
        {
            IDataReader reader = null;
            var fields = new List<string>();
            var dataRow = new Dictionary<string, object>();
            try
            {
                reader = await ExecuteReader(cmdText, cmdType, parameters);
                if (reader.Read())
                {
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        fields.Add(reader.GetName(i));
                    }
                    while (reader.Read())
                    {
                        dataRow = GetDataDict(fields, reader, pascalCase);
                    }
                }
                return dataRow;
            }
            finally
            {
                reader.CloseReader();
            }
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetDataDictionaryCollection(string cmdText, CommandType cmdType, bool pascalCase = false, Dictionary<string, object> parameters = null)
        {
            IDataReader reader = null;
            var fields = new List<string>();
            var dataTable = new List<Dictionary<string, object>>();
            try
            {
                reader = await ExecuteReader(cmdText, cmdType, parameters);
                if (reader.Read())
                {
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        fields.Add(reader.GetName(i));
                    }
                    while (reader.Read())
                    {
                        dataTable.Add(GetDataDict(fields, reader, pascalCase));
                    }
                }
                return dataTable;
            }
            finally
            {
                reader.CloseReader();
            }
        }

        public async Task<DataSet> GetDataSet(string cmdText, CommandType cmdType, bool pascalCase = false, Dictionary<string, object> parameters = null)
        {
            IDataReader reader = null;

            try
            {
                reader = await ExecuteReader(cmdText, cmdType, parameters);
                return DataReaderToDataSet(reader);
                //return _;
            }
            finally
            {
                reader.CloseReader();
            }
        }

        public Task<Dictionary<string, object>> GetDataDictionary(string cmdText, bool pascalCase, Dictionary<string, object> parameters = null)
        {
            return GetDataDictionary(cmdText, CommandType.Text, pascalCase, parameters);
        }
        public Task<IEnumerable<Dictionary<string, object>>> GetDataDictionaryCollection(string cmdText, bool pascalCase = false, Dictionary<string, object> parameters = null)
        {
            return GetDataDictionaryCollection(cmdText, CommandType.Text, pascalCase, parameters);

        }

        public async Task<DataSet> GetDataSet(string cmdText, params object[] parameters)
        {
            return await GetDataSet(cmdText, CommandType.Text);
        }
        #endregion Generic/Raw Query Execution

        public async Task<T> GetModel<T>(string cmdText, params object[] parameters)
        where T : class
        {

            Dictionary<string, object> dictionary = this.CreateParameters(parameters);
            return await GetModel<T>(cmdText, CommandType.Text, dictionary);
        }
        public async Task<T> GetModel<T>(string cmdText, CommandType cmdType, Dictionary<string, object> parameters = null)
        where T : class
        {
            //if (!typeof(T)()) throw new Exception("Item must be inherited from BaseModel class.");
            IDataReader reader = null;
            try
            {
                reader = await ExecuteReader(cmdText, cmdType, parameters);
                while (reader.Read())
                {
                    var fields = ModelAdapterSQL.GetFields(reader);
                    var model = CreateModel<T>(fields);
                    return model;
                }
                return Activator.CreateInstance<T>();
            }
            finally
            {
                reader.CloseReader();
            }
        }
        public T GetModel<T>(Dictionary<string, object> data)
        where T : class
        {
            return ModelAdapterSQL.CreateModel<T>(data);
        }
        public async Task<List<T>> GetModels<T>(string cmdText, params object[] parameters)
        where T : class
        {
            Dictionary<string, object> dictionary = this.CreateParameters(parameters);
            return await GetModels<T>(cmdText, CommandType.Text, dictionary);
        }
        public async Task<List<T>> GetModels<T>(string cmdText, CommandType cmdType, Dictionary<string, object> parameters = null)
        where T : class
        {
            IDataReader reader = null;
            var list = new List<T>();
            try
            {
                reader = await ExecuteReader(cmdText, cmdType, parameters);
                while (reader.Read())
                {
                    var fields = ModelAdapterSQL.GetFields(reader);
                    var model = CreateModel<T>(fields);
                    list.Add(model);
                }
                return list;
            }
            finally
            {
                reader.CloseReader();
            }
        }
        public List<T> GetModels<T>(List<Dictionary<string, object>> data)
        where T : class
        {
            List<T> list = new List<T>();
            foreach (Dictionary<string, object> datum in data)
            {
                list.Add(ModelAdapterSQL.CreateModel<T>(datum));
            }
            return list;
        }
        private static T CreateModel<T>(Dictionary<string, object> fields)
        {
            if (fields.IsNull())
            {
                fields = new Dictionary<string, object>();
            }
            T t = Activator.CreateInstance<T>();
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(t);
            foreach (KeyValuePair<string, object> field in fields)
            {
                PropertyDescriptor propertyDescriptor = properties.Find(field.Key, true);
                if (propertyDescriptor.IsNull())
                {
                    continue;
                }
                object obj = field.Value.MapField(propertyDescriptor.PropertyType);
                propertyDescriptor.SetValue(t, obj);
            }
            return t;
        }
        private Dictionary<string, object> CreateParameters(object[] parameters)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            for (int i = 0; i < (int)parameters.Length; i++)
            {
                dictionary.Add(string.Format("p{0}", i), parameters[i]);
            }
            return dictionary;
        }
        private DataSet DataReaderToDataSet(IDataReader reader)
        {
            DataSet dataSet = new DataSet();
            do
            {
                int fieldCount = reader.FieldCount;
                DataTable dataTable = new DataTable();
                for (int i = 0; i < fieldCount; i++)
                {
                    dataTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
                }
                dataTable.BeginLoadData();
                object[] objArray = new object[fieldCount];
                while (reader.Read())
                {
                    reader.GetValues(objArray);
                    dataTable.LoadDataRow(objArray, true);
                }
                dataTable.EndLoadData();
                dataSet.Tables.Add(dataTable);
            }
            while (reader.NextResult());
            reader.Close();
            return dataSet;
        }
        private async Task<DbDataReader> ExecuteReader(string cmdText, CommandType cmdType, Dictionary<string, object> parameters)
        {
            DbDataReader dbDataReader;
            if (cmdText.IsNullOrEmpty())
            {
                throw new Exception("Query is blank.");
            }
            DbConnection dbConnection = await this.CreateConnection();
            try
            {
                var fields = new List<string>();
                using (var command = dbConnection.CreateCommand())
                {
                    command.CommandText = cmdText;
                    command.CommandType = cmdType;
                    DbHelper.PrepareCommandParameter(command, parameters);
                    dbDataReader = command.ExecuteReader();
                    for (var i = 0; i < dbDataReader.FieldCount; i++)
                    {
                        fields.Add(dbDataReader.GetName(i));
                    }
                }
                //var dbCommand = dbConnection.CreateCommand();
                //DbHelper.PrepareCommandParameter(dbCommand, parameters);
                //dbDataReader = await dbCommand.ExecuteReaderAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                this.CloseConnection(dbConnection);
                throw new Exception();
            }
            //finally
            //{
            //    this.CloseConnection(dbConnection);
            //}
            return dbDataReader;
        }

        private async Task<List<Dictionary<string, object>>> GetAll(DbDataReader reader)
        {
            List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
            List<string> list1 = new List<string>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                list1.Add(reader.GetName(i));
            }
            while (true)
            {
                if (!await reader.ReadAsync().ConfigureAwait(false))
                {
                    break;
                }
                list.Add(ModelAdapterSQL.GetFields(list1, reader));
            }
            return list;
        }
        private static Dictionary<string, object> GetFields(IEnumerable<string> cols, IDataRecord reader)
        {
            var values = new Dictionary<string, object>();
            foreach (var colName in cols)
            {
                var ordinal = reader.GetOrdinal(colName);
                var type = reader.GetDataTypeName(ordinal);
                var value = reader.GetValue(ordinal);

                if (value.IsNullOrDbNull())
                {
                    if (type == "nvarchar")
                    {
                        value = string.Empty;
                    }

                    values[colName] = value;
                    continue;
                }

                switch (type)
                {
                    case "bigint":
                        values[colName] = value.ToString();
                        break;
                    case "date":
                        values[colName] = ((DateTime)value).ToString(Util.SysDateFormat);
                        break;
                    case "datetime":
                        values[colName] = ((DateTime)value).ToString(Util.SysDateFormat + " " + Util.TimeFormat);
                        break;
                    case "time":
                        values[colName] = DateTime.MinValue.Add((TimeSpan)value).ToString(Util.TimeFormat);
                        break;
                    default:
                        values[colName] = value;
                        break;
                }
            }

            return values;
        }
        private static Dictionary<String, Object> GetFields(IDataRecord reader)
        {
            var fieldCount = reader.FieldCount;
            var fields = new Dictionary<String, Object>(fieldCount);
            for (var i = 0; i < fieldCount; i++)
            {
                var fieldName = reader.GetName(i);
                var ordinal = reader.GetOrdinal(fieldName);
                fields[fieldName] = reader.GetValue(ordinal);
            }

            return fields;
        }
        public async Task<List<Dictionary<string, object>>[]> GetResultSet(string cmdText, params object[] parameters)
        {
            Dictionary<string, object> dictionary = this.CreateParameters(parameters);
            return await this.GetResultSet(cmdText, 1, dictionary);
        }

        private static string MakeParameter(List<FinderParameter> parameters, out Dictionary<string, object> queryParams)
        {
            if (parameters.IsNull())
            {
                parameters = new List<FinderParameter>();
            }
            queryParams = new Dictionary<string, object>();
            string empty = string.Empty;
            int num = 0;
            foreach (FinderParameter parameter in parameters)
            {
                if (parameter.value.IsNullOrEmpty())
                {
                    continue;
                }
                string str = parameter.operat;
                object obj = parameter.value;
                if (parameter.type == "Date")
                {
                    obj = obj.ToString().ToDate(false).StringValue();
                }
                else if (parameter.type == "Int")
                {
                    obj = Convert.ChangeType(obj, typeof(int));
                }
                else if (parameter.type == "Decimal")
                {
                    obj = Convert.ChangeType(obj, typeof(decimal));
                }
                string str1 = parameter.operat;
                if (str1 != null)
                {
                    switch (str1)
                    {
                        case "bw":
                            {
                                str = "LIKE";
                                obj = string.Format("{0}%", obj);
                                break;
                            }
                        case "bn":
                            {
                                str = "NOT LIKE";
                                obj = string.Format("{0}%", obj);
                                break;
                            }
                        case "ew":
                            {
                                str = "LIKE";
                                obj = string.Format("%{0}", obj);
                                break;
                            }
                        case "en":
                            {
                                str = "NOT LIKE";
                                obj = string.Format("%{0}", obj);
                                break;
                            }
                        case "cn":
                            {
                                str = "LIKE";
                                obj = string.Format("%{0}%", obj);
                                break;
                            }
                        case "nc":
                            {
                                str = "NOT LIKE";
                                obj = string.Format("%{0}%", obj);
                                break;
                            }
                        case "in":
                            {
                                str = "IN";
                                break;
                            }
                        case "ni":
                            {
                                str = "NOT IN";
                                break;
                            }
                    }
                }
                if (parameter.operat == "in" || parameter.operat == "ni")
                {
                    empty = (num != 0 ? string.Concat(empty, string.Format(" AND {0} {1} (SELECT * FROM dbo.SplitString(@{2},','))", parameter.name, str, num)) : string.Format("WHERE {0} {1} (SELECT * FROM dbo.SplitString(@{2},','))", parameter.name, str, num));
                }
                else
                {
                    empty = (num != 0 ? string.Concat(empty, string.Format(" AND {0} {1} @{2}", parameter.name, str, num)) : string.Format("WHERE {0} {1} @{2}", parameter.name, str, num));
                }
                queryParams.Add(num.ToString(), obj);
                num++;
            }
            return empty;
        }
        private static string MakeSearchQuery(GridParameter finder, out Dictionary<string, object> queryParams, string sql)
        {
            string empty = string.Empty;
            string str = string.Empty;
            if (!finder.SortName.IsNotNullOrEmpty())
            {
                finder.SortName = string.Empty;
            }
            else
            {
                finder.SortName = string.Concat("ORDER BY ", finder.SortName, " ", finder.SortOrder);
            }
            string str1 = ModelAdapterSQL.MakeParameter(finder.Parameters, out queryParams);
            if (finder.ServerPagination)
            {
                empty = string.Format("OFFSET {0} ROWS FETCH NEXT {1} ROWS ONLY", finder.Offset, finder.Limit);
                str = ", COUNT(*) OVER () AS TotalRows";
            }
            return string.Concat(new string[] { "SELECT * ", str, "\r\n                            FROM (\r\n\t                            ", sql, "                                \r\n                            ) AS RES\r\n                            ", str1, "\r\n                            ", finder.SortName, "\r\n                            ", empty });
        }
        #region Data Access Utilities
        private Dictionary<string, object> GetDataDict(IEnumerable<string> fields, IDataRecord reader, bool pascalCase = false)
        {
            var dataDict = new Dictionary<string, object>();

            foreach (var colName in fields)
            {
                var ordinal = reader.GetOrdinal(colName);
                var value = reader.GetValue(ordinal);
                var type = reader.GetDataTypeName(ordinal);

                var mappedFieldName = pascalCase ? colName.Substring(0, 1).ToLower() + colName.Substring(1, colName.Length - 2) + colName.Substring(colName.Length - 1, 1).ToLower() : colName;

                switch (type)
                {
                    case "bigint":
                        dataDict[mappedFieldName] = value.ToString();
                        break;
                    default:
                        dataDict[mappedFieldName] = value;
                        break;
                }
            }

            return dataDict;
        }

        public Task<DataSet> GetDataIntoDataSet(string cmdText)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}