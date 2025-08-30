using Ring.Schema.Enums;

namespace Ring.Schema.Extensions;

internal static class StringExtensions
{
	internal static FieldType ToFieldType(this string? value) => value != null && int.TryParse(value, out var intValue) ? intValue.ToFieldType() : FieldType.Undefined; // Code size: 23 (0x17)
    internal static EntityType ToEntityType(this string? value) => value != null && int.TryParse(value, out var intValue) ? intValue.ToEntityType() : EntityType.Undefined; // Code size: 23 (0x17)
}
