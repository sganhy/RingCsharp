using Ring.Sql;
using Ring.Schema.Enums;
using Ring.Util.Extensions;
using Ring.Util.Helpers;
using System.Globalization;

namespace Ring.Schema.Extensions;

internal static class DatabaseProviderExtensions
{
    private static string[] _postgreSqlWords = Array.Empty<string>();
    private static string[] _mySqlWords = Array.Empty<string>();
    private static string[] _sqlServerWords = Array.Empty<string>();
    private static string[] _sqlLiteWords = Array.Empty<string>();
    private static bool _postgreSqlInit;
    private static bool _mySqlInit;
    private static bool _sqlServerInit;
    private static bool _sqlLiteInit;
    private static readonly object _syncRoot = new();

    internal static IDdlBuilder GetDdlBuilder(this DatabaseProvider provider)
    {
        switch (provider)
        {
            case DatabaseProvider.MySql: return new Sql.MySQL.DdlBuilder();
            case DatabaseProvider.PostgreSql: return new Sql.PostgreSQL.DdlBuilder();
            case DatabaseProvider.SqlServer: return new Sql.SQLServer.DdlBuilder();
            case DatabaseProvider.SqlLite: return new Sql.SQLite.DdlBuilder();
        }
        throw new NotImplementedException();
    }

    internal static bool IsReservedWord(this DatabaseProvider provider, string word)
    {
        if (provider == DatabaseProvider.PostgreSql)
        {
            if (!_postgreSqlInit) LoadResource(provider, ref _postgreSqlWords, ref _postgreSqlInit);
            return _postgreSqlWords.Exists(word.ToUpper(CultureInfo.InvariantCulture));
        }
        else if (provider == DatabaseProvider.MySql)
        {
            if (!_mySqlInit) LoadResource(provider, ref _mySqlWords, ref _mySqlInit);
            return _mySqlWords.Exists(word.ToUpper(CultureInfo.InvariantCulture));
        }
        else if (provider == DatabaseProvider.SqlServer)
        {
            if (!_sqlServerInit) LoadResource(provider, ref _sqlServerWords , ref _sqlServerInit);
            return _sqlServerWords.Exists(word.ToUpper(CultureInfo.InvariantCulture));
        }
        else if (provider == DatabaseProvider.SqlLite)
        {
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
