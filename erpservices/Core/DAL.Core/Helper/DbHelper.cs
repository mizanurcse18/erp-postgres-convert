using Core;
using Core.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace DAL.Core
{
	internal static class DbHelper
	{
		public static void IsValidCommandText(string commandText)
		{
			if (commandText.IsNullOrEmpty())
			{
				throw new Exception("Query is blank.");
			}
		}

		public static void PrepareCommand(IDbCommand command, CommandType commandType, string cmdText)
		{
			command.CommandType = commandType;
			command.CommandText = cmdText;
			command.CommandTimeout = 600;
		}

		public static void PrepareCommandParameter(IDbCommand command, Dictionary<string, object> parameters)
		{
			Type type;
			if (parameters.IsNull() || parameters.Count.IsZero())
			{
				return;
			}
			foreach (KeyValuePair<string, object> parameter in parameters)
			{
				IDbDataParameter dbDataParameter = command.CreateParameter();
				object value = parameter.Value;
				IDbDataParameter dbDataParameter1 = dbDataParameter;
				if (value != null)
				{
					type = value.GetType();
				}
				else
				{
					type = null;
				}
				dbDataParameter1.DbType = MappedDbType.GetDbType(type);
				dbDataParameter.ParameterName = parameter.Key;
				dbDataParameter.Value = value;
				command.Parameters.Add(dbDataParameter);
			}
		}
	}
}