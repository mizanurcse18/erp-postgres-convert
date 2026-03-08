using System;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

using Core;
using Core.AppContexts;

using DAL.Core.Extension;
using DAL.Core.EntityBase;
using DAL.Core;
using System.Data;

namespace DAL.Core.Repository
{
    public class Repository<TDbContext, TEntity> : IRepository<TEntity> where TDbContext : BaseDbContext where TEntity : EntityBase.EntityBase
    {
        private readonly TDbContext Context;
        private readonly DbSet<TEntity> entities;

        public IQueryable<TEntity> Entities => entities;

        public Repository(TDbContext Context)
        {
            this.Context = Context;
            entities = Context.Set<TEntity>();
        }

        public async Task<IEnumerable<TEntity>> GetAll()
        {
            return await entities.ToListAsync();
        }

        public List<TEntity> GetAllList()
        {
            return entities.ToList();
        }

        public List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate)
        {
            return entities.Where(predicate).ToList();
        }

        public TEntity Get(params object[] keyValues)
        {
            TEntity entity = entities.Find(keyValues);
            return entity;
        }

        public async Task<TEntity> GetAsync(params object[] keyValues)
        {
            TEntity entity = await entities.FindAsync(keyValues);
            return entity;
        }

        public async Task<List<TEntity>> GetAllListAsync()
        {
            return await entities.ToListAsync();
        }

