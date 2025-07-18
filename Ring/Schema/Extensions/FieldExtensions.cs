using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Helpers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.Schema.Extensions;

internal static class FieldExtensions
{
	private const char HashCodeSeparator = (char)3333;

#pragma warning disable RCS1187 // Use constant instead of field
	private static readonly string PrimaryKeyFieldName = "id";
	private static readonly string PrimaryKeyDescription = "Internal record number";
	private static readonly string NumberDefaultValue = "0";
	private static readonly Field _defaultPrimaryKeyInt64 =
		new(0, PrimaryKeyFieldName, PrimaryKeyDescription, FieldType.Long, 0, NumberDefaultValue, SearchableType.None, true, true, false, true);
	private static readonly Field _defaultPrimaryKeyInt32 =
		new(0, PrimaryKeyFieldName, PrimaryKeyDescription, FieldType.Int, 0, NumberDefaultValue, SearchableType.None, true, true, false, true);
	private static readonly Field _defaultPrimaryKeyInt16 =
		new(0, PrimaryKeyFieldName, PrimaryKeyDescription, FieldType.Short, 0, NumberDefaultValue, SearchableType.None, true, true, false, true);
	private static readonly Field _defaultPrimaryKeyInt08 =
		new(0, PrimaryKeyFieldName, PrimaryKeyDescription, FieldType.Byte, 0, NumberDefaultValue, SearchableType.None, true, true, false, true);
#pragma warning restore RCS1187

	internal static bool IsValid(this Field field) => IsPrimaryKey(field) || field.Id > 0; 
	internal static bool IsDateTime(this Field field) => field.Type == FieldType.DateTime ||
		field.Type == FieldType.ShortDateTime || field.Type == FieldType.LongDateTime;
	internal static bool IsNumeric(this Field field) => field.Type == FieldType.Long || field.Type == FieldType.Int ||
		field.Type == FieldType.Short || field.Type == FieldType.Byte || field.Type == FieldType.Float ||
		field.Type == FieldType.Double;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsPrimaryKey(this Field field) =>
		ReferenceEquals(field, _defaultPrimaryKeyInt64) || ReferenceEquals(field, _defaultPrimaryKeyInt32) ||
		ReferenceEquals(field, _defaultPrimaryKeyInt16) || ReferenceEquals(field, _defaultPrimaryKeyInt08);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsSearchable(this Field field) // Code size: 22 (0x16)
		=> field.Type == FieldType.String && field.SearchableType != SearchableType.None;

	/// <summary>
	/// Calculate searchable field value (remove diacritic characters and value.ToUpper())
	/// </summary>
	internal static string? GetSearchableValue(this Field? _, SearchableType searchableType, string? value)
	{
		// Code size: 117 (0x75)
		if (value == null) return null;
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
		switch (fieldType)
		{
			case FieldType.Byte: return _defaultPrimaryKeyInt08;
			case FieldType.Short: return _defaultPrimaryKeyInt16;
			case FieldType.Int: return _defaultPrimaryKeyInt32;
			case FieldType.Long: return _defaultPrimaryKeyInt64;
		}
		return null;
	}

	internal static Field SetType(this Field field, FieldType fieldType) // Code size: 67 (0x43)
		=> new(field.Id, field.Name, field.Description, fieldType, field.Size, field.DefaultValue, field.SearchableType, field.Baseline, field.NotNull, field.Multilingual, field.Active);
	internal static Field SetSize(this Field field, int size) // Code size: 67 (0x43)
		=> new(field.Id, field.Name, field.Description, field.Type, size, field.DefaultValue, field.SearchableType, field.Baseline, field.NotNull, field.Multilingual, field.Active);

	internal static long GetHashCode(this Field field)
	{
		HashHelper.Djb2X(GetStringCode(field), out long hash);
		return hash;
	}

	// Code size: 148 (0x94) - checked 2025-07-18
	internal static string GetStringCode(this Field field)
		=> new StringBuilder()
			.Append((int)field.SearchableType)
			.Append(HashCodeSeparator)
			.Append(field.DefaultValue)
			.Append(HashCodeSeparator)
			.Append(field.Multilingual)
			.Append(HashCodeSeparator)
			.Append(field.NotNull)
			.Append(HashCodeSeparator)
			.Append(field.Size)
			.Append(HashCodeSeparator)
			.Append((int)field.Type)
			.Append(HashCodeSeparator)
			.Append(BaseEntityExtensions.GetStringCode(field)) // + BaseEntity string code
			.ToString();

}