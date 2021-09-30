using Ring.Data.Core;
using Ring.Data.Core.Extensions;
using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.Data.Helpers
{

	/// <summary>
	/// Sql helper DML
	/// </summary>
	internal static class SqlHelper
	{

		/// <summary>
		/// Generate select query (READ)
		/// </summary>
		public static IDbCommand GetQuery(IDbConnection connection, BulkRetrieveQuery bulkQuery)
		{
			if (bulkQuery.Type == BulkQueryType.SetRoot && bulkQuery.ForeignKeys.Count <= 0) return null;
			var query = new StringBuilder();
			IDbCommand result = null;

			#region select

			query.Append(Constants.Select);
			bulkQuery.TargetObject.AppendSelectClause(query); // remove one allocation
			// manage @mtm tables without fields 
			if (bulkQuery.ForeignKeys.Count > 0) query.Append(Constants.SelectSeparator);
			for (var i = 0; i < bulkQuery.ForeignKeys.Count; ++i)
			{
				query.Append(bulkQuery.ForeignKeys[i].Name);
				query.Append(Constants.SelectSeparator);
			}
			if (bulkQuery.ForeignKeys.Count > 0) query.Length = query.Length - 1;

			#endregion

			#region from

			query.Append(Constants.From);
			query.Append(bulkQuery.TargetObject.PhysicalName);

			#endregion

			#region where

			switch (bulkQuery.Type)
			{
				case BulkQueryType.SetRoot:
				case BulkQueryType.SimpleQuery:
					result = BuildSimpleQuery(connection, bulkQuery, query);
					break;
				case BulkQueryType.TraverseFromParent:
					result = BuildTraverseFromParent(connection, bulkQuery, query);
					break;
			}

			#endregion

			#region sort

			BuildOrderBy(bulkQuery, query);

			#endregion

			#region page size

			//TODO: create a bind variable (parameter) for limit ? 
			if (bulkQuery.PageSize > 0)
				query.Append(string.Format(Constants.PageSizeLite, bulkQuery.PageSize.ToString()));

			#endregion

			// adapt commandText
			if (result != null && query.Length > 0) result.CommandText = query.ToString();
			return result;
		}

		/// <summary>
		/// Generate select query (READ)
		/// </summary>
		public static IDbCommand GetQuery(IDbConnection connection, PathEvaluatorQuery pathEvaluatorQuery)
		{
			if (pathEvaluatorQuery.Fields.Count + pathEvaluatorQuery.Relations.Count <= 0) return null;
			var query = new StringBuilder();
			var result = connection.CreateNewCommandInstance();

			#region select

			query.Append(Constants.Select);
			pathEvaluatorQuery.Result.Table.AppendSelectClause(query, pathEvaluatorQuery.Fields, pathEvaluatorQuery.Relations);

			#endregion

			#region from

			query.Append(Constants.From);
			query.Append(pathEvaluatorQuery.Result.Table.PhysicalName);

			#endregion

			#region where

			query.Append(Constants.Where);
			query.Append(pathEvaluatorQuery.Result.Table.PrimaryKey.Name);
			query.Append(Constants.Equal);
			query.Append(Constants.BindVariablePrefix);
			query.Append(Constants.FirstBindVariableName);

			#endregion

			// adapt commandText
			result.CommandText = query.ToString();
			result.AddParameter(GetParameter(connection, FieldType.Long, Constants.FirstBindVariableName,
				pathEvaluatorQuery.Result.GetField()));
			return result;
		}


		/// <summary>
		/// Generate insert, update, delete queries (WRITE)
		/// </summary>
		public static IDbCommand GetQuery(IDbConnection connection, BulkSaveQuery query)
		{
			IDbCommand result = null;

			if (query.CurrentRecord.Table == null) throw new ArgumentException(Constants.ErrUnknowRecordType);

			switch (query.Type)
			{
				case BulkSaveType.DeleteRecord:
					result = GetDeleteQuery(connection, query);
					break;
				case BulkSaveType.InsertMtm:
				case BulkSaveType.InsertMtmIfNotExist:
					result = GetInsertMtmQuery(connection, query);
					break;
				case BulkSaveType.InsertRecord:
					result = GetInsertQuery(connection, query);
					break;
				case BulkSaveType.UpdateRecord:
				case BulkSaveType.RelateRecords:
					result = GetUpdateQuery(connection, query);
					break;
			}
			return result;
		}

		/// <summary>
		/// Update @meta table (reset all value)
		/// </summary>
		public static void UpdateMetaData(IDbConnection connection, int schemaId, int objectId, int objectType,
			int referenceId, MetaData meta)
		{
			// UPDATE {0} SET {1} WHERE {2}
			//   ==> MetaDataWhere = @"id=:B1 AND schema_id=:B2 AND object_type=:B3 and reference_id=:B4"
			//   ==> MetaDataSet = @"flags=:B5, description=:B6, value=:B7";
			var sql = string.Format(Constants.UpdateQuery, Global.Databases.MetaDataTable.PhysicalName, Constants.MetaDataSet,
				Constants.MetaDataWhere);
			var parameters = new IDbParameter[7];

			// parameters 
			// $1 - $4 - WHERE
			parameters[0] = GetParameter(connection, FieldType.Long, Constants.SecondBindVariableName, objectId.ToString());
			parameters[1] = GetParameter(connection, FieldType.Long, Constants.ThirdBindVariableName, schemaId.ToString());
			parameters[2] = GetParameter(connection, FieldType.Long, Constants.FourthBindVariableName, objectType.ToString());
			parameters[3] = GetParameter(connection, FieldType.Long, Constants.FifthBindVariableName, referenceId.ToString());

			// $5 - $7 - WHERE
			parameters[4] = GetParameter(connection, FieldType.Long, Constants.BindVariableName + 5.ToString(),
				meta.Flags.ToString());
			parameters[5] = GetParameter(connection, FieldType.String, Constants.BindVariableName + 6.ToString(),
				meta.Description);
			parameters[6] = GetParameter(connection, FieldType.String, Constants.BindVariableName + 7.ToString(),
				meta.Value);

			// execute command 
			using (var cmd = connection.CreateNewCommandInstance())
			{
				cmd.Connection = connection;
				cmd.CommandText = sql;
				cmd.AddParameters(parameters);
				cmd.ExecuteNonQuery();
				cmd.Connection = null;
			}
		}

		/// <summary>
		/// Update @meta_id as fast as possible (to generate a new Objid) - Improve performance
		/// </summary>
		public static IDbCommand GetUpdateMetaIdQuery(IDbConnection connection, int id, int schemaId, int objectType,
			long range)
		{
			// TODO manage multiple instance on Ring on the same dataBase !!!! Change the query 
			// TODO use prepared statments
			var parameters = new IDbParameter[4];
			var sql = connection.CreateNewCommandInstance();

			// parameters 
			// :$0 - SET 
			parameters[0] = GetParameter(connection, FieldType.Long, Constants.FirstBindVariableName, range.ToString());

			// :$1 & :B2 - WHERE id=:B1 AND schema_id=:B2 AND object_type=:B3
			parameters[1] = GetParameter(connection, FieldType.Int, Constants.SecondBindVariableName, id.ToString());
			parameters[2] = GetParameter(connection, FieldType.Int, Constants.ThirdBindVariableName, schemaId.ToString());
			parameters[3] = GetParameter(connection, FieldType.Int, Constants.FourthBindVariableName, objectType.ToString());

			sql.CommandText = string.Format(Constants.UpdateQueryWithReturning, Global.Databases.MetaDataIdTable.PhysicalName,
				Constants.MetaDataIdSet, Constants.MetaDataIdWhere, Constants.MetaDataIdSetField);
			sql.AddParameters(parameters);

			return sql;
		}

		#region private methods 

		/// <summary>
		/// Manage Where clause for Simple Queries and sqlite provider
		/// </summary>
		private static IDbCommand BuildSimpleQuery(IDbConnection connection, BulkRetrieveQuery bulkQuery, StringBuilder query)
		{
			var result = connection.CreateNewCommandInstance();
			if (bulkQuery?.Filters?.Count > 0 && query != null)
			{
				query.Append(Constants.Where);
				var parameters = GetFilters(connection, bulkQuery, query, 0, true);
				if (parameters.Length > 1) result.AddParameters(parameters);
				if (parameters.Length == 1) result.AddParameter(parameters[0]);
			}

			#region page size

			if (bulkQuery?.PageSize > 0 && bulkQuery.PageNumber > 1 && query != null)
			{
				// 1) build subquery (avoid recursion) 
				var subquery = new StringBuilder();
				var rowid = GetRowId(connection.Provider);

				subquery.Append(string.Format(Constants.DefaultSqlFullScan.ToLower(), rowid, bulkQuery.TargetObject.PhysicalName));
				var limitValue = (bulkQuery.PageNumber - 1) * bulkQuery.PageSize;

				if (bulkQuery.Filters?.Count > 0)
				{
					subquery.Append(Constants.Where);
					GetFilters(connection, bulkQuery, subquery, 0, false);
				}
				BuildOrderBy(bulkQuery, subquery);
				subquery.Append(string.Format(Constants.PageSizeLite, limitValue));

				// 2)  add criteria to query
				query.Append(bulkQuery.Filters?.Count > 0 ? Constants.And : Constants.Where);
				query.Append(rowid);
				query.Append(Constants.NotIn);
				query.Append(Constants.StartParenthesis);
				query.Append(subquery);
				query.Append(Constants.EndParenthesis);
			}

			#endregion

			//result[0].CommandText = query?.ToString();
			return result;
		}

		/// <summary>
		/// Build order by for all provider
		/// </summary>
		private static void BuildOrderBy(BulkRetrieveQuery bulkQuery, StringBuilder query)
		{
			if (bulkQuery.Sorts?.Count <= 0) return;
			query.Append(Constants.OrderBy);
			var sortCount = bulkQuery.Sorts?.Count;
			for (var i = 0; i < sortCount; ++i)
			{
				var filter = bulkQuery.Sorts[i];
				if (filter.Field.Type == FieldType.String && !filter.Field.CaseSensitive)
					query.Append(Schema.Helpers.SqlHelper.GetSearchableFieldName(filter.Field));
				else query.Append(filter.Field.Name);
				query.Append(filter.Type == SortOrderType.Descending
					? Constants.OrderByDesc
					: Constants.OrderByAsc);
				if (i < sortCount - 1) query.Append(Constants.SelectSeparator);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private static IDbCommand BuildTraverseFromParent(IDbConnection connection, BulkRetrieveQuery bulkQuery,
			StringBuilder query)
		{
			if (bulkQuery.Relation == null) return null;

			// checks
			if (bulkQuery.ParentQuery == null) return null; // do nothing !!!
			if (!bulkQuery.ParentQuery.Launched) bulkQuery.ParentQuery.LoadResult(connection);
			if (bulkQuery.ParentQuery.Result.Count <= 0) return null;

			query.Append(Constants.Where);
			var parameters = GetFilters(connection, bulkQuery, query, bulkQuery.Relation); // filter to traverse 
			if (parameters == null || parameters.Length == 0) return null;
			var result = connection.CreateNewCommandInstance();
			result.AddParameters(parameters);
			if (bulkQuery.Filters.Count <= 0) return result;
			var paremetersExtra = GetFilters(connection, bulkQuery, query, parameters.Length, true);
			result.AddParameters(paremetersExtra);
			return result;
		}

		/// <summary>
		/// Generate Filters for Simple queries 
		/// </summary>
		private static IDbParameter[] GetFilters(IDbConnection connection, BulkRetrieveQuery bulkQuery, StringBuilder query,
			int baseParamIndex, bool createParameter)
		{
			IDbParameter[] result = createParameter ? new IDbParameter[bulkQuery.GetBindVariableCount()] : null;
			var paramIndex = baseParamIndex;
			var bindVariableId = 0;
			if (query == null) return Constants.DefaultFilters;
			if (baseParamIndex > 0) query.Append(Constants.And);
			for (var i = 0; i < bulkQuery.Filters.Count; ++i)
			{
				var filter = bulkQuery.Filters[i];
				SetLeftOperand(connection, filter, query);

				// operator
				query.Append(GetOperator(filter));

				#region rigth operand

				if (filter.Operation != OperationType.In)
				{
					if (filter.Operation == OperationType.Equal && filter.Operand == null) query.Append(Constants.NullOperand);
					else
					{
						query.Append(Constants.BindVariablePrefix);
						query.Append(Constants.BindVariableName);
						query.Append(paramIndex.ToString());
						// TODO TEST !!!
						if (createParameter)
							result[bindVariableId] = GetParameter(connection, filter.Field.Type,
								Constants.BindVariableName + paramIndex.ToString(),
								filter.CaseSensitiveSearch ? filter.Field.GetSearchableValue(filter.Operand) : filter.Operand);
						++paramIndex;
						++bindVariableId;
					}
				}
				else
				{
					query.Append(Constants.StartParenthesis);
					// manage cbIn
					for (var j = 0; j < filter.Operands.Length; ++j)
					{
						query.Append(Constants.BindVariablePrefix);
						query.Append(Constants.BindVariableName);
						query.Append(paramIndex.ToString());
						query.Append(Constants.SelectSeparator);
						if (createParameter)
							result[bindVariableId] = GetParameter(connection, filter.Field.Type,
								Constants.BindVariableName + paramIndex.ToString(),
								filter.CaseSensitiveSearch ? filter.Field.GetSearchableValue(filter.Operands[j]) : filter.Operands[j]);
						++paramIndex;
						++bindVariableId;
					}
					query.Length = query.Length - 1;
					query.Append(Constants.EndParenthesis);
				}

				#endregion

				if (i < bulkQuery.Filters.Count - 1) query.Append(Constants.And);
			}
			return result;
		}

		/// <summary>
		/// Generate filters for traverseFromParent
		/// </summary>
		private static IDbParameter[] GetFilters(IDbConnection connection, BulkRetrieveQuery bulkQuery, StringBuilder query,
			Relation relation)
		{
			if (query == null) return Constants.DefaultFilters;
			IDbParameter[] result;
			long[] distinctId;
			string fieldCriterion;
			var afterMtmQuery = false;
			var targetMtmTable = bulkQuery.TargetObject.Type == TableType.Mtm;

			if (RelationType.Mtm == relation.Type && !targetMtmTable)
			{
				// second query after query on mtm table
				distinctId = bulkQuery.ParentQuery.GetDistinctRelationId(relation.Name);
				fieldCriterion = bulkQuery.TargetObject.PrimaryKey.Name;
				afterMtmQuery = true;
			}
			else
			{
				distinctId = relation.Type == RelationType.Mto || relation.Type == RelationType.Otop
					? bulkQuery.ParentQuery.GetDistinctRelationId(relation.Name)
					: bulkQuery.ParentQuery.GetDistinctId();
				fieldCriterion = relation.Type == RelationType.Mto || relation.Type == RelationType.Otop
					? bulkQuery.TargetObject.PrimaryKey.Name
					: relation.InverseRelationName;
			}

			// Cases ==>  (1) =,(2) In,(3) Subqueries,(4) Multiple queries
			if (distinctId.Length == 1)
			{
				#region 1 record

				// ok for MTM 
				result = new IDbParameter[1];
				query.Append(fieldCriterion);
				query.Append(Constants.Equal);
				query.Append(Constants.BindVariablePrefix);
				query.Append(Constants.FirstBindVariableName);
				result[0] = GetParameter(connection, Constants.FirstBindVariableName, distinctId[0]);
				return result; // we stop here

				#endregion
			}
			else if (distinctId.Length <= Constants.MaxInElement)
			{
				#region >1 && <=255 records

				// ok for MTM 
				result = new IDbParameter[distinctId.Length];
				query.Append(fieldCriterion);
				query.Append(Constants.In);
				query.Append(Constants.StartParenthesis);
				for (var i = 0; i < distinctId.Length; ++i)
				{
					query.Append(Constants.BindVariablePrefix);
					query.Append(Constants.BindVariableName);
					query.Append(i.ToString());
					query.Append(Constants.SelectSeparator);
					result[i] = GetParameter(connection, Constants.BindVariableName + (i).ToString(), distinctId[i]);
				}
				query.Length = query.Length - 1;
				query.Append(Constants.EndParenthesis);
				return result; // we stop here

				#endregion
			}
			else if (!bulkQuery.ParentQuery.SubQuery && !bulkQuery.ParentQuery.MultipleQuery)
			{
				#region >255 records generate sub-query

				// Huge queries - parent cannot be a subquery already (max 2 levels)
				//  more than > 255: use subquery 
				string rigthFieldCriterion;
				if (targetMtmTable) rigthFieldCriterion = relation.To.PrimaryKey.Name;
				else if (afterMtmQuery) rigthFieldCriterion = relation.Name;
				else
					rigthFieldCriterion = relation.Type == RelationType.Mto || relation.Type == RelationType.Otop
						? relation.InverseRelationName
						: bulkQuery.TargetObject.PrimaryKey.Name;

				bulkQuery.SubQuery = true; // max two level 

				// re-regenerate parameters
				query.Append(fieldCriterion);
				query.Append(Constants.In);
				query.Append(Constants.StartParenthesis);
				query.Append(Constants.Select);
				query.Append(rigthFieldCriterion);
				query.Append(Constants.From);
				query.Append(bulkQuery.ParentQuery.TargetObject.PhysicalName);
				query.Append(Constants.Where);
				if (afterMtmQuery) result = GetFilters(connection, bulkQuery.ParentQuery, query, bulkQuery.Relation);
				else result = GetFilters(connection, bulkQuery.ParentQuery, query, 0, true);
				query.Append(Constants.EndParenthesis);
				return result; // we stop here

				#endregion
			}
			else
			{
				#region >255 generate multiple query

				bulkQuery.MultipleQuery = true; // multiple query 
				bulkQuery.PartitionSize = Constants.MaxInElement;
				result = new IDbParameter[Constants.MaxInElement];
				query.Append(fieldCriterion);
				query.Append(Constants.In);
				query.Append(Constants.StartParenthesis);
				for (var i = 0; i < Constants.MaxInElement; ++i)
				{
					query.Append(Constants.BindVariablePrefix);
					query.Append(Constants.BindVariableName);
					query.Append(i.ToString());
					query.Append(Constants.SelectSeparator);
					result[i] = GetParameter(connection, Constants.BindVariableName + i.ToString(), distinctId[i]);
				}
				query.Length = query.Length - 1;
				bulkQuery.Parameters = distinctId;
				query.Append(Constants.EndParenthesis);

				#endregion
			}
			return result;
		}

		/// <summary>
		/// Generate sql string  from operationType
		/// </summary>
		private static string GetOperator(BulkRetrieveFilter filter)
		{
			//TODO: replace by Dictionary search to test 
			switch (filter.Operation)
			{
				case OperationType.Equal:
					if (filter.Operand == null) return Constants.IsOpp;
					return Constants.Equal;
				case OperationType.NotEqual: return Constants.NotEqual;
				case OperationType.Greater: return Constants.Greater;
				case OperationType.GreaterOrEqual: return Constants.GreaterOrEqual;
				case OperationType.Less: return Constants.Less;
				case OperationType.LessOrEqual: return Constants.LessOrEqual;
				case OperationType.Like: return Constants.Like;
				case OperationType.NotLike: return Constants.NotLike;
				//case OperationType.SoundsLike:
				//TODO not implemented in sqlite 
				//break;
				case OperationType.In: return Constants.In;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// Get parameters for relations
		/// </summary>
		private static IDbParameter GetParameter(IDbConnection connection, string parameterName, long value) =>
			connection.CreateNewParameterInstance(DbType.Int64, parameterName, value.ToString());

		/// <summary>
		/// Get parameters
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static IDbParameter GetParameter(IDbConnection connection, FieldType fieldType, string parameterName, string value)
		{
			//TODO: replace by Dictionary search to test 
			switch (fieldType)
			{
				case FieldType.Long:
				case FieldType.Int:
				case FieldType.Short:
				case FieldType.Byte:
					return connection.CreateNewParameterInstance(DbType.Int64, parameterName, value);
				case FieldType.Boolean:
					//if (bool.FalseString.Equals(value, StringComparison.OrdinalIgnoreCase)) value = Constants.FalseString;
					//if (bool.TrueString.Equals(value, StringComparison.OrdinalIgnoreCase)) value = Constants.TrueString;
					return connection.CreateNewParameterInstance(DbType.Bool, parameterName, value);
				case FieldType.Float:
				case FieldType.Double:
					return connection.CreateNewParameterInstance(DbType.Double, parameterName, value);
				case FieldType.String:
				case FieldType.Array:
					// manage clob 
					return connection.CreateNewParameterInstance(DbType.String, parameterName, value);
				case FieldType.ShortDateTime:
				case FieldType.DateTime:
				case FieldType.LongDateTime:
					return connection.CreateNewParameterInstance(DbType.DateTime, parameterName, value);
				case FieldType.NotDefined:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			return null;
		}

		/// <summary>
		/// Get RowId (physicale address) from a provider 
		/// </summary>
		private static string GetRowId(DatabaseProvider databaseProvider)
		{
			var result = string.Empty;
			switch (databaseProvider)
			{
				case DatabaseProvider.Sqlite:
					result = Constants.SqliteRowid;
					break;
				case DatabaseProvider.PostgreSql:
					result = Constants.PostgreRowid;
					break;
			}
			return result;
		}

		/// <summary>
		/// Get insert queries 
		/// </summary>
		private static IDbCommand GetInsertQuery(IDbConnection connection, BulkSaveQuery query)
		{
			var result = connection.CreateNewCommandInstance();
			var paramCount = 0;
			var sqlBindVariable = new StringBuilder();
			var sqlFields = query.CurrentRecord.Table.GetInsertClause();
			var fieldList = query.CurrentRecord.Table.FieldsById;
			var parameters = new List<IDbParameter>();

			#region add fields

			//TODO optimise here !! difference between O(n log n) search by name (can be O(n))
			for (var i = 0; i < fieldList.Length; ++i)
			{
				var field = fieldList[i];
				sqlBindVariable.Append(Constants.BindVariablePrefix);
				sqlBindVariable.Append(Constants.BindVariableName);
				sqlBindVariable.Append(paramCount.ToString());
				sqlBindVariable.Append(Constants.SelectSeparator);
				parameters.Add(GetParameter(connection, field.Type, Constants.BindVariableName + paramCount.ToString(),
					query.CurrentRecord.GetField(field.Name)));
				++paramCount;
				if (field.Type == FieldType.String && !field.CaseSensitive)
				{
					sqlBindVariable.Append(Constants.BindVariablePrefix);
					sqlBindVariable.Append(Constants.BindVariableName);
					sqlBindVariable.Append(paramCount.ToString());
					sqlBindVariable.Append(Constants.SelectSeparator);
					parameters.Add(GetParameter(connection, field.Type,
						Constants.BindVariableName + paramCount.ToString(),
						field.GetSearchableValue(query.CurrentRecord.GetField(field.Name))));
					++paramCount;
				}
			}

			#endregion

			#region add relations

			// relation exist - is there relation ? 
			if (query.CurrentRecord.IsDirty)
			{
				var relationChanges = query.CurrentRecord.GetRelationChanges();
				if (relationChanges.Length > 0)
				{
					var tempFields = new StringBuilder(sqlFields);
					for (var i = 0; i < relationChanges.Length; ++i)
					{
						var relation = relationChanges[i];
						sqlBindVariable.Append(Constants.BindVariablePrefix);
						sqlBindVariable.Append(Constants.BindVariableName);
						sqlBindVariable.Append(paramCount.ToString());
						sqlBindVariable.Append(Constants.SelectSeparator);
						parameters.Add(GetParameter(connection, FieldType.Long, Constants.BindVariableName + paramCount.ToString(),
							query.CurrentRecord.GetRelation(relation.Name).ToString()));
						if (tempFields.Length > 0) tempFields.Append(Constants.SelectSeparator);
						tempFields.Append(relation.Name);
						++paramCount;
					}
					sqlFields = tempFields.ToString();
				}
			}

			#endregion

			// remove last BIND_VARIABLE_SEPARATOR
			sqlBindVariable.Remove(sqlBindVariable.Length - 1, 1);
			// add relation - into sa.table_{0} ({1}) values ({2})
			result.CommandText = string.Format(Constants.InsertQuery, query.CurrentRecord.Table.PhysicalName, sqlFields,
				sqlBindVariable);
			result.AddParameters(parameters.ToArray());
			return result;
		}

		/// <summary>
		/// Get delete query 
		/// </summary>
		private static IDbCommand GetDeleteQuery(IDbConnection connection, BulkSaveQuery query)
		{
			var result = connection.CreateNewCommandInstance();
			switch (query.CurrentRecord.Table.Type)
			{
				case TableType.Mtm:
					//query.CurrentRecord.Table.IsMtmTable()
					var relationChanges = query.CurrentRecord.GetRelationChanges();
					if (relationChanges.Length < 2) return null;
					var relation1 = relationChanges[0].Name;
					var relation2 = relationChanges[1].Name;
					result.CommandText = string.Format(Constants.DeleteMtmQuery,
						query.CurrentRecord?.Table.PhysicalName, relation1, relation2);
					result.AddParameter(GetParameter(connection, FieldType.Long,
						Constants.FirstBindVariableName, query.CurrentRecord.GetRelation(relation1).ToString()));
					result.AddParameter(GetParameter(connection, FieldType.Long,
						Constants.SecondBindVariableName, query.CurrentRecord.GetRelation(relation2).ToString()));
					break;
				case TableType.MetaId:
					//else if (query.CurrentRecord.Table.IsMetaTableId())
					result.CommandText = string.Format(Constants.DeleteMetaIdQuery, query.CurrentRecord.Table.PhysicalName);
					result.AddParameter(GetParameter(connection, FieldType.Long,
						Constants.BindVariableName + 1.ToString(), query.CurrentRecord.GetField(Constants.MetaDataId)));
					result.AddParameter(GetParameter(connection, FieldType.Long,
						Constants.BindVariableName + 2.ToString(), query.CurrentRecord.GetField(Constants.MetaDataSchemaId)));
					result.AddParameter(GetParameter(connection, FieldType.Long,
						Constants.BindVariableName + 3.ToString(), query.CurrentRecord.GetField(Constants.MetaDataObjectType)));
					break;
				case TableType.Meta:
					//else if (query.CurrentRecord.Table.IsMetaTable())
					result.CommandText = string.Format(Constants.DeleteMetaQuery, query.CurrentRecord.Table.PhysicalName);
					result.AddParameter(GetParameter(connection, FieldType.Long,
						Constants.BindVariableName + 1.ToString(), query.CurrentRecord.GetField(Constants.MetaDataId)));
					result.AddParameter(GetParameter(connection, FieldType.Long,
						Constants.BindVariableName + 2.ToString(), query.CurrentRecord.GetField(Constants.MetaDataSchemaId)));
					result.AddParameter(GetParameter(connection, FieldType.Long,
						Constants.BindVariableName + 3.ToString(), query.CurrentRecord.GetField(Constants.MetaDataObjectType)));
					result.AddParameter(GetParameter(connection, FieldType.Long,
						Constants.BindVariableName + 4.ToString(), query.CurrentRecord.GetField(Constants.MetaDataRefId)));
					break;
				case TableType.LexiconItem:
					result.CommandText = string.Format(Constants.DeleteLexiconItemQuery, query.CurrentRecord.Table.PhysicalName);
					result.AddParameter(GetParameter(connection, FieldType.Long,
						Constants.BindVariableName + 0.ToString(), query.CurrentRecord.GetField(Constants.LexiconItemLexiId)));
					break;
				default:
					result.CommandText = string.Format(Constants.DeleteQuery,
						query.CurrentRecord.Table.PhysicalName, query.CurrentRecord.Table.PrimaryKey.Name);
					result.AddParameter(GetParameter(connection, query.CurrentRecord.Table.PrimaryKey.Type,
						Constants.FirstBindVariableName, query.CurrentRecord.GetField()));
					// integrity issues ? 
					//TODO: delete MTM tables 
					break;
			}
			return result;
		}

		/// <summary>
		/// Insert mtm relation if exists
		/// </summary>
		private static IDbCommand GetInsertMtmQuery(IDbConnection connection, BulkSaveQuery query)
		{
			//TODO optimize later
			var result = connection.CreateNewCommandInstance();
			var relationChanges = query.CurrentRecord.GetRelationChanges();
			var sqlBindVariable = new StringBuilder();
			var sqlFields = new StringBuilder();
			var paramCount = 0;
			var parameters = new List<IDbParameter>();

			if (relationChanges.Length <= 0) return null; // try an exception
			// no need a loop here !!!!
			for (var i = 0; i < relationChanges.Length; ++i)
			{
				var relation = relationChanges[i];
				sqlBindVariable.Append(Constants.BindVariablePrefix);
				sqlBindVariable.Append(Constants.BindVariableName);
				sqlBindVariable.Append(paramCount.ToString());
				sqlBindVariable.Append(Constants.SelectSeparator);
				parameters.Add(GetParameter(connection, FieldType.Long, Constants.BindVariableName + paramCount.ToString(),
					query.CurrentRecord.GetRelation(relation.Name).ToString()));
				sqlFields.Append(relation.Name);
				sqlFields.Append(Constants.SelectSeparator);
				++paramCount;
			}
			sqlBindVariable.Length = sqlBindVariable.Length - 1;
			sqlFields.Length = sqlFields.Length - 1;
			// check conflict or not ??
			if (query.Type == BulkSaveType.InsertMtm)
				result.CommandText = string.Format(Constants.InsertQuery, query.CurrentRecord.Table.PhysicalName,
					sqlFields, sqlBindVariable);
			else
				result.CommandText = string.Format(Constants.InsertQuery +
				                                   Constants.Space +
				                                   query.CurrentRecord.Table.GetConflictClause(connection.Provider,
					                                   sqlFields.ToString()),
					query.CurrentRecord.Table.PhysicalName, sqlFields, sqlBindVariable);
			result.AddParameters(parameters.ToArray());
			return result;
		}

		/// <summary>
		/// Update relation & relate record 
		/// </summary>
		private static IDbCommand GetUpdateQuery(IDbConnection connection, BulkSaveQuery query)
		{
			var result = connection.CreateNewCommandInstance();
			//TODO optimize later
			var parameters = new List<IDbParameter>();
			var paramCount = 0;

			if (query.CurrentRecord.Table.Type == TableType.MetaId) return null;

			var fieldChanges = query.Type == BulkSaveType.RelateRecords
				? new Field[0]
				: query.CurrentRecord.GetFieldChanges();
			var relationChanges = query.CurrentRecord.GetRelationChanges();
			if (fieldChanges.Length + relationChanges.Length == 0) return null;
			// manage where clause on objid 
			var whereUpdValue = query.CurrentRecord.Table.PrimaryKey.Name + Constants.Equal +
			                       Constants.BindVariablePrefix + Constants.FirstBindVariableName;
			parameters.Add(GetParameter(connection, FieldType.Long,
				Constants.FirstBindVariableName, query.CurrentRecord.GetField()));
			++paramCount;
			var tempSetValue = new StringBuilder();

			// build set element - for fields

			for (var i = 0; i < fieldChanges.Length; ++i)
			{
				var field = fieldChanges[i];
				tempSetValue.Append(field.Name);
				tempSetValue.Append(Constants.Equal);
				tempSetValue.Append(Constants.BindVariablePrefix);
				tempSetValue.Append(Constants.BindVariableName);
				tempSetValue.Append(paramCount.ToString());
				tempSetValue.Append(Constants.SelectSeparator);
				parameters.Add(GetParameter(connection, field.Type, Constants.BindVariableName + paramCount.ToString(), query.CurrentRecord.GetField(field.Name)));
				++paramCount;

				if (field.Type == FieldType.String && !field.CaseSensitive)
				{
					tempSetValue.Append(Schema.Helpers.SqlHelper.GetSearchableFieldName(field));
					tempSetValue.Append(Constants.Equal);
					tempSetValue.Append(Constants.BindVariablePrefix);
					tempSetValue.Append(Constants.BindVariableName);
					tempSetValue.Append(paramCount.ToString());
					tempSetValue.Append(Constants.SelectSeparator);
					parameters.Add(GetParameter(connection, field.Type,
						Constants.BindVariableName + paramCount.ToString(),
						field.GetSearchableValue(field.GetSearchableValue(
							query.CurrentRecord.GetField(field.Name)))));
					++paramCount;
				}
			}

			// build set element - for relations

			for (var i = 0; i < relationChanges.Length; ++i)
			{
				var relation = relationChanges[i];
				var relationValue = query.CurrentRecord.GetRelation(relation.Name).ToString();
				if (relationValue == Constants.NullRelation) relationValue = null;
				tempSetValue.Append(relation.Name);
				tempSetValue.Append(Constants.Equal);
				tempSetValue.Append(Constants.BindVariablePrefix);
				tempSetValue.Append(Constants.BindVariableName);
				tempSetValue.Append(paramCount.ToString());
				tempSetValue.Append(Constants.SelectSeparator);
				parameters.Add(GetParameter(connection, FieldType.Long, Constants.BindVariableName + paramCount.ToString(), relationValue));
				++paramCount;
			}

			tempSetValue.Length = tempSetValue.Length - 1;
			var setUpdValue = tempSetValue.ToString();
			result.CommandText = string.Format(Constants.UpdateQuery, query.CurrentRecord.Table.PhysicalName, setUpdValue,
				whereUpdValue);
			result.AddParameters(parameters.ToArray());
			return result;
		}

		/// <summary>
		/// Get left operand for filter
		/// </summary>
		private static void SetLeftOperand(IDbConnection connection, BulkRetrieveFilter filter, StringBuilder query)
		{
			if (!filter.CaseSensitiveSearch)
				query.Append(Schema.Helpers.SqlHelper.GetFieldName(filter.Field, connection.Provider, false));
			else if (filter.Field.Type == FieldType.String && !filter.Field.CaseSensitive)
				query.Append(Schema.Helpers.SqlHelper.GetSearchableFieldName(filter.Field));
			else query.Append(filter.Field.Name);
		}


		#endregion

	}
}