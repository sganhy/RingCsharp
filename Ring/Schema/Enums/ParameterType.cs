namespace Ring.Schema.Enums;

internal enum ParameterType
{
	SchemaVersion = 1,
	SchemaCreationTime = 2,
	LastUpdate = 3,
	DefaultLanguage = 4,
	MinPoolSize = 15,
	MaxPoolSize = 16,
	DbConnectionString = 21,
	DbConnectionType = 22,
	Undefined = int.MaxValue
}