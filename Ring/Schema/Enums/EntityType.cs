namespace Ring.Schema.Enums
{
    internal enum EntityType : sbyte
    {
        Null = -1, // reserve for null entity
        Table = 0,
        Field = 1,
        Relation = 2,
        Index = 3,
        Schema = 7,
        Service = 8,
        Role = 9,
        Lexicon = 11,
        Language = 12,
        Sequence = 15,
        Assembly = 16,
        Event = 17,
        TableSpace = 18,
        NotDefined = 99,
        Comment = 100
    }
}