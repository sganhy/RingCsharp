using Ring.Schema.Enums;
using Ring.Util.Builders;
using Ring.Util.Helpers;

namespace Ring.Schema.Extensions;

internal static class DatabaseProviderExtensions
{
	// reserved key words 
	private static readonly Lazy<HashSet<string>> OracleWords = new(() => new(ResourceHelper.GetReservedWords(DatabaseProvider.Oracle), StringComparer.OrdinalIgnoreCase), true);
	private static readonly Lazy<HashSet<string>> PostgreSqlWords = new(() => new(ResourceHelper.GetReservedWords(DatabaseProvider.PostgreSql), StringComparer.OrdinalIgnoreCase), true);
	private static readonly Lazy<HashSet<string>> MySqlWords = new(() => new(ResourceHelper.GetReservedWords(DatabaseProvider.MySql), StringComparer.OrdinalIgnoreCase), true);
	private static readonly Lazy<HashSet<string>> SqlServerWords = new(() => new(ResourceHelper.GetReservedWords(DatabaseProvider.SqlServer), StringComparer.OrdinalIgnoreCase), true);
	private static readonly Lazy<HashSet<string>> SqlLiteWords = new(() => new(ResourceHelper.GetReservedWords(DatabaseProvider.SqlLite), StringComparer.OrdinalIgnoreCase), true);

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
		// Code size: 133 (0x85)
		switch (provider)
		{
			case DatabaseProvider.Oracle: return OracleWords.Value.Contains(word);
			case DatabaseProvider.PostgreSql: return PostgreSqlWords.Value.Contains(word);
			case DatabaseProvider.MySql: return MySqlWords.Value.Contains(word);
			case DatabaseProvider.SqlServer: return SqlServerWords.Value.Contains(word);
			case DatabaseProvider.SqlLite: return SqlLiteWords.Value.Contains(word);
		}
		throw new NotImplementedException();
	}

}
