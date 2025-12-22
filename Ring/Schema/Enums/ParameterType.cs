namespace Ring.Schema.Enums;

// Id define position into Ring.Util.Resources.Parameter.gz
internal enum ParameterType
{
	SchemaVersion = 1,
	SchemaCreationTime = 2,
	LastUpgrade = 3,
	DefaultLanguage = 4,
	Ring0Version = 5,
	MinPoolSize = 15,
	MaxPoolSize = 16,
	DbConnectionString = 21,
	DbConnectionType = 22,
	Undefined = int.MaxValue
}