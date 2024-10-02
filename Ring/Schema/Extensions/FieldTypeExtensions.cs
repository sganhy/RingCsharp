using Ring.Schema.Enums;
using System.Globalization;

namespace Ring.Schema.Extensions;

internal static class FieldTypeExtensions
{
    private static readonly string DefaultNumberValue = "0";
    private static readonly string DefaultBoolValue = false.ToString(CultureInfo.InvariantCulture);

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

    internal static string? GetDefaultValue(this FieldType fieldType)
    {
        switch (fieldType)
        {
            case FieldType.Int:
            case FieldType.Long:
            case FieldType.Byte:
            case FieldType.Short:
            case FieldType.Float:
            case FieldType.Double:
                return DefaultNumberValue;
            case FieldType.Boolean:
                return DefaultBoolValue;
        }
        return null;
    }

}
