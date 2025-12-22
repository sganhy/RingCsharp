namespace Ring.Util.Enums;

internal enum ResourceType : short
{
	/// <summary>
	/// Resource file --> 
	/// </summary>
	LogMessage = 1,
	SqlCommand = 2,
	XmlSchemaTemplate = 3,
	MethodInfo = 4,
	Description = 6,
	OracleReservedKeyWord = 17,
	PostgreSQLReservedKeyWord = 19,
	MySQLReservedKeyWord = 21,
	SQLServerReservedKeyWord = 36,
	SQLiteReservedKeyWord = 49,

	/// <summary>
	/// Bulk alter/ retrieve / save ressources
	/// </summary>
	BulkAlterInvalidFieldName = 70,
	BulkAlterInvalidTableName = 71,

	/// <summary>
	/// Record ressources
	/// </summary>
	RecordWrongRelationType = 99,
	RecordUnkownRelationName = 100,
	RecordUnkownFieldName = 101,
	RecordUnkownRecordType = 102,
	RecordWrongStringFormat = 103,
	RecordValueTooLarge = 104,
	RecordWrongBooleanValue = 105,
	RecordCannotConvert = 106,

	/// <summary>
	/// Miscellaneous
	/// </summary>
	UnRepresentableDateTime = 201,
	NotSupportedInputDateTime = 202,
	InvalidBase64String = 203,
	FieldIsMandatory = 204,
	WrongParameterType = 205,
	UnknownMessageResourceType = 206,

	/// <summary>
	/// Sql operations description
	/// </summary>
	CreateTableNotOk = 245,
	CreateTableOk = 246,

	/// <summary>
	/// IRingConnection messages 
	/// </summary>
	UnsuportedOperation = 294,
	DdlException = 295,
	DdlTableCreated = 296,

	/// <summary>
	/// Reserved for unit tests
	/// </summary>
	UnitTest = 350
}