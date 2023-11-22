using System.Text;

namespace Ring.Util;

internal static class NamingConvention
{
    private static readonly char Space = ' ';
    private static readonly char SnakeCaseSeparator = '_';

    public static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var result = new StringBuilder(char.IsLower(name[0]) ? name[0].ToString() : name[0].ToString().ToLower());
        if (name.IndexOf(Space) >= 0)
            name = name.Replace(Space, SnakeCaseSeparator).ToLower();
        for (var i = 1; i < name.Length; ++i)
            if (char.IsUpper(name[i])) result.Append(SnakeCaseSeparator.ToString() + char.ToLower(name[i]));
            else result.Append(name[i]);
        return result.ToString();
    }
    
}