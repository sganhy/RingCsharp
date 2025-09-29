using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Helpers;

namespace Ring.Schema.Extensions;

internal static class DatabaseProviderExtensions
{
	// reserved key words 
	private readonly static Lazy<HashSet<string>> _oracleWords = new(() => ResourceHelper.GetReservedWords(DatabaseProvider.Oracle), true); 
	private readonly static Lazy<HashSet<string>> _postgreSqlWords = new(() => ResourceHelper.GetReservedWords(DatabaseProvider.PostgreSql), true);
	private readonly static Lazy<HashSet<string>> _mySqlWords = new(() => ResourceHelper.GetReservedWords(DatabaseProvider.MySql), true);
	private readonly static Lazy<HashSet<string>> _sqlServerWords = new(() => ResourceHelper.GetReservedWords(DatabaseProvider.SqlServer), true);
	private readonly static Lazy<HashSet<string>> _sqlLiteWords = new(() => ResourceHelper.GetReservedWords(DatabaseProvider.SqlLite), true);

	// catalogs
	private static readonly Dictionary<EntityType, Catalog> _postreSqlCatalog = new() {
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
        // Code size: 140 (0x8c)
        var currentWord = word.ToUpperInvariant();
		switch (provider)
		{
			case DatabaseProvider.Oracle: return _oracleWords.Value.Contains(currentWord);
			case DatabaseProvider.PostgreSql: return _postgreSqlWords.Value.Contains(currentWord);
			case DatabaseProvider.MySql: return _mySqlWords.Value.Contains(currentWord);
			case DatabaseProvider.SqlServer: return _sqlServerWords.Value.Contains(currentWord);
			case DatabaseProvider.SqlLite: return _sqlLiteWords.Value.Contains(currentWord);
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
				return _postreSqlCatalog[entityType].ViewName;
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
				result = _postreSqlCatalog[entityType].FieldSchemaName;
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
				result = _postreSqlCatalog[entityType].FieldEntityName;
				break;
		}
		return result;
	}

}
