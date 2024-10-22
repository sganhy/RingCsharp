namespace Ring.Util.Enums;

internal enum ResourceType : short
{
	LogMessage = 1,
	SqlCommand = 2,
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
    UnRepresentableDateTime = 107,
	NotSupportedInputDateTime = 108,
	InvalidBase64String = 109,
	FieldIsMandatory = 110,

    /// <summary>
    /// IRingConnection messages 
    /// </summary>
    UnsuportedOperation = 200,
    DdlException = 201,
    DdlTableCreated = 202
}
