using Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace DAL.Core
{
    public interface IModelAdapter
    {
        Task<Dictionary<string, object>> GetDataDictionary(string cmdText, CommandType cmdType, bool pascalCase, Dictionary<string, object> parameters = null);
        Task<Dictionary<string, object>> GetDataDictionary(string cmdText, bool pascalCase, Dictionary<string, object> parameters = null);
        Task<IEnumerable<Dictionary<string, object>>> GetDataDictionaryCollection(string cmdText, CommandType cmdType, bool pascalCase = false, Dictionary<string, object> parameters = null);
        Task<IEnumerable<Dictionary<string, object>>> GetDataDictionaryCollection(string cmdText, bool pascalCase = false, Dictionary<string, object> parameters = null);
        Task<T> GetModel<T>(string cmdText, params object[] parameters)
        where T : class;

        Task<T> GetModel<T>(string cmdText, CommandType cmdType, Dictionary<string, object> parameters = null)
        where T : class;

        T GetModel<T>(Dictionary<string, object> data)
        where T : class;

        Task<List<T>> GetModels<T>(string cmdText, params object[] parameters)
        where T : class;

        Task<List<T>> GetModels<T>(string cmdText, CommandType cmdType, Dictionary<string, object> parameters = null)
        where T : class;

        List<T> GetModels<T>(List<Dictionary<string, object>> data)
        where T : class;

        Task<List<Dictionary<string, object>>[]> GetResultSet(string cmdText, params object[] parameters);
        Task<DataSet> GetDataSet(string cmdText, params object[] parameters);
        Task<DataSet> GetDataIntoDataSet(string cmdText);
    }
}