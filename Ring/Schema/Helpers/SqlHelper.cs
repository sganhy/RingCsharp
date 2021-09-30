using Ring.Data;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Database = Ring.Schema.Models.Schema;

namespace Ring.Schema.Helpers
{
	/// <summary>
	///     Sql DDL statements generator for the schema
	/// </summary>
	internal static class SqlHelper
	{
		/// <summary>
		///     Add table
		/// </summary>
		internal static void CreateTable(IDbConnection connection, Table table, TableSpace tableSpace)
		{
			using (var cmd = connection.CreateNewCommandInstance())
			{
				cmd.Connection = connection;
				cmd.CommandText = ToTable(connection.Provider, table, DatabaseOperation.Create, tableSpace);
				cmd.ExecuteNonQuery();
				cmd.Connection = null;
			}
		}

		/// <summary>
		///     Create primary key on table
		/// </summary>
		internal static void CreatePrimaryKey(IDbConnection connection, Table table, TableSpace tableSpace)
		{
			using (var cmd = connection.CreateNewCommandInstance())
			{
				cmd.Connection = connection;
				cmd.CommandText = ToConstaint(connection.Provider, table, null, ConstraintType.PrimaryKey, DatabaseOperation.Create, tableSpace);
				if (!string.IsNullOrEmpty(cmd.CommandText)) cmd.ExecuteNonQuery();
				cmd.Connection = null;
			}
		}

		/// <summary>
		///     Add primary key on table
		/// </summary>
		internal static void CreateForeignKey(IDbConnection connection, Relation relation)
		{
			if (relation.Type == RelationType.Otof || relation.Type == RelationType.Otm) return;
			using (var cmd = connection.CreateNewCommandInstance())
			{
				cmd.Connection = connection;
				cmd.CommandText = ToConstaint(connection.Provider, relation.Type!=RelationType.Mtm ? relation.GetCurrentTable(): relation.GetMtmTable(), relation,
							ConstraintType.ForeignKey, DatabaseOperation.Create,null);
				if (!string.IsNullOrEmpty(cmd.CommandText)) cmd.ExecuteNonQuery();
				cmd.Connection = null;
			}
		}

		/// <summary>
		///     Add schema
		/// </summary>
		internal static void CreateSchema(IDbConnection connection, string schemaName)
		{
			using (var cmd = connection.CreateNewCommandInstance())
			{
				cmd.Connection = connection;
				cmd.CommandText = ToSchema(connection.Provider, DatabaseOperation.Create, schemaName);
				if (!string.IsNullOrEmpty(cmd.CommandText)) cmd.ExecuteNonQuery();
				cmd.Connection = null;
			}
		}

		/// <summary>
		///     Add index
		/// </summary>
		internal static void CreateIndex(IDbConnection connection, Table table, Index index, TableSpace tableSpace)
		{
			using (var cmd = connection.CreateNewCommandInstance())
			{
				cmd.Connection = connection;
				cmd.CommandText = ToIndex(connection.Provider, table, index, DatabaseOperation.Create, tableSpace);
				if (!string.IsNullOrEmpty(cmd.CommandText)) cmd.ExecuteNonQuery();
				cmd.Connection = null;
			}
		}

		/// <summary>
		///     Drop index
		/// </summary>
		internal static void DropIndex(IDbConnection connection, Database schema, Table table, Index index)
		{
			using (var cmd = connection.CreateNewCommandInstance())
			{
				cmd.Connection = connection;
				cmd.CommandText = ToIndex(connection.Provider, table, index, DatabaseOperation.Delete, null);
				if (!string.IsNullOrEmpty(cmd.CommandText)) cmd.ExecuteNonQuery();
				cmd.Connection = null;
			}
		}

		/// <summary>
		///     Add index
		/// </summary>
		internal static void CreateTableSpace(IDbConnection connection, TableSpace tableSpace)
		{
			using (var cmd = connection.CreateNewCommandInstance())
			{
				cmd.Connection = connection;
				cmd.CommandText = ToTableSpace(connection.Provider, DatabaseOperation.Create, tableSpace);
				if (!string.IsNullOrEmpty(cmd.CommandText)) cmd.ExecuteNonQuery();
				cmd.Connection = null;
			}
		}

		/// <summary>
		///     Truncate table
		/// </summary>
		internal static void TruncateTable(IDbConnection connection, Table table)
		{
			using (var cmd = connection.CreateNewCommandInstance())
			{
				cmd.Connection = connection;
				cmd.CommandText = string.Format(Constants.TruncateTable, table.PhysicalName);
				if (!string.IsNullOrEmpty(cmd.CommandText)) cmd.ExecuteNonQuery();
				cmd.Connection = null;
			}
		}

		/// <summary>
		///     Alter table
		/// </summary>
		internal static void AlterTable(IDbConnection connection, Table table, BaseEntity entity, DatabaseOperation operation)
		{
			if (connection == null || table == null || entity == null) return;
			var field = entity as Field;
			using (var cmd = connection.CreateNewCommandInstance())
			{
				cmd.Connection = connection;
				cmd.CommandText = ToTable(connection.Provider,  table, entity, operation);
				cmd.ExecuteNonQuery();
				if (field?.CaseSensitive == false && field.Type == FieldType.String)
				{
					cmd.CommandText = ToTable(connection.Provider, table, entity, operation, true);
					cmd.ExecuteNonQuery();
				}
				cmd.Connection = null;
			}
		}

