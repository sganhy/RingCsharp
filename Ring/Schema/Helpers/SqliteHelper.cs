using System;
using System.Text;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Schema.Helpers
{
	internal static class SqliteHelper
	{
		internal static string ToFieldType(Field field)
		{
			var result = new StringBuilder();
			switch (field.Type)
			{
				case FieldType.String:
				case FieldType.ShortDateTime:
				case FieldType.DateTime:
				case FieldType.LongDateTime:
				case FieldType.Array:
					result = result.Append(Constants.SqliteString.ToUpper().PadRight(Constants.ScriptTypePadding, Constants.Space));
					break;
				case FieldType.Boolean:
				case FieldType.Byte:
				case FieldType.Short:
				case FieldType.Int:
				case FieldType.Long:
					result = result.Append(Constants.SqliteNumber.ToUpper().PadRight(Constants.ScriptTypePadding, Constants.Space));
					break;
				case FieldType.Float:
				case FieldType.Double:
					result = result.Append(Constants.SqliteFloat.ToUpper().PadRight(Constants.ScriptTypePadding, Constants.Space));
					break;
				case FieldType.NotDefined:
					throw new ArgumentException();
				default:
					throw new ArgumentOutOfRangeException();
			}
			var fieldConstraint = ToFieldConstraint(field);
			if (!string.IsNullOrWhiteSpace(fieldConstraint))
			{
				result.Append(Constants.Space);
				result.Append(fieldConstraint);
			}
			return result.ToString();
		}

		internal static string ToIndex(Table table, Index index, DatabaseOperation databaseOperation, TableSpace tableSpace)
		{
			switch (databaseOperation)
			{
				case DatabaseOperation.Create: return CreateIndex(table, index, tableSpace);
				case DatabaseOperation.Delete: return DropIndex(table, index);
			}
			return null;
		}

		internal static string ToTable(Table table, DatabaseOperation databaseOperation, TableSpace tableSpace)
		{
			switch (databaseOperation)
			{
				case DatabaseOperation.Create: return CreateTable(table, tableSpace);
				case DatabaseOperation.Delete: return DropTable(table);
			}
			return null;
		}

		internal static string ToTable(Table table, BaseEntity entity, DatabaseOperation operation, bool searchable)
		{
			return null;
		}

		internal static string GetPrimaryKey(Table table, TableSpace tableSpace)
		{
			var result = new StringBuilder();
			result.Append(Constants.DoubleSpace);
			result.Append(Constants.PrimaryKey.ToUpper().PadRight(Constants.ScriptNamePadding, Constants.Space));
			result.Append(Constants.Space);
			result.Append(Constants.StartClause);
			return result.ToString();
		}

		private static string ToFieldConstraint(Field field)
		{
			var result = new StringBuilder();
			result.Append(Constants.Space);
			var fieldName = field.Name;

			switch (field.Type)
			{
				case FieldType.String:
					if (field.Size > 0 && field.Size < int.MaxValue)
					{
						result.Append(string.Format(Constants.SqliteFieldSize, fieldName, field.Size));
					}
					break;
				case FieldType.Long:
					result.Append(string.Format(Constants.SqliteLongConstraint, field.Name));
					break;
				case FieldType.Int:
					result.Append(string.Format(Constants.SqliteIntConstraint, field.Name));
					break;
				case FieldType.Short:
					result.Append(string.Format(Constants.SqliteShortConstraint, field.Name));
					break;
				case FieldType.Byte:
					result.Append(string.Format(Constants.SqliteByteConstraint, field.Name));
					break;
				case FieldType.ShortDateTime:
				case FieldType.DateTime:
				case FieldType.LongDateTime:
					result.Append(string.Format(Constants.SqliteDateConstraint, fieldName));
					/* default value ==> 0001-01-01T00:00:00Z not working see bug of sqlite driver 
					result.Append(Constants.StringDelimiter);
					result.Append(schemaSource == SchemaSourceType.ClfyXml
						? Constants.DefautDateClfy 
						: Constants.SqliteDefautDate);
					result.Append(Constants.StringDelimiter);
					*/
					break;
				case FieldType.Boolean:
					result.Append(string.Format(Constants.SqliteBoolConstraint, fieldName));
					break;
				case FieldType.Float:
				case FieldType.Double:
					// do nothing 
					break;
				case FieldType.Array:
					break;
				case FieldType.NotDefined:
					throw new ArgumentException();
				default:
					throw new ArgumentOutOfRangeException();
			}

			return result.ToString();
		}

		private static string CreateTable(Table table, TableSpace tableSpace)
		{
			return null;
		}

		private static string ToRelation(Table table, Relation relation, DatabaseOperation databaseOperation)
		{
			var tableName = table.PhysicalName;
			tableName += relation.To.Name;
			var relationName = relation.Name;
			var result = new StringBuilder();
			if (relation.Type == RelationType.Mto || relation.Type == RelationType.Otop ||
			    relation.Type == RelationType.Mtm && table.Type == TableType.Mtm)
			{
				// name
				result.Append(
					(Constants.DoubleQuote + relationName + Constants.DoubleQuote).PadRight(
						Constants.ScriptNamePadding, Constants.Space));
				result.Append(Constants.Space);
				// type
				result.Append(Constants.SqliteNumber.ToUpper().PadRight(Constants.ScriptTypePadding, Constants.Space));
				result.Append(Constants.DoubleSpace);
				// type
				result.Append(Constants.SqliteRelationRef.ToUpper());
				result.Append(Constants.Space);
				result.Append(string.Format(Constants.SqliteTableDelimiter, tableName));
				result.Append(Constants.Space);
				result.Append(string.Format(Constants.SqliteLongConstraint, relation.Name));
				return result.ToString();
			}
			return result.ToString();
		}

		internal static string DropTable(Table table) => string.Format(Constants.DropTable, table.PhysicalName);

		internal static string DropIndex(Table table, Index index)
		{
			var schema = table.GetCurrentSchema();
			return string.Format(Constants.DropIndex, Constants.AlterIfExist,
				schema.SearchPath + Constants.SchemaSeparator + index.GetPhysicalName(table)).Trim();
		}

		private static string CreateIndex(Table table, Index index, TableSpace tableSpace)
		{
			var result = new StringBuilder();
			var indexName = index.GetPhysicalName(table);
			var tableName = table.PhysicalName;

			result.Append(index.Unique ? Constants.CreateUniqueIndex.ToUpper() : Constants.CreateIndex.ToUpper());
			result.Append(Constants.Space);
			result.Append(indexName);
			result.Append(Constants.IndexOnTable);
			result.Append(tableName);
			result.Append(Constants.Space);
			result.Append(Constants.StartClause);
			for (var i = 0; i < index.Fields.Length; ++i)
			{
				var indexFieldObj = index.Fields[i] as Field;
				if (indexFieldObj != null && !indexFieldObj.CaseSensitive)
					result.Append(Constants.SearchablePrefixClfy);
				result.Append(index.Fields[i].Name);
				if (i < index.Fields.Length - 1) result.Append(Constants.CommaSeparator);
			}
			result.Append(Constants.EndClause);
			if (tableSpace == null) return result.ToString();
			result.Append(Constants.Space);
			result.Append(string.Format(Constants.TableSpace, tableSpace.Name));
			return result.ToString();
		}


	}
}
