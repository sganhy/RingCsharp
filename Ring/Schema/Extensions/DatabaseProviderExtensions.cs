﻿using Ring.Schema.Enums;
using Ring.Util.Extensions;
using Ring.Util.Helpers;
using System.Globalization;
using Ring.Util.Builders;

namespace Ring.Schema.Extensions;

internal static class DatabaseProviderExtensions
{
    // reserved key words 
    private readonly static string[] _oracleWords = ResourceHelper.GetReservedWords(DatabaseProvider.Oracle);
    private readonly static string[] _postgreSqlWords = ResourceHelper.GetReservedWords(DatabaseProvider.PostgreSql);
    private readonly static string[] _mySqlWords = ResourceHelper.GetReservedWords(DatabaseProvider.MySql);
    private readonly static string[] _sqlServerWords = ResourceHelper.GetReservedWords(DatabaseProvider.SqlServer);
    private readonly static string[] _sqlLiteWords = ResourceHelper.GetReservedWords(DatabaseProvider.SqlLite);

#pragma warning disable IDE0066 // Convert switch statement to expression

    internal static IDdlBuilder GetDdlBuilder(this DatabaseProvider provider)
    {
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
        switch (provider)
        {
            case DatabaseProvider.Oracle: return _oracleWords.Exists(word.ToUpper(CultureInfo.InvariantCulture));
            case DatabaseProvider.PostgreSql: return _postgreSqlWords.Exists(word.ToUpper(CultureInfo.InvariantCulture));
            case DatabaseProvider.MySql: return _mySqlWords.Exists(word.ToUpper(CultureInfo.InvariantCulture));
            case DatabaseProvider.SqlServer: return _sqlServerWords.Exists(word.ToUpper(CultureInfo.InvariantCulture));
            case DatabaseProvider.SqlLite: return _sqlLiteWords.Exists(word.ToUpper(CultureInfo.InvariantCulture));
        }
        throw new NotImplementedException();
    }

#pragma warning restore IDE0066

}
