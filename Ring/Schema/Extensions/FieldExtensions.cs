﻿using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.Schema.Extensions;

internal static class FieldExtensions
{
	private static readonly string PrimaryKeyFieldName = "id";
	private static readonly string PrimaryKeyDescription = "Internal record number";
	private static readonly string NumberDefaultValue = "0";
	private static readonly Field _defaultPrimaryKeyInt64 =
		new(0, PrimaryKeyFieldName, PrimaryKeyDescription, FieldType.Long, 0, NumberDefaultValue, false, true, true, true, false);
	private static readonly Field _defaultPrimaryKeyInt32 =
		new(0, PrimaryKeyFieldName, PrimaryKeyDescription, FieldType.Int, 0, NumberDefaultValue, false, true, true, true, false);
	private static readonly Field _defaultPrimaryKeyInt16 =
		new(0, PrimaryKeyFieldName, PrimaryKeyDescription, FieldType.Short, 0, NumberDefaultValue, false, true, true, true, false);
	private static readonly Field _defaultPrimaryKeyInt08 =
		new(0, PrimaryKeyFieldName, PrimaryKeyDescription, FieldType.Byte, 0, NumberDefaultValue, false, true, true, true, false);

	internal static bool IsValid(this Field field) => IsPrimaryKey(field) || field.Id > 0; 
	internal static bool IsDateTime(this Field field) => field.Type == FieldType.DateTime ||
														field.Type == FieldType.ShortDateTime ||
														field.Type == FieldType.LongDateTime;
	internal static bool IsNumeric(this Field field) => field.Type == FieldType.Long || field.Type == FieldType.Int ||
		field.Type == FieldType.Short || field.Type == FieldType.Byte || field.Type == FieldType.Float ||
		field.Type == FieldType.Double;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsPrimaryKey(this Field field) =>
		ReferenceEquals(field, _defaultPrimaryKeyInt64) || ReferenceEquals(field, _defaultPrimaryKeyInt32) ||
		ReferenceEquals(field, _defaultPrimaryKeyInt16) || ReferenceEquals(field, _defaultPrimaryKeyInt08);

    /// <summary>
    /// Calculate searchable field value (remove diacritic characters and value.ToUpper())
    /// </summary>
	internal static string? GetSearchableValue(this Field _, string value)
	{
		if (value == null) return null;
		var result = new StringBuilder();
		var normalizedString = value.Normalize(NormalizationForm.FormD);
		var count = normalizedString.Length;

		for (var i = 0; i < count; ++i)
		{
			// CharUnicodeInfo.GetUnicodeCategory(c) <> UnicodeCategory.NonSpacingMark
			var c = normalizedString[i];
			if (char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
				result.Append(char.ToUpper(c, CultureInfo.InvariantCulture));
		}
		return result.ToString();
	}

	internal static Meta ToMeta(this Field field, int tableId, FieldType? newFieldType=null) 
	{
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags,field.Baseline);
        flags = Meta.SetFieldNotNull(flags, field.NotNull);
        flags = Meta.SetFieldMultilingual(flags, field.Multilingual);
        flags = Meta.SetFieldSize(flags, field.Size);
        var dataType = 0 ;
        dataType = Meta.SetFieldType(dataType, newFieldType ?? field.Type);
        string? value = null;
        var meta = new Meta(field.Id, (byte)EntityType.Field, tableId, dataType, flags, field.Name, field.Description, value, field.Active);
		return meta;
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

}

