using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.Schema.Extensions;

internal static class FieldExtensions
{

#pragma warning disable RCS1187 // Use constant instead of field
	private static readonly string PrimaryKeyFieldName = "id";
	private static readonly string PrimaryKeyDescription = "Internal record number";
	private static readonly string NumberDefaultValue = "0";
	private static readonly Field DefaultPrimaryKeyInt64 =	new(0, PrimaryKeyFieldName, PrimaryKeyDescription, FieldType.Long, 0, NumberDefaultValue, SearchableType.None, true, true, false, false, true);
	private static readonly Field DefaultPrimaryKeyInt32 =	new(0, PrimaryKeyFieldName, PrimaryKeyDescription, FieldType.Int, 0, NumberDefaultValue, SearchableType.None, true, true, false, false, true);
	private static readonly Field DefaultPrimaryKeyInt16 =	new(0, PrimaryKeyFieldName, PrimaryKeyDescription, FieldType.Short, 0, NumberDefaultValue, SearchableType.None, true, true, false, false, true);
	private static readonly Field DefaultPrimaryKeyInt08 =	new(0, PrimaryKeyFieldName, PrimaryKeyDescription, FieldType.Byte, 0, NumberDefaultValue, SearchableType.None, true, true, false, false, true);
#pragma warning restore RCS1187

	internal static bool IsValid(this Field field) => IsPrimaryKey(field) || field.Id > 0; 
	internal static bool IsDateTime(this Field field) => field.Type == FieldType.DateTime ||
		field.Type == FieldType.ShortDateTime || field.Type == FieldType.LongDateTime;
	internal static bool IsNumeric(this Field field) => field.Type == FieldType.Long || field.Type == FieldType.Int ||
		field.Type == FieldType.Short || field.Type == FieldType.Byte || field.Type == FieldType.Float ||
		field.Type == FieldType.Double;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsPrimaryKey(this Field field) =>
		ReferenceEquals(field, DefaultPrimaryKeyInt64) || ReferenceEquals(field, DefaultPrimaryKeyInt32) ||
		ReferenceEquals(field, DefaultPrimaryKeyInt16) || ReferenceEquals(field, DefaultPrimaryKeyInt08);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsSearchable(this Field field) // Code size: 22 (0x16)
		=> field.Type == FieldType.String && field.SearchableType != SearchableType.None;

	/// <summary>
	/// Calculate searchable field value (remove diacritic characters and value.ToUpper())
	/// </summary>
	internal static string? GetSearchableValue(this Field? _, SearchableType searchableType, string? value)
	{
		// Code size: 117 (0x75)
		if (value is null) return null;
		switch (searchableType)
		{
			case SearchableType.IgnoreCase:	return value.ToUpperInvariant();
			case SearchableType.IgnoreCaseAndDiacritics:
				var normalizedString = value.Normalize(NormalizationForm.FormD).AsSpan();
				var result = new StringBuilder(normalizedString.Length);
				foreach (var c in normalizedString)
				{
					// CharUnicodeInfo.GetUnicodeCategory(c) <> UnicodeCategory.NonSpacingMark
					if (char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
						result.Append(char.ToUpper(c, CultureInfo.InvariantCulture));
				}
				return result.ToString();
		}
		return value;
	}

	internal static Meta ToMeta(this Field field, int tableId, FieldType? newFieldType=null)
	{
		// Code size: 148 (0x94)
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags,field.Baseline);
		flags = Meta.SetFieldNotNull(flags, field.NotNull);
		flags = Meta.SetFieldMultilingual(flags, field.Multilingual);
		if (field.Type == FieldType.String) flags = Meta.SetSearchableType(flags, field.SearchableType); // check data flags quality 
		flags = Meta.SetFieldSize(flags, field.Size);
		var dataType = 0 ;
		dataType = Meta.SetFieldType(dataType, newFieldType ?? field.Type);
		return new (field.Id, (byte)EntityType.Field, tableId, dataType, flags, field.Name, field.Description, null, field.Active);
	}

	internal static Field? GetDefaultPrimaryKey(this Field? _, FieldType fieldType)
	{
		// Code size: 67 (0x43)
		switch (fieldType)
		{
			case FieldType.Byte: return DefaultPrimaryKeyInt08;
			case FieldType.Short: return DefaultPrimaryKeyInt16;
			case FieldType.Int: return DefaultPrimaryKeyInt32;
			case FieldType.Long: return DefaultPrimaryKeyInt64;
		}
		return null;
	}

	internal static Field SetType(this Field field, FieldType fieldType) // Code size: 67 (0x43)
		=> new(field.Id, field.Name, field.Description, fieldType, field.Size, field.DefaultValue, field.SearchableType, field.Baseline, field.NotNull, field.Multilingual, field.AllowTruncation, field.Active);

	internal static Field SetNotNull(this Field field, bool notNull) // Code size: 67 (0x43)
		=> new(field.Id, field.Name, field.Description, field.Type, field.Size, field.DefaultValue, field.SearchableType, field.Baseline, notNull, field.Multilingual, field.AllowTruncation, field.Active);

	internal static Field SetSize(this Field field, int size) // Code size: 67 (0x43)
		=> new(field.Id, field.Name, field.Description, field.Type, size, field.DefaultValue, field.SearchableType, field.Baseline, field.NotNull, field.Multilingual, field.AllowTruncation, field.Active);

	internal static int Hash(this Field field) 
	{
        // // Code size: 24 (0x18)
        var hash = new HashCode();
		hash.AddField(field);
		return hash.ToHashCode();
	}

	/// <summary>
	/// Determines if two Field instances have equivalent definitions,
	/// regardless of whether they're the same object reference.
	/// </summary>
	internal static bool IsEquivalentTo(this Field field, Field? other)
	{
        // Code size: 116 (0x74)
        if (!field.BaseEntityEquals(other)) return false;
		// other cannot be null here 
		return field.Type == other!.Type && field.Size == other.Size && field.NotNull == other.NotNull && field.Multilingual == other.Multilingual && field.AllowTruncation == other.AllowTruncation
            && field.SearchableType == other.SearchableType && string.Equals(field.DefaultValue, other.DefaultValue, StringComparison.Ordinal);
	}
}
