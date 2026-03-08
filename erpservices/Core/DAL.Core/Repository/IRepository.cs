using System;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;

using Core;
using System.Data;

namespace DAL.Core.Repository
{
    public interface IRepository<TEntity> where TEntity : EntityBase.EntityBase
    {
        IQueryable<TEntity> Entities { get; }
        Task<IEnumerable<TEntity>> GetAll();
        List<TEntity> GetAllList();
        Task<List<TEntity>> GetAllListAsync();
        List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate);
        Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate);
        TEntity Get(params object[] keyValues);
        Task<TEntity> GetAsync(params object[] keyValues);
        TEntity Single(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate);
        TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> collection);
        int Update<T>(object entity) where T : EntityBase.EntityBase;
        void SaveChanges();
        void SaveChangesWithAudit();
        int Count();
        Task<int> CountAsync();
        int Count(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
        long LongCount();
        Task<long> LongCountAsync();
        long LongCount(Expression<Func<TEntity, bool>> predicate);
        Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate);
        T GetMaxNumber<T>(string fieldName);
        int ExecuteSqlCommand(string sql, params object[] parameters);

        #region Generic/Raw Query Execution
        IEnumerable<Dictionary<string, object>> GetDataDictCollection(string query);
        Task<IEnumerable<Dictionary<string, object>>> GetDataDictCollectionAsync(string query);
        IEnumerable<Dictionary<string, object>> GetDataDictCollectionWithTransaction(string query);
        List<T> GetDataModelCollection<T>(string query);
        Task<List<T>> GetDataModelCollectionAsync<T>(string query);
        T GetModelData<T>(string query);
        Task<T> GetModelDataAsync<T>(string query);
        string GetJsonData(string query);
        Dictionary<string, object> GetData(string query, bool pascalCase = false);
        Task<Dictionary<string, object>> GetDataAsync(string query, bool pascalCase = false);
        #endregion

        #region Grid
        GridModel LoadGridModel(GridParameter parameters, string commandText);
        GridModel LoadGridModelOptimized(GridParameter parameters, string commandText);
        Task<GridModel> LoadGridModelAsync(GridParameter parameters, string commandText);
        #endregion

        #region Combo
        List<ComboModel> LoadComboModel(string commandText, string valueField);
        List<ComboModel> LoadComboModel(string commandText, string valueField, string textField);
        List<ComboModel> LoadComboModel(string commandText, string valueField, string textField, string descField);
        #endregion

        #region Bulk Upload
        bool BulkInsertFromDataTableToMSSQL(string tableName, DataTable dataTable, bool isTruncateTable, string[] dbColumnName, string[] inputColumnName);
        bool ExecuteSQLCommand(string sql);
        #endregion
    }
}
