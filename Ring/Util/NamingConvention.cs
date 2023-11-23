using System.Globalization;
using System.Text;

namespace Ring.Util;

internal static class NamingConvention
{
    private static readonly char Space = ' ';
    private static readonly char SnakeCaseSeparator = '_';

    public static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
#pragma warning disable CA1308 // Normalize strings to uppercase
        var result = new StringBuilder(char.IsLower(name[0]) ? 
            name[0].ToString() : name[0].ToString().ToLowerInvariant());
        if (name.Contains(Space, StringComparison.Ordinal))
            name = name.Replace(Space, SnakeCaseSeparator).ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
        for (var i = 1; i < name.Length; ++i)
            if (char.IsUpper(name[i]))
            {
                result.Append(SnakeCaseSeparator);
                result.Append(char.ToLower(name[i], CultureInfo.InvariantCulture));
            }
            else result.Append(name[i]);
        return result.ToString();
    }
    
}