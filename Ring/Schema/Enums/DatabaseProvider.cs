namespace Ring.Schema.Enums;

internal enum DatabaseProvider : byte
{
    Oracle = 1,
    PostgreSql = 2,
    MySql = 3,
    InfluxDb = 4,
    SqlServer = 5,
    SqlLite = 7,
    Undefined = 127
}
