using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Runtime.CompilerServices;

namespace Ring.Schema.Extensions;

internal static class SchemaTemplateItemExtensions
{

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static SchemaTemplateAttribute? GetAttribute(this SchemaTemplateItem schemaTemplateItem, SchemaTemplateAttributeType attributeType)
	{
		// Code size: 98 (0x62)
		var attributeTypeId = (int)attributeType;
		var span = new ReadOnlySpan<SchemaTemplateAttribute>(schemaTemplateItem.Attributes);
		int indexerLeft = 0, indexerRight = span.Length - 1;
		while (indexerLeft <= indexerRight)
		{
			var indexerMiddle = (indexerLeft + indexerRight) >> 1;
			var indexerCompare = attributeTypeId.CompareTo(span[indexerMiddle].TypeId);
			if (indexerCompare == 0) return span[indexerMiddle];
			if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			else indexerRight = indexerMiddle - 1;
		}
		return null;
	}


}