		/// <summary>
		///     Drop table
		/// </summary>
		internal static void DropTable(IDbConnection connection, Table table)
		{
			// we tested before if table exists !!!!!!
			using (var cmd = connection.CreateNewCommandInstance())
			{
				cmd.Connection = connection;
				cmd.CommandText = ToTable(connection.Provider, table, DatabaseOperation.Delete, null);
				cmd.ExecuteNonQuery();
				cmd.Connection = null;
			}
		}

		/// <summary>
		///     Analyze table
		/// </summary>
		internal static void Analyze(IDbConnection connection, Table table)
		{
			using (var cmd = connection.CreateNewCommandInstance())
			{
				cmd.Connection = connection;
				cmd.CommandText = string.Format(Constants.AnalyzeTable, table.PhysicalName);
				cmd.ExecuteNonQuery();
				cmd.Connection = null;
			}
		}

		/// <summary>
		///     Analyze table
		/// </summary>
		internal static void Vacuum(IDbConnection connection, Table table)
		{
			using (var cmd = connection.CreateNewCommandInstance())
			{
				switch (connection.Provider)
				{
					case DatabaseProvider.PostgreSql:
						cmd.Connection = connection;
						cmd.CommandText = string.Format(Constants.VacuumTable, table.PhysicalName);
						cmd.ExecuteNonQuery();
						cmd.Connection = null;
						break;
				}
			}
		}

		/// <summary>
		///     Return searchable field
		/// </summary>
		internal static string GetSearchableFieldName(Field field)
		{
			return Constants.SearchablePrefixClfy + field.Name;
		}

		/// <summary>
		///     Get field name and add abilitty to force case insensitive search
		/// </summary>
		internal static string GetFieldName(Field field, DatabaseProvider provider, bool caseSensitiveSearch)
		{
			if (field == null) return null;
			if (caseSensitiveSearch) return field.Name;
			switch (provider)
			{
				case DatabaseProvider.PostgreSql:
					return string.Format(Constants.PostgreToUpperCase, field.Name);
			}
			return field.Name;
		}

		#region private methods 

		/// <summary>
		///     Generate a script to create an index
		/// </summary>
		private static string ToIndex(DatabaseProvider provider, Table table, Index index, DatabaseOperation databaseOperation, TableSpace tableSpace)
		{
			switch (provider)
			{
				case DatabaseProvider.Sqlite: return PostgreSqlHelper.ToIndex(table, index, databaseOperation, tableSpace);
				case DatabaseProvider.PostgreSql: return SqliteHelper.ToIndex(table, index, databaseOperation, tableSpace);
			}
			return null;
		}

		/// <summary>
		///     Generate a script to create a tablespace 
		/// </summary>
		private static string ToTableSpace(DatabaseProvider provider, DatabaseOperation databaseOperation, TableSpace tableSpace)
		{
			switch (provider)
			{
				case DatabaseProvider.PostgreSql: return PostgreSqlHelper.ToTableSpace(databaseOperation, tableSpace);
			}
			return null;
		}

		/// <summary>
		///     Generate a script to create a schema
		/// </summary>
		private static string ToSchema(DatabaseProvider provider, DatabaseOperation databaseOperation, string schemaName)
		{
			switch (provider)
			{
				case DatabaseProvider.PostgreSql: return PostgreSqlHelper.ToSchema(databaseOperation, schemaName);
			}
			return null;
		}

		/// <summary>
		///     Generate script for a table
		/// </summary>
		private static string ToTable(DatabaseProvider provider, Table table, DatabaseOperation databaseOperation, TableSpace tableSpace)
		{
			switch (provider)
			{
				case DatabaseProvider.Sqlite: return SqliteHelper.ToTable(table, databaseOperation, tableSpace);
				case DatabaseProvider.PostgreSql: return PostgreSqlHelper.ToTable(table, databaseOperation, tableSpace);
			}
			return null;
		}

		/// <summary>
		///     Generate script to alter table 
		/// </summary>
		private static string ToTable(DatabaseProvider provider, Table table, BaseEntity entity, DatabaseOperation operation, bool searchable= false)
		{
			switch (provider)
			{
				case DatabaseProvider.Sqlite: return SqliteHelper.ToTable(table, entity, operation, searchable);
				case DatabaseProvider.PostgreSql: return PostgreSqlHelper.ToTable(table, entity, operation, searchable);
			}
			return null;
		}

		/// <summary>
		///     Generate script for a constraint
		/// </summary>
		private static string ToConstaint(DatabaseProvider provider, Table table, Relation relation, ConstraintType constraintType, DatabaseOperation databaseOperation, 
			TableSpace tablespace)
		{
			switch (provider)
			{
				case DatabaseProvider.PostgreSql:
					return PostgreSqlHelper.ToConstraint(table, relation, constraintType, databaseOperation, tablespace);
			}
			return null;
		}
		
		#endregion
	}
}
