using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Ring.Util;

internal static class NamingConvention
{
    private const char Space = ' ';
    private const char SnakeCaseSeparator = '_';
    private const char SpecialCharacter = '@';
    private static readonly string SpecialCharacterStr = SpecialCharacter.ToString();
    private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

    public static string? ToSnakeCase(string? name)
    {
        // Code size: 251 (0xfb)
#pragma warning disable CA1308 // Normalize strings to uppercase
        if (string.IsNullOrEmpty(name)||SpecialCharacterStr==name) return name;
        if (!CaseChangeDetected(name)) return name.ToLower(DefaultCulture);
        var i=1;
        var result = new StringBuilder(char.IsLower(name[0]) ? 
            name[0].ToString() : name[0].ToString().ToLowerInvariant());
        if (name[0]==SpecialCharacter) {
            result.Append(char.ToLower(name[i], CultureInfo.InvariantCulture));
            ++i;
        }
        if (name.Contains(Space, StringComparison.Ordinal)) name = name.Replace(Space, SnakeCaseSeparator).ToLowerInvariant();
        while (i < name.Length)
        {
            if (char.IsUpper(name[i]))
            {
                if (i<=0 || name[i-1]!= SnakeCaseSeparator) result.Append(SnakeCaseSeparator);
                result.Append(char.ToLower(name[i], CultureInfo.InvariantCulture));
            }
            else result.Append(name[i]);
            ++i;
        }

#pragma warning restore CA1308 // Normalize strings to uppercase
        return result.ToString();
    }

    private static bool CaseChangeDetected(string name)
    {
        var countUpper = 0;
        var countLower = 0;
        var span = name.AsSpan();
        foreach (var c in span)
        {
            if (char.IsUpper(c)) ++countUpper;
            if (char.IsLower(c)) ++countLower;
        }
        return (countUpper>0 && countLower>0) || (countUpper == 0 && countLower == 0);
    }
}