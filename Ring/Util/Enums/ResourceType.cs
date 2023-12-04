namespace Ring.Util.Enums;

internal enum ResourceType : short
{
    LogMessage = 1,
    SqlCommand = 2,
    PostgreSQLReservedKeyWord = 19,
    MySQLReservedKeyWord = 21,
    SQLServerReservedKeyWord = 36,
    SQLiteReservedKeyWord = 49,
    /// <summary>
    /// Record ressources
    /// </summary>
    RecordUnkownFieldName = 101,
    RecordUnkownRecordType = 102,
    RecordWrongStringFormat = 103,
    RecordValueTooLarge = 104,
    RecordWrongBooleanValue = 105,
    RecordCannotConvert = 106
}
