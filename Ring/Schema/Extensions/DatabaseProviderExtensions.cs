using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Helpers;

namespace Ring.Schema.Extensions;

internal static class DatabaseProviderExtensions
{
	// reserved key words 
	private static readonly Lazy<HashSet<string>> OracleWords = new(() => ResourceHelper.GetReservedWords(DatabaseProvider.Oracle), true); 
	private static readonly Lazy<HashSet<string>> PostgreSqlWords = new(() => ResourceHelper.GetReservedWords(DatabaseProvider.PostgreSql), true);
	private static readonly Lazy<HashSet<string>> MySqlWords = new(() => ResourceHelper.GetReservedWords(DatabaseProvider.MySql), true);
	private static readonly Lazy<HashSet<string>> SqlServerWords = new(() => ResourceHelper.GetReservedWords(DatabaseProvider.SqlServer), true);
	private static readonly Lazy<HashSet<string>> SqlLiteWords = new(() => ResourceHelper.GetReservedWords(DatabaseProvider.SqlLite), true);

	// catalogs
	private static readonly Dictionary<EntityType, Catalog> PostreSqlCatalog = new() {
		{ EntityType.Table, new Catalog { FieldSchemaName="table_schema", FieldEntityName= "table_name", ViewName="tables" } }
	};

	internal static IDdlBuilder GetDdlBuilder(this DatabaseProvider provider)
	{
		// Code size: 78 (0x4e)
		switch (provider)
		{
			case DatabaseProvider.Oracle: return new Util.Builders.Oracle.DdlBuilder();
			case DatabaseProvider.PostgreSql: return new Util.Builders.PostgreSQL.DdlBuilder();
			case DatabaseProvider.MySql: return new Util.Builders.MySQL.DdlBuilder();
			case DatabaseProvider.SqlServer: return new Util.Builders.SQLServer.DdlBuilder();
			case DatabaseProvider.SqlLite: return new Util.Builders.SQLite.DdlBuilder();
		}
		throw new NotImplementedException();
	}

	internal static IDmlBuilder GetDmlBuilder(this DatabaseProvider provider)
	{
		// Code size: 78 (0x4e)
		switch (provider)
		{
			case DatabaseProvider.Oracle: return new Util.Builders.Oracle.DmlBuilder();
			case DatabaseProvider.PostgreSql: return new Util.Builders.PostgreSQL.DmlBuilder();
			case DatabaseProvider.MySql: return new Util.Builders.MySQL.DmlBuilder();
			case DatabaseProvider.SqlServer: return new Util.Builders.SQLServer.DmlBuilder();
			case DatabaseProvider.SqlLite: return new Util.Builders.SQLite.DmlBuilder();
		}
		throw new NotImplementedException();
	}

	internal static IDqlBuilder GetDqlBuilder(this DatabaseProvider provider)
	{
		// Code size: 78 (0x4e)
		switch (provider)
		{
			case DatabaseProvider.Oracle: return new Util.Builders.Oracle.DqlBuilder();
			case DatabaseProvider.PostgreSql: return new Util.Builders.PostgreSQL.DqlBuilder();
			case DatabaseProvider.MySql: return new Util.Builders.MySQL.DqlBuilder();
			case DatabaseProvider.SqlServer: return new Util.Builders.SQLServer.DqlBuilder();
			case DatabaseProvider.SqlLite: return new Util.Builders.SQLite.DqlBuilder();
		}
		throw new NotImplementedException();
	}

	internal static bool IsReservedWord(this DatabaseProvider provider, string word)
	{
		// Code size: 150 (0x96)
		//var currentWord = word.ToUpperInvariant();  // removed memory allocation on heap
		if (string.IsNullOrEmpty(word)) return false;
		var upperWord = word.ToUpperInvariant();
		switch (provider)
		{
			case DatabaseProvider.Oracle: return OracleWords.Value.Contains(upperWord);
			case DatabaseProvider.PostgreSql: return PostgreSqlWords.Value.Contains(upperWord);
			case DatabaseProvider.MySql: return MySqlWords.Value.Contains(upperWord);
			case DatabaseProvider.SqlServer: return SqlServerWords.Value.Contains(upperWord);
			case DatabaseProvider.SqlLite: return SqlLiteWords.Value.Contains(upperWord);
		}
		throw new NotImplementedException();
	}

	internal static string GetCatalogSchema(this DatabaseProvider provider)
	{
		switch (provider)
		{
			case DatabaseProvider.PostgreSql:
			case DatabaseProvider.MySql:
			case DatabaseProvider.SqlServer: return "information_schema";
			case DatabaseProvider.Oracle:
			case DatabaseProvider.SqlLite: return string.Empty;
		}
		throw new NotImplementedException();
	}

	internal static string GetCatalogViewName(this DatabaseProvider provider, EntityType entityType)
	{
		switch (provider)
		{
			case DatabaseProvider.PostgreSql:
			case DatabaseProvider.MySql:
			case DatabaseProvider.SqlServer:
				return PostreSqlCatalog[entityType].ViewName;
		}
		throw new NotImplementedException();
	}

	internal static string GetSchemaFieldName(this DatabaseProvider provider, EntityType entityType)
	{
		var result = string.Empty;
		switch (provider)
		{
			case DatabaseProvider.PostgreSql:
			case DatabaseProvider.MySql:
			case DatabaseProvider.SqlServer:
				result = PostreSqlCatalog[entityType].FieldSchemaName;
				break;
		}
		return result;
	}

	internal static string GetEntityFieldName(this DatabaseProvider provider, EntityType entityType)
	{
		var result = string.Empty;
		switch (provider)
		{
			case DatabaseProvider.PostgreSql:
			case DatabaseProvider.MySql:
			case DatabaseProvider.SqlServer:
				result = PostreSqlCatalog[entityType].FieldEntityName;
				break;
		}
		return result;
	}

}
