namespace Ring.Schema.Enums
{
    public enum SchemaSourceType : long
    {
        NativeXml = 1L,
        ClfyXml = 2L,
        NativeDataBase = 4L,
        ClfyDataBase = 5L,
        NotDefined = -999L
    }
}