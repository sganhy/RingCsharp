using System;
using System.Text;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Schema.Helpers
{
	internal static class PostgreSqlHelper
	{
		/// <summary>
		/// Create index
		/// </summary>
		internal static string ToIndex(Table table, Index index, DatabaseOperation databaseOperation, TableSpace tableSpace) => SqliteHelper.ToIndex(table, index, databaseOperation, tableSpace);

		/// <summary>
		/// Create table
		/// </summary>
		internal static string ToTable(Table table, DatabaseOperation databaseOperation, TableSpace tableSpace)
		{
			switch (databaseOperation)
			{
				case DatabaseOperation.Create: return CreateTable(table, tableSpace);
				case DatabaseOperation.Delete: return DropTable(table);
			}
			return null;
		}

		/// <summary>
		/// Alter table
		/// </summary>
		internal static string ToTable(Table table, BaseEntity entity, DatabaseOperation databaseOperation, bool searchable)
		{
			switch (databaseOperation)
			{
				case DatabaseOperation.Create: return AddField(table, entity, searchable);
				case DatabaseOperation.Delete: return RemoveField(table, entity, searchable);
			}
			return null;
		}

		/// <summary>
		/// Create constraint
		/// </summary>
		internal static string ToConstraint(Table table, Relation relation, ConstraintType constraintType, DatabaseOperation databaseOperation, TableSpace tablespace)
		{
			if (databaseOperation == DatabaseOperation.Create && constraintType == ConstraintType.PrimaryKey)
				return CreatePrimaryKey(table, tablespace);
			if (databaseOperation == DatabaseOperation.Create && constraintType == ConstraintType.ForeignKey)
				return CreateForeignKey(table, relation);
			return null;
		}

		#region private methods 

		private static string ToFieldType(Field field)
		{
			var result = new StringBuilder();
			switch (field.Type)
			{
				case FieldType.ShortDateTime:
					result = result.Append(Constants.PostgreDate.ToUpper().PadRight(Constants.ScriptTypePadding, Constants.Space));
					break;
				case FieldType.DateTime:
				case FieldType.LongDateTime:
					result = result.Append(
						Constants.PostgreDateTime.ToUpper().PadRight(Constants.ScriptTypePadding, Constants.Space));
					break;
				case FieldType.String:
					result = result.Append(field.Size <= Constants.PostgreMaxVarcharSize
						? Constants.PostgreString.ToUpper().PadRight(Constants.ScriptTypePadding, Constants.Space)
						: Constants.PostgreBigString.ToUpper().PadRight(Constants.ScriptTypePadding, Constants.Space));
					break;
				case FieldType.Array:
					result = result.Append(Constants.SqliteString.ToUpper().PadRight(Constants.ScriptTypePadding, Constants.Space));
					break;
				case FieldType.Boolean:
					result = result.Append(Constants.PostgreBool.ToUpper().PadRight(Constants.ScriptTypePadding, Constants.Space));
					break;
				case FieldType.Byte:
				case FieldType.Short:
					result = result.Append(Constants.PostgreShort.ToUpper().PadRight(Constants.ScriptTypePadding, Constants.Space));
					break;
				case FieldType.Int:
					result = result.Append(Constants.PostgreInt.ToUpper().PadRight(Constants.ScriptTypePadding, Constants.Space));
					break;
				case FieldType.Long:
					result = result.Append(Constants.PostgreLong.ToUpper().PadRight(Constants.ScriptTypePadding, Constants.Space));
					break;
				case FieldType.Float:
					result = result.Append(Constants.PostgreFloat.ToUpper().PadRight(Constants.ScriptTypePadding, Constants.Space));
					break;
				case FieldType.Double:
					result = result.Append(Constants.PostgreDouble.ToUpper().PadRight(Constants.ScriptTypePadding, Constants.Space));
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

		private static string CreateForeignKey(Table table, Relation relation)
		{
			var result = new StringBuilder();
			if (table.Type != TableType.Mtm)
				result.Append(string.Format(Constants.AddConstraint, table.PhysicalName,
					Constants.ForeignKeyPrefix + table.Id + Constants.Underscore + relation.Name));
			else result.Append(string.Format(Constants.AddConstraint, table.PhysicalName,
				Constants.ForeignKeyPrefix + relation.GetCurrentTable().Id + Constants.Underscore + relation.Name));
			result.Append(Constants.Space);
			result.Append(Constants.ForeignKey.ToUpper());
			result.Append(Constants.StartClause);
			result.Append(relation.Name);
			result.Append(Constants.EndClause);
			result.Append(Constants.Space);
			result.Append(Constants.Reference.ToUpper());
			result.Append(Constants.Space);
			result.Append(relation.To.PhysicalName);
			result.Append(Constants.Space);
			result.Append(Constants.StartClause);
			result.Append(relation.To.PrimaryKey.Name);
			result.Append(Constants.EndClause);
			return result.ToString();
		}
		private static string CreatePrimaryKey(Table table, TableSpace tableSpace)
		{
			var result = new StringBuilder();

			// max length for PostGreSql is 63 bytes (can be a problem for oracle)
			// no primary key on mtm tables 
			result.Append(string.Format(Constants.AddConstraint, table.PhysicalName, Constants.DoubleQuote + Constants.PrimaryKeyPrefix + table.Name + Constants.DoubleQuote));
			result.Append(Constants.Space);
			result.Append(Constants.PrimaryKey.ToUpper());
			result.Append(Constants.StartClause);

			// fields 
			switch (table.Type)
			{
				case TableType.Meta:
					// unique key (1)  - "id","schema_id","object_type","reference_id"
					result.Append(Constants.DoubleQuote + Constants.MetaDataId +
					              Constants.DoubleQuote);
					result.Append(Constants.CommaSeparator);
					result.Append(Constants.DoubleQuote + Constants.MetaDataSchemaId +
					              Constants.DoubleQuote);
					result.Append(Constants.CommaSeparator);
					result.Append(Constants.DoubleQuote + Constants.MetaDataObjectType +
					              Constants.DoubleQuote);
					result.Append(Constants.CommaSeparator);
					result.Append(Constants.DoubleQuote + Constants.MetaDataRefId +
					              Constants.DoubleQuote);
					break;
				case TableType.MetaId:
					// resharper not working here 
					result.Append(Constants.DoubleQuote + Constants.MetaDataId +
					              Constants.DoubleQuote);
					result.Append(Constants.CommaSeparator);
					result.Append(Constants.DoubleQuote + Constants.MetaDataSchemaId +
					              Constants.DoubleQuote);
					result.Append(Constants.CommaSeparator);
					result.Append(Constants.DoubleQuote + Constants.MetaDataObjectType +
					              Constants.DoubleQuote);
					break;
				default:
					result.Append(table.PrimaryKey.Name);
					break;
			}
			result.Append(Constants.EndClause);

			if (tableSpace != null)
			{
				result.Append(Constants.Space);
				result.Append(string.Format(Constants.PostgreTableSpaceIndex, tableSpace.Name));
			}

			return result.ToString();

		}
		internal static string ToTableSpace(DatabaseOperation databaseOperation, TableSpace tableSpace)
		{
			if (databaseOperation == DatabaseOperation.Create) return CreateTablespace(tableSpace);
			return null;
		}

		internal static string ToSchema(DatabaseOperation databaseOperation, string schemaName)
		{
			if (databaseOperation == DatabaseOperation.Create) return string.Format(Constants.CreateSchema, schemaName);
			return null;
		}


		private static string ToFieldConstraint(Field field)
		{
			var result = new StringBuilder();
			switch (field.Type)
			{
				case FieldType.String:
					if (field.Size <= Constants.PostgreMaxVarcharSize)
						result.Append(string.Format(Constants.PostgreStringConstraint, field.Size));
					break;
				case FieldType.Byte:
					result.Append(string.Format(Constants.PostgreByteConstraint, field.Name));
					break;
			}
			return result.ToString();
		}
		private static string ToField(Field field, DatabaseOperation databaseOperation, bool searchable = false)
		{
			var result = new StringBuilder();
			var fieldName = searchable ? Constants.SearchablePrefixClfy + field.Name : field.Name;

			switch (databaseOperation)
			{
				case DatabaseOperation.Create:
					result.Append(fieldName.PadRight(Constants.ScriptNamePadding, Constants.Space));
					result.Append(Constants.Space);
					result.Append(ToFieldType(field));
					break;
				case DatabaseOperation.Delete:
					result.Append(fieldName);
					break;
			}
			return result.ToString();
		}
		private static string CreateTable(Table table, TableSpace tableSpace)
		{
			var tableName = table.PhysicalName;
			var result = new StringBuilder();
			Field field;
			Relation relation;
			result.Append(string.Format(Constants.CreateTable.ToUpper(), GetCreateTableParameter(table)));
			result.Append(Constants.Space);
			result.Append(tableName);
			result.AppendLine(string.Empty);
			result.Append(Constants.StartClause);
			result.AppendLine(string.Empty);
			
			// sort 
			var fieldList = (Field[])table.FieldsById.Clone(); // shallow copy to avoid to change order of Table.Fields [] 
			var relationList =
				(Relation[])table.Relations.Clone(); // shallow copy to avoid to change order of Table.Fields [] 
			Array.Sort(relationList, (x, y) => x.Id - y.Id);

			// fields 
			for (var i = 0; i < fieldList.Length; ++i)
			{
				field = fieldList[i];
				result.Append(ToField(field, DatabaseOperation.Create));
				result.Append(Constants.CommaSeparator);
				result.AppendLine(string.Empty); // CRLF 
				if (field.Type != FieldType.String || field.CaseSensitive) continue;
				result.Append(ToField(field, DatabaseOperation.Create, true));
				result.Append(Constants.CommaSeparator);
				result.AppendLine(string.Empty); // CRLF 
			}

			// relations
			for (var i = 0; i < relationList.Length; ++i)
			{
				relation = relationList[i];
				var relationBaseScript = ToRelation(table, relation, DatabaseOperation.Create);
				if (relationBaseScript == null) continue;
				result.Append(ToRelation(table, relation, DatabaseOperation.Create));
				result.Append(Constants.CommaSeparator);
				result.AppendLine(string.Empty); // CRLF 
			}

			result.Length = result.ToString().LastIndexOf(Constants.CommaSeparator);
			var storageParameters = GetStorageParameters(table);
			result.AppendLine(Constants.EndClause.ToString());
			if (!string.IsNullOrEmpty(storageParameters)) result.AppendLine(storageParameters);
			if (tableSpace != null) result.AppendLine(string.Format(Constants.TableSpace, tableSpace.Name));
			result.Append(Constants.EndStatement);
			return result.ToString();
		}
		private static string CreateTablespace(TableSpace tableSpace)
		{
			var result = string.Format(Constants.PostgreCreateTableSpace, tableSpace.Name);
			if (!string.IsNullOrWhiteSpace(tableSpace.FileName))
				result += string.Format(Constants.PostgreTableSpaceLocation, tableSpace.FileName);
			return result;
		}

		private static string DropTable(Table table) => SqliteHelper.DropTable(table);

		/// <summary>
		///     Generate create table parameters
		/// </summary>
		private static string GetCreateTableParameter(Table table)
		{
			if (table == null) return null;
			if (table.Unlogged) return Constants.CreateUnloggedTable;
			return string.Empty;
		}

		/// <summary>
		///     Generate storage parameters
		/// </summary>
		private static string GetStorageParameters(Table table)
		{
			if (table == null) return null;
			return Constants.PostgreStorageParameters;
		}

		internal static string AddField(Table table, BaseEntity entity, bool searchable)
		{
			var result = new StringBuilder();
			var field = entity as Field;
			var relation = entity as Relation;
			result.Append(string.Format(Constants.AlterTableAddColumn, table.PhysicalName));
			if (field != null) result.Append(ToField(field, DatabaseOperation.Create, searchable));
			if (relation != null) result.Append(ToRelation(table, relation, DatabaseOperation.Create));
			return result.ToString();
		}

		internal static string RemoveField(Table table, BaseEntity entity, bool searchable)
		{
			return null;
		}

		/// <summary>
		///     Add relationship SQL script (name + data_type)
		/// </summary>
		internal static string ToRelation(Table table, Relation relation, DatabaseOperation databaseOperation)
		{
			var result = new StringBuilder();
			result.Append(Constants.DoubleSpace);
			var relationName = relation.Name;
			var targetType = Constants.PostgreLong.ToUpper();
			switch (relation.To.PrimaryKey.Type)
			{
				case FieldType.Int:
					targetType = Constants.PostgreInt.ToUpper();
					break;
				case FieldType.Short:
					targetType = Constants.PostgreShort.ToUpper();
					break;
			}


			switch (databaseOperation)
			{
				case DatabaseOperation.Delete:
					return relation.Name;
				case DatabaseOperation.Create:

					if (relation.Type == RelationType.Mto || relation.Type == RelationType.Otop ||
						relation.Type == RelationType.Mtm && table.Type == TableType.Mtm)
					{
						// name
						result.Append(relationName.PadRight(Constants.ScriptNamePadding, Constants.Space));
						result.Append(Constants.Space);
						// type
						result.Append(targetType.PadRight(Constants.ScriptTypePadding, Constants.Space));
						//
						result.Append(Constants.DoubleSpace);
						if (relation.NotNull && relation.Type == RelationType.Mtm) result.Append(Constants.NotNull);
						return result.ToString();
					}
					break;
			}
			return null;
		}

		#endregion
	}
}
