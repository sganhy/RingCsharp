using System.Globalization;
using System.Text;

namespace Ring.Util;

internal static class NamingConvention
{
    private static readonly char Space = ' ';
    private static readonly char SnakeCaseSeparator = '_';
    private static readonly char SpecialCharacter = '@';
    private static readonly string SpecialCharacterStr = SpecialCharacter.ToString();

    public static string? ToSnakeCase(string? name)
    {
#pragma warning disable CA1308 // Normalize strings to uppercase

        if (string.IsNullOrEmpty(name)||SpecialCharacterStr==name) return name;
        var i=1;
        var result = new StringBuilder(char.IsLower(name[0]) ? 
            name[0].ToString() : name[0].ToString().ToLowerInvariant());
        if (name[0]==SpecialCharacter) {
            result.Append(char.ToLower(name[i], CultureInfo.InvariantCulture));
            ++i;
        }
        if (name.Contains(Space, StringComparison.Ordinal))
            name = name.Replace(Space, SnakeCaseSeparator).ToLowerInvariant();
        while (i < name.Length)
        {
            if (char.IsUpper(name[i]))
            {
                result.Append(SnakeCaseSeparator);
                result.Append(char.ToLower(name[i], CultureInfo.InvariantCulture));
            }
            else result.Append(name[i]);
            ++i;
        }

#pragma warning restore CA1308 // Normalize strings to uppercase
        return result.ToString();
    }
    
}