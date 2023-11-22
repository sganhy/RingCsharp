namespace Ring.Schema.Enums;

internal enum FieldType : byte
{
    Long           = 0,
    Int            = 1,
    Short          = 2,
    Byte           = 3,
    Float          = 14,
    Double         = 15,
    String         = 16,
    ShortDateTime  = 17,
    DateTime       = 18,
    LongDateTime   = 19,
    ByteArray      = 21,
    Boolean        = 23,
    LongString     = 27,
    // 125 & 126 reserved for unit tests
    Undefined = 127
}