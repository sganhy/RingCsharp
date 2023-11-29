using Ring.Schema.Enums;

namespace Ring.Schema.Extensions;

internal static class FieldTypeExtensions
{
    internal static string RecordTypeDisplay(this FieldType fieldType) {
        switch (fieldType)
        {
            case FieldType.Long: return "Int64";
            case FieldType.Int: return "Int32";
            case FieldType.Short: return "Int16";
            case FieldType.Byte: return "Int8";
            default:
                return fieldType.ToString();
        }
    }
}
