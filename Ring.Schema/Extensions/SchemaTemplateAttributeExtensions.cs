using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using System.Runtime.CompilerServices;

namespace Ring.Schema.Extensions;

internal static class SchemaTemplateAttributeExtensions
{
	internal static SchemaTemplateAttribute SetValues(this SchemaTemplateAttribute schemaTemplateAttribute, SchemaTemplateAttributeValue[] values) // Code size: 19 (0x13)
		=> new(schemaTemplateAttribute.Name, schemaTemplateAttribute.Type, values);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static FieldType GetFieldType(this SchemaTemplateAttribute schemaTemplateAttribute, string value)
	{
		// Code size: 31 (0x1f)
		var attributeValue = GetAttributeValue(schemaTemplateAttribute, value.ToUpperInvariant());
		return attributeValue is null ? FieldType.Undefined : attributeValue.Id.ToFieldType();
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
	#endregion 


}
