using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ring.Schema.Extensions;

internal static class SchemaTemplateAttributeExtensions
{
    private static readonly string TrueString = true.ToString(CultureInfo.InvariantCulture);
	private static readonly string FalseString = false.ToString(CultureInfo.InvariantCulture);
	private static readonly string YesString = "yes";
	private static readonly string Number1String = 1.ToString(CultureInfo.InvariantCulture);
	private static readonly string Number0String = 0.ToString(CultureInfo.InvariantCulture);
	private static readonly string NoString = "no";

	internal static SchemaTemplateAttribute SetValues(this SchemaTemplateAttribute schemaTemplateAttribute, SchemaTemplateAttributeValue[] values) // Code size: 19 (0x13)
		=> new(schemaTemplateAttribute.Name, schemaTemplateAttribute.Type, values);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static FieldType GetFieldType(this SchemaTemplateAttribute schemaTemplateAttribute, string value)
	{
		// Code size: 36 (0x24)
		var attributeValue = GetAttributeValue(schemaTemplateAttribute, value.ToUpperInvariant().Trim());
		return attributeValue is null ? FieldType.Undefined : attributeValue.Id.ToFieldType();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static RelationType GetRelationType(this SchemaTemplateAttribute schemaTemplateAttribute, string value)
	{
		// Code size: 36 (0x24)
		var attributeValue = GetAttributeValue(schemaTemplateAttribute, value.ToUpperInvariant().Trim());
		return attributeValue is null ? RelationType.Undefined : attributeValue.Id.ToRelationType();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static SearchableType GetSearchableType(this SchemaTemplateAttribute schemaTemplateAttribute, string value)
	{
		// Code size: 23 (0x17)
		switch (schemaTemplateAttribute.Type)
		{
			case SchemaTemplateAttributeType.CaseSensitive: return GetFlagValue(value) == false ? SearchableType.IgnoreCase : SearchableType.None;
		}
		return SearchableType.None;
	}

	#region private methods 

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static SchemaTemplateAttributeValue? GetAttributeValue(SchemaTemplateAttribute schemaTemplateAttribute, string value)
	{
		// Code size: 92 (0x5c)
		var span = new ReadOnlySpan<SchemaTemplateAttributeValue>(schemaTemplateAttribute.AttributeValues);
		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = indexerLeft + indexerRight;
			indexerMiddle >>= 1; // indexerMiddle <-- indexerMiddle /2 
			var indexerCompare = string.CompareOrdinal(value, span[indexerMiddle].Value);
			if (indexerCompare == 0) return span[indexerMiddle];
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return null;
	}

	private static bool? GetFlagValue(string value)
	{
		// Code size: 115 (0x73)
		var comparedvalue = value.Trim();
		if (string.Equals(TrueString, comparedvalue, StringComparison.OrdinalIgnoreCase) || 
			string.Equals(Number1String, comparedvalue, StringComparison.OrdinalIgnoreCase) ||
			string.Equals(YesString, comparedvalue, StringComparison.OrdinalIgnoreCase))
			return true;
		if (string.Equals(FalseString, comparedvalue, StringComparison.OrdinalIgnoreCase) || 
			string.Equals(Number0String, comparedvalue, StringComparison.OrdinalIgnoreCase) ||
			string.Equals(NoString, comparedvalue, StringComparison.OrdinalIgnoreCase)) 
			return false;
		return null;
	}

	#endregion


}
