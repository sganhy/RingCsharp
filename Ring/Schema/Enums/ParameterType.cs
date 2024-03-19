using Ring.Schema.Attributes;

namespace Ring.Schema.Enums;

internal enum ParameterType
{
	[ParameterType("@version", "Schema version", FieldType.String, EntityType.Schema)]
	SchemaVersion = 1,

	[ParameterType("@creation_time", "Schema creation time", FieldType.DateTime, EntityType.Schema)]
	SchemaCreationTime = 2,

	[ParameterType("@last_upgrade", "Last Schema upgrade", FieldType.DateTime, EntityType.Schema)]
	LastUpdate = 3,

	[ParameterType("@language", "Default language", FieldType.String, EntityType.Schema)]
	DefaultLanguage = 4,

	[ParameterType("@MinConnPoolSize", "Mininimum database connection pool", FieldType.Int, EntityType.Schema, "1")]
	MinPoolSize = 15,

	[ParameterType("@MaxConnPoolSize", "Maximum database connection pool", FieldType.Int, EntityType.Schema, "1")]
	MaxPoolSize = 16,

	[ParameterType("@DbConnectionString", "DataBase connectioon string", FieldType.String, EntityType.Schema)]
	DbConnectionString = 21,

	[ParameterType("@DbConnectionType", "DataBase connectioon type", FieldType.Int, EntityType.Schema)]
	DbConnectionType = 22,

	[ParameterType(FieldType.Undefined, EntityType.Undefined)]
	Undefined = int.MaxValue
}