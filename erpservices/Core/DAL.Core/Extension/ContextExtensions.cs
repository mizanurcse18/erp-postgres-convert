using System;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

using Core.Extensions;
using DAL.Core.Helper;

namespace DAL.Core.Extension
{
    internal static class ContextExtensions
    {
        public static string GetTableName<TEntity>(this DbContext context) where TEntity : EntityBase.EntityBase
        {
            var sqlHelper = context.CreateSqlGennerator();
            var mapping = context.Model.FindEntityType(typeof(TEntity));
            var schema = mapping.GetSchema() ?? "dbo";
            schema = $"{sqlHelper.QuoteIdentifier(schema)}.";
            var tableName = sqlHelper.QuoteIdentifier(mapping.GetTableName());
            return $"{schema}{tableName}";
        }

        public static List<string> GetKeyNames<TEntity>(this DbContext context) where TEntity : EntityBase.EntityBase
        {
            return context.GetKeyNames(typeof(TEntity));
        }

        public static List<string> GetKeyNames(this DbContext context, Type entityType)
        {
            return context.Model.FindEntityType(entityType).FindPrimaryKey().Properties.Select(x => x.Name).ToList();
        }

        public static string GenerateUpdateSql<TEntity>(this DbContext context, object entity, List<PropertyInfo> propertyList) where TEntity : EntityBase.EntityBase
        {
            var sqlHelper = context.CreateSqlGennerator();
            var type = typeof(TEntity);
            var tableName = context.GetTableName<TEntity>();
            var entityType = context.Model.FindEntityType(type);
            var keyFields = context.GetKeyNames(type);
            var properties = entityType.GetProperties();

            var updateColumn = new StringBuilder();
            updateColumn.Append($"UPDATE {tableName}\n");
            updateColumn.Append("\tSET\t");

            int pindex = 0;
            foreach (var param in propertyList)
            {
                if (keyFields.Contains(param.Name)) continue;
                var col = properties.FirstOrDefault(p => p.Name == param.Name);
                if (col.IsNull()) continue;
                updateColumn.Append($"{sqlHelper.QuoteIdentifier((col ?? throw new InvalidOperationException()).GetColumnName())} = @p{pindex},\n\t\t");
                pindex++;
            }

            updateColumn = updateColumn.Remove(updateColumn.Length - 4, 4);

            updateColumn.Append("\n\tWHERE\t");

            foreach (var key in keyFields)
            {
                var kProp = entity.GetType().GetProperty(key);
                if (kProp.IsNotNull())
                {
                    if (propertyList.Contains(kProp)) propertyList.Remove(kProp);
                    propertyList.Add(kProp);
                }

                updateColumn.Append($"{sqlHelper.QuoteIdentifier(key)} = @p{pindex} AND ");
                pindex++;
            }

            return updateColumn.Remove(updateColumn.Length - 5, 5).ToString();
        }

        public static bool IsSqlProvider(this DbContext context)
        {
            //return context.Database.GetDbConnection() is SqlConnection;
            return true;
        }

        public static ISqlGen CreateSqlGennerator(this DbContext context)
        {
            if (context.IsSqlProvider())
                return new SqlGen();

            return new OracleGen();
        }
    }
}