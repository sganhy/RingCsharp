using Ring.Schema.Enums;

namespace Ring.Schema.Extensions;

internal static class FieldTypeExtensions
{
    internal static string RecordTypeDisplay(this FieldType fieldType) {
#pragma warning disable IDE0066 // Convert switch statement to expression
        switch (fieldType)
        {
            case FieldType.Long: return "Int64";
            case FieldType.Int: return "Int32";
            case FieldType.Short: return "Int16";
            case FieldType.Byte: return "Int8";
            default:
                return fieldType.ToString();
        }
#pragma warning restore IDE0066
    }
}
