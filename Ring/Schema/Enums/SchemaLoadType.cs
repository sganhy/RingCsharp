namespace Ring.Schema.Enums
{
    public enum SchemaLoadType : long
    {
        Full = 1L,
        QueryOnly = 2L // only field, relation & unique index !;;;
    }
}