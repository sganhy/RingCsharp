namespace Ring.Data
{
    // Warning ==> cache for bind variables expecting DatabaseProvider is defined as byte ( see Ring.Data.Helpers.SqlHelper)
    public enum DatabaseProvider : byte
    {
        Sqlite = 1,
        Oracle = 2,
        PostgreSql = 3,
        MySql = 4
    }
}
