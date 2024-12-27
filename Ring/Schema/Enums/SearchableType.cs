namespace Ring.Schema.Enums;

// stored on 6 bits : value shoulbe included [0,63]
internal enum SearchableType : byte
{
	None = 0, 
	IngoreCase = 1,
	IngoreCaseAndDiacritics = 2,
}
