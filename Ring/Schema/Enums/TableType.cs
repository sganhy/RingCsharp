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
	SchemaCatalog         = 22,
	TableCatalog          = 24,
	TableSpaceCatalog     = 25,
	Logical               = 27
}