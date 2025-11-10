using Ring.Schema.Attributes;

namespace Ring.Schema.Enums;

/// <summary>
/// 	stored on 6 bits: value should be included [0,63]
/// </summary>
[Range(0, 63)]
[Flags]
internal enum SearchableType : byte
{
	None = 0,
	IgnoreCase = 1,
	IgnoreDiacritic = 1 << 1,
}