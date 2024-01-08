using Ring.Schema.Enums;
using Ring.Util.Extensions;
using Ring.Util.Helpers;
using System.Globalization;
using Ring.Util.Builders;

namespace Ring.Schema.Extensions;

internal static class DatabaseProviderExtensions
{
    private static string[] _oracleWords = Array.Empty<string>();
    private static string[] _postgreSqlWords = Array.Empty<string>();
    private static string[] _mySqlWords = Array.Empty<string>();
    private static string[] _sqlServerWords = Array.Empty<string>();
    private static string[] _sqlLiteWords = Array.Empty<string>();
    private static bool _oracleInit;
    private static bool _postgreSqlInit;
    private static bool _mySqlInit;
    private static bool _sqlServerInit;
    private static bool _sqlLiteInit;
    private static readonly object _syncRoot = new();

    internal static IDdlBuilder GetDdlBuilder(this DatabaseProvider provider)
    {
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (provider)
        {
            case DatabaseProvider.Oracle: return new Util.Builders.Oracle.DdlBuilder();
            case DatabaseProvider.PostgreSql: return new Util.Builders.PostgreSQL.DdlBuilder();
            case DatabaseProvider.MySql: return new Util.Builders.MySQL.DdlBuilder();
            case DatabaseProvider.SqlServer: return new Util.Builders.SQLServer.DdlBuilder();
            case DatabaseProvider.SqlLite: return new Util.Builders.SQLite.DdlBuilder();
        }
#pragma warning restore IDE0066
        throw new NotImplementedException();
    }

    internal static IDmlBuilder GetDmlBuilder(this DatabaseProvider provider)
    {
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (provider)
        {
            case DatabaseProvider.Oracle: return new Util.Builders.Oracle.DmlBuilder();
            case DatabaseProvider.PostgreSql: return new Util.Builders.PostgreSQL.DmlBuilder();
            case DatabaseProvider.MySql: return new Util.Builders.MySQL.DmlBuilder();
            case DatabaseProvider.SqlServer: return new Util.Builders.SQLServer.DmlBuilder();
            case DatabaseProvider.SqlLite: return new Util.Builders.SQLite.DmlBuilder();
        }
#pragma warning restore IDE0066
        throw new NotImplementedException();
    }

    internal static IDqlBuilder GetDqlBuilder(this DatabaseProvider provider)
    {
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (provider)
        {
            case DatabaseProvider.Oracle: return new Util.Builders.Oracle.DqlBuilder();
            case DatabaseProvider.PostgreSql: return new Util.Builders.PostgreSQL.DqlBuilder();
            case DatabaseProvider.MySql: return new Util.Builders.MySQL.DqlBuilder();
            case DatabaseProvider.SqlServer: return new Util.Builders.SQLServer.DqlBuilder();
            case DatabaseProvider.SqlLite: return new Util.Builders.SQLite.DqlBuilder();
        }
#pragma warning restore IDE0066
        throw new NotImplementedException();
    }

    internal static bool IsReservedWord(this DatabaseProvider provider, string word)
    {
        switch (provider)
        {
            case DatabaseProvider.Oracle:
                if (!_oracleInit) LoadResource(provider, ref _oracleWords, ref _oracleInit);
                return _postgreSqlWords.Exists(word.ToUpper(CultureInfo.InvariantCulture));
            case DatabaseProvider.PostgreSql:
                if (!_postgreSqlInit) LoadResource(provider, ref _postgreSqlWords, ref _postgreSqlInit);
                return _postgreSqlWords.Exists(word.ToUpper(CultureInfo.InvariantCulture));
            case DatabaseProvider.MySql:
                if (!_mySqlInit) LoadResource(provider, ref _mySqlWords, ref _mySqlInit);
                return _mySqlWords.Exists(word.ToUpper(CultureInfo.InvariantCulture));
            case DatabaseProvider.SqlServer:
                if (!_sqlServerInit) LoadResource(provider, ref _sqlServerWords, ref _sqlServerInit);
                return _sqlServerWords.Exists(word.ToUpper(CultureInfo.InvariantCulture));
            case DatabaseProvider.SqlLite:
                if (!_sqlLiteInit) LoadResource(provider, ref _sqlLiteWords, ref _sqlLiteInit);
                return _sqlLiteWords.Exists(word.ToUpper(CultureInfo.InvariantCulture));
        }
        return true;
    }

    #region private methods

    private static void LoadResource(DatabaseProvider databaseProvider, ref string[] bucket, ref bool initialized)
    {
        lock (_syncRoot)
        {
            if (!initialized)
            {
                var resourceHelper = new ResourceHelper();
                bucket = resourceHelper.GetReservedWords(databaseProvider);
                initialized = true;
            }
        }
    }

    #endregion 

}
