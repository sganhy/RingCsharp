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
	BulkAlterUnsuportedAlterQueryType = 72,

	BulkRetrieveInvalidIndex = 80,
	BulkRetrieveInvalidPageSize = 81,
	BulkRetrieveInvalidPageNumber = 82,
	BulkRetrieveInvalidNumber = 83,
	BulkRetrieveInvalidDate = 84,
	BulkRetrieveInvalidList = 85,
	BulkRetrieveIndexAlreadyExist = 86,
	BulkRetrieveInvalidListType = 87,
	BulkRetrieveInvalidOperation = 88,
	BulkRetrieveCriteriaListEmpty = 89,
	BulkRetrieveParentEntryIndex = 90,
	BulkRetrieveTraverseFromRoot = 91,
	BulkRetrieveInvalidSchemaName = 92,
	BulkRetrieveInvalidObject = 93,

	/// <summary>
	/// Record ressources
	/// </summary>
	RecordWrongRelationType = 119,
	RecordUnkownRelationName = 120,
	RecordUnkownFieldName = 121,
	RecordUnkownRecordType = 122,
	RecordWrongStringFormat = 123,
	RecordValueTooLarge = 124,
	RecordWrongBooleanValue = 125,
	RecordCannotConvert = 126,

	/// <summary>
	/// Miscellaneous
	/// </summary>
	UnRepresentableDateTime = 201,
	NotSupportedInputDateTime = 202,
	InvalidBase64String = 203,
	FieldIsMandatory = 204,
	UnsupportedParamType = 205,
	UnknownMessageResourceType = 206,
	UnexpectedTableType = 207,

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
	/// Providers error message 501-799
	/// </summary>
	UnexpectedProviderMessage = 501,
	ConnectionClosedByServer = 502,
	InvalidMessageLengthFromServer = 503,
	ConnectionAlreadyOpen=504,
	
	/// <summary>
	/// Reserved for unit tests
	/// </summary>
	UnitTest = 2350

}