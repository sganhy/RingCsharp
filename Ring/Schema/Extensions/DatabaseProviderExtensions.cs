using Ring.Data;
using Ring.Schema.Enums;
using Ring.Util.Builders;
using Ring.Util.Helpers;
using System.Text;

namespace Ring.Schema.Extensions;

internal static class DatabaseProviderExtensions
{
	// reserved key words 
	private static readonly Lazy<HashSet<string>> OracleWords = new(() => new(ResourceHelper.GetReservedWords(DatabaseProvider.Oracle), StringComparer.OrdinalIgnoreCase), true);
	private static readonly Lazy<HashSet<string>> PostgreSqlWords = new(() => new(ResourceHelper.GetReservedWords(DatabaseProvider.PostgreSql), StringComparer.OrdinalIgnoreCase), true);
	private static readonly Lazy<HashSet<string>> MySqlWords = new(() => new(ResourceHelper.GetReservedWords(DatabaseProvider.MySql), StringComparer.OrdinalIgnoreCase), true);
	private static readonly Lazy<HashSet<string>> SqlServerWords = new(() => new(ResourceHelper.GetReservedWords(DatabaseProvider.SqlServer), StringComparer.OrdinalIgnoreCase), true);
	private static readonly Lazy<HashSet<string>> SqlLiteWords = new(() => new(ResourceHelper.GetReservedWords(DatabaseProvider.SqlLite), StringComparer.OrdinalIgnoreCase), true);

	internal static IDdlBuilder GetDdlBuilder(this DatabaseProvider provider, Encoding clientEncoding, bool logSql)
	{
		// Code size: 100 (0x64)
		switch (provider)
		{
			case DatabaseProvider.Oracle: return new Util.Builders.Oracle.DdlBuilder(clientEncoding, logSql);
			case DatabaseProvider.PostgreSql: return new Util.Builders.PostgreSQL.DdlBuilder(clientEncoding, logSql);
			case DatabaseProvider.MySql: return new Util.Builders.MySQL.DdlBuilder(clientEncoding, logSql);
			case DatabaseProvider.SqlServer: return new Util.Builders.SQLServer.DdlBuilder(clientEncoding, logSql);
			case DatabaseProvider.SqlLite: return new Util.Builders.SQLite.DdlBuilder(clientEncoding, logSql);
			case DatabaseProvider.MariaDb: return new Util.Builders.MariaDB.DdlBuilder(clientEncoding, logSql);
		}
		throw new NotImplementedException();
	}

	internal static IDmlBuilder GetDmlBuilder(this DatabaseProvider provider, Encoding clientEncoding, bool logSql)
	{
		// Code size: 100 (0x64)
		switch (provider)
		{
			case DatabaseProvider.Oracle: return new Util.Builders.Oracle.DmlBuilder(clientEncoding, logSql);
			case DatabaseProvider.PostgreSql: return new Util.Builders.PostgreSQL.DmlBuilder(clientEncoding, logSql);
			case DatabaseProvider.MySql: return new Util.Builders.MySQL.DmlBuilder(clientEncoding, logSql);
			case DatabaseProvider.SqlServer: return new Util.Builders.SQLServer.DmlBuilder(clientEncoding, logSql);
			case DatabaseProvider.SqlLite: return new Util.Builders.SQLite.DmlBuilder(clientEncoding, logSql);
			case DatabaseProvider.MariaDb: return new Util.Builders.MariaDB.DmlBuilder(clientEncoding, logSql);
		}
		throw new NotImplementedException();
	}

	internal static IDqlBuilder GetDqlBuilder(this DatabaseProvider provider, Encoding clientEncoding, bool logSql)
	{
		// Code size: 100 (0x64)
		switch (provider)
		{
			case DatabaseProvider.Oracle: return new Util.Builders.Oracle.DqlBuilder(clientEncoding, logSql);
			case DatabaseProvider.PostgreSql: return new Util.Builders.PostgreSQL.DqlBuilder(clientEncoding, logSql);
			case DatabaseProvider.MySql: return new Util.Builders.MySQL.DqlBuilder(clientEncoding, logSql);
			case DatabaseProvider.SqlServer: return new Util.Builders.SQLServer.DqlBuilder(clientEncoding, logSql);
			case DatabaseProvider.SqlLite: return new Util.Builders.SQLite.DqlBuilder(clientEncoding, logSql);
			case DatabaseProvider.MariaDb: return new Util.Builders.MariaDB.DqlBuilder(clientEncoding, logSql);
		}
		throw new NotImplementedException();
	}	

	internal static ITclBuilder GetTclBuilder(this DatabaseProvider provider, Encoding clientEncoding, bool logSql)
	{
		// Code size: 100 (0x64)
		switch (provider)
		{
			case DatabaseProvider.Oracle: return new Util.Builders.Oracle.TclBuilder(clientEncoding, logSql);
			case DatabaseProvider.PostgreSql: return new Util.Builders.PostgreSQL.TclBuilder(clientEncoding, logSql);
			case DatabaseProvider.MySql: return new Util.Builders.MySQL.TclBuilder(clientEncoding, logSql);
			case DatabaseProvider.SqlServer: return new Util.Builders.SQLServer.TclBuilder(clientEncoding, logSql);
			case DatabaseProvider.SqlLite: return new Util.Builders.SQLite.TclBuilder(clientEncoding, logSql);
			case DatabaseProvider.MariaDb: return new Util.Builders.MariaDB.TclBuilder(clientEncoding, logSql);
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
			case DatabaseProvider.MariaDb:
			case DatabaseProvider.MySql: return MySqlWords.Value.Contains(word);
			case DatabaseProvider.SqlServer: return SqlServerWords.Value.Contains(word);
			case DatabaseProvider.SqlLite: return SqlLiteWords.Value.Contains(word);
		}
		throw new NotImplementedException();
	}

	internal static IsolationLevel GetDefaultIsolationLevel(this DatabaseProvider provider)
	{
		switch (provider)
		{
			case DatabaseProvider.Oracle:
			case DatabaseProvider.PostgreSql: return IsolationLevel.ReadCommitted;
			case DatabaseProvider.MariaDb:
			case DatabaseProvider.MySql: return IsolationLevel.RepeatableRead;
			case DatabaseProvider.SqlServer: return IsolationLevel.ReadUncommitted;
		}
		throw new NotImplementedException();
	}

}
