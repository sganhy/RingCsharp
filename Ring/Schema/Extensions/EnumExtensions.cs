using System.Globalization;
using System.Reflection;

namespace Ring.Schema.Extensions;

internal static class EnumExtensions
{
    /// <summary>
    /// Gets the custom attribute <typeparamref name="T"/> for the enum constant, 
    ///      if such a constant is defined and has such an attribute; otherwise null.
    /// </summary>
    internal static T? GetCustomAttribute<T>(this Enum value) where T : Attribute =>
        GetField(value)?.GetCustomAttribute<T>(inherit: false);

    /// <summary>
    /// Gets the FieldInfo for the enum constant, if such a constant is defined; otherwise null.
    /// </summary>
    private static FieldInfo? GetField(this Enum value)
    {
        int s32 = ToInt32(value);
        var fields = value.GetType().GetFields(BindingFlags.Public | BindingFlags.Static);
        for (var i = 0; i < fields?.Length; i++) if (ToInt32(fields[i].GetRawConstantValue())==s32) return fields[i];
        return null;
    }

    private static int ToInt32(object? value)
    {
        switch (Convert.GetTypeCode(value))
        {
            case TypeCode.Int16:
            case TypeCode.Int32:
                return unchecked(Convert.ToInt32(value, CultureInfo.InvariantCulture));
            case TypeCode.Byte:
                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            default: throw new InvalidOperationException("Unknown EnumType");
        }
    }

}
