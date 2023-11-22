namespace Ring.Schema.Enums;

internal enum TableType : byte
{
    Business              = 1,
    Meta                  = 3,
    MetaId                = 4,
    Fake                  = 5,
    Mtm                   = 6,
    Log                   = 7,
    Lexicon               = 8,
    LexiconItem           = 9,
    SchemaDictionary      = 22,
    TableDictionary       = 24,
    TableSpaceDictionary  = 25,
    Logical               = 27
}