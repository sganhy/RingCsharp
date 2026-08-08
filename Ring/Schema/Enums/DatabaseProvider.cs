namespace Ring.Schema.Enums;

internal enum DatabaseProvider : byte
{
	Oracle = 1,
	PostgreSql = 2,
	MySql = 3,
	InfluxDb = 5,
	SqlServer = 7,
	SqlLite = 8,
	MariaDb = 9,
	Undefined = 127
}
