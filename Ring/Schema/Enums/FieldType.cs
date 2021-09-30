namespace Ring.Schema.Enums
{
    internal enum FieldType : byte
    {
        Long = 0,
        Int = 1,
        Short = 2,
        Byte = 3,
        Float = 4,
        Double = 5,
        String = 6,
        ShortDateTime = 7,
        DateTime = 8,
        LongDateTime = 9,
        Array = 11,
        Boolean = 13,
        NotDefined = 127
    }
}