        public async Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await entities.Where(predicate).ToListAsync();
        }

        public TEntity Single(Expression<Func<TEntity, bool>> predicate)
        {
            return entities.Single(predicate);
        }

        public async Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await entities.SingleAsync(predicate);
        }

        public TEntity SingleOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return entities.SingleOrDefault(predicate);
        }

        public async Task<TEntity> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await entities.SingleOrDefaultAsync(predicate);
        }

        public virtual TEntity FirstOrDefault(Expression<Func<TEntity, bool>> predicate)
        {
            return entities.FirstOrDefault(predicate);
        }

        public virtual Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return entities.FirstOrDefaultAsync(predicate);
        }

        public int Count()
        {
            return entities.Count();
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return entities.Where(predicate).Count();
        }

        public async Task<int> CountAsync()
        {
            return await entities.CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await entities.Where(predicate).CountAsync();
        }

        public long LongCount()
        {
            return entities.LongCount();
        }

        public long LongCount(Expression<Func<TEntity, bool>> predicate)
        {
            return entities.Where(predicate).LongCount();
        }

        public async Task<long> LongCountAsync()
        {
            return await entities.LongCountAsync();
        }

        public async Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await entities.Where(predicate).LongCountAsync();
        }

        public void Add(TEntity entity)
        {
            UpdateToList(entity);
            SetAudited(entity);
        }

        public void AddRange(IEnumerable<TEntity> collection)
        {
            foreach (var entity in collection)
            {
                Add(entity);
            }
        }

        public int Update<T>(object entity) where T : EntityBase.EntityBase
        {
            var values = new List<object>();
            var properties = entity.GetType().GetProperties().ToList();
            var query = Context.GenerateUpdateSql<T>(entity, properties);
            foreach (var property in properties)
            {
                values.Add(property.GetValue(entity));
            }
            return ExecuteSqlCommand(query, values.ToArray());
        }

        public void SaveChanges()
        {
            Context.SaveChanges();
        }

        public void SaveChangesWithAudit()
        {
            Context.SaveChangesWithAudit();
        }

        protected virtual void AttachIfNot(TEntity entity)
        {
            if (entities.Local.Contains(entity)) return;
            entities.Attach(entity);
        }
        private void UpdateToList(TEntity entity)
        {
            switch (entity.ObjectState)
            {
                case ModelState.Added:
                    entities.Add(entity);
                    break;
                case ModelState.Modified:
                case ModelState.Archived:
                    AttachIfNot(entity);
                    Context.Entry(entity).State = EntityState.Modified;
                    break;
                case ModelState.Deleted:
                    AttachIfNot(entity);
                    entities.Remove(entity);
                    break;
            }
        }

        private void SetAudited(TEntity entity)
        {
            var auEntity = entity as Auditable;
            if (auEntity.IsNull()) return;
            var currUser = AppContexts.User;
            if (auEntity != null)
                switch (auEntity.ObjectState)
                {
                    case ModelState.Added:
                        auEntity.CompanyID = currUser.CompanyID;
                        auEntity.CreatedBy = currUser.UserID;
                        auEntity.CreatedDate = DateTime.Now;
                        auEntity.CreatedIP = currUser.IPAddress;
                        auEntity.RowVersion = 1;
                        break;
                    case ModelState.Modified:
                    case ModelState.Archived:
                        auEntity.UpdatedBy = currUser.UserID;
                        auEntity.UpdatedDate = DateTime.Now;
                        auEntity.UpdatedIP = currUser.IPAddress;

                        var oldVal = auEntity.RowVersion;
                        Context.Entry(entity).Property("RowVersion").OriginalValue = oldVal;
                        auEntity.RowVersion += 1;
                        break;
                }
        }

        private void ConcurrencyCheck(TEntity entity)
        {
            var auEntity = entity as Auditable;
            if (auEntity.IsNull()) return;
            if (auEntity != null)
                switch (auEntity.ObjectState)
                {
                    case ModelState.Added:
                        auEntity.RowVersion = 1;
                        break;
                    case ModelState.Modified:
                    case ModelState.Archived:
                        var oldVal = auEntity.RowVersion;
                        Context.Entry(entity).Property("RowVersion").OriginalValue = oldVal;
                        auEntity.RowVersion += 1;
                        break;
                }
        }

        public T GetMaxNumber<T>(string fieldName)
        {
            var sqlHelper = Context.CreateSqlGennerator();
            var tableName = Context.GetTableName<TEntity>();
            var sql = $"SELECT {sqlHelper.IsNullFunction()}(MAX({sqlHelper.QuoteIdentifier(fieldName)}), 0) + 1 AS {sqlHelper.QuoteIdentifier("MaxNumber")} FROM {tableName}";
            var result = Context.ExecuteScalar(sql);
            return (T)Convert.ChangeType(result, typeof(T));
        }

        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return Context.Database.ExecuteSqlRaw(sql, parameters);
        }

        #region Generic/Raw Query Execution
        public Dictionary<string, object> GetData(string query, bool pascalCase = false)
        {
            return Context.GetData(query, pascalCase);
        }
        public async Task<Dictionary<string, object>> GetDataAsync(string query, bool pascalCase = false)
        {
            return await Context.GetDataAsync(query, pascalCase);
        }

        public IEnumerable<Dictionary<string, object>> GetDataDictCollection(string query)
        {
            return Context.GetDataDictCollection(query);
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetDataDictCollectionAsync(string query)
        {
            return await Context.GetDataDictCollectionAsync(query);
        }

        public IEnumerable<Dictionary<string, object>> GetDataDictCollectionWithTransaction(string query)
        {
            return Context.GetDataDictCollectionWithTransaction(query);
        }

        public List<T> GetDataModelCollection<T>(string query)
        {
            return Context.GetDataModelCollection<T>(query);
        }

        public async Task<List<T>> GetDataModelCollectionAsync<T>(string query)
        {
            return await Context.GetDataModelCollectionAsync<T>(query);
        }

        public T GetModelData<T>(string query)
        {
            return Context.GetModelData<T>(query);
        }
        public async Task<T> GetModelDataAsync<T>(string query)
        {
            return await Context.GetModelDataAsync<T>(query);
        }

        public string GetJsonData(string query)
        {
            return Context.GetJsonData(query);
        }

        #endregion

        #region GridModel
        public GridModel LoadGridModel(GridParameter parameters, string commandText)
        {
            return Context.LoadGridModel(parameters, commandText);
        }

        public async Task<GridModel> LoadGridModelAsync(GridParameter parameters, string commandText)
        {
            return await Context.LoadGridModelAsync(parameters, commandText);
        }

        public GridModel LoadGridModelOptimized(GridParameter parameters, string commandText)
        {
            return Context.LoadGridModelOptimized(parameters, commandText);
        }

        #endregion

        #region ComboModel
        public List<ComboModel> LoadComboModel(string commandText, string valueField)
        {
            return Context.LoadComboModel(commandText, valueField);
        }

        public List<ComboModel> LoadComboModel(string commandText, string valueField, string textField)
        {
            return Context.LoadComboModel(commandText, valueField, textField);
        }

        public List<ComboModel> LoadComboModel(string commandText, string valueField, string textField, string descField)
        {
            return Context.LoadComboModel(commandText, valueField, textField, descField);
        }

        public bool BulkInsertFromDataTableToMSSQL(string tableName, DataTable dataTable, bool isTruncateTable, string[] DbColumnName, string[] InputColumnName)
        {
            return Context.BulkInsertFromDataTableToMSSQL(tableName, dataTable, isTruncateTable, DbColumnName, InputColumnName);
        }

        public bool ExecuteSQLCommand(string sql)
        {
            return Context.ExecuteSQLCommand(sql);
        }
        #endregion
    }
}