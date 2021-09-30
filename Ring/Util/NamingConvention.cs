using System.Text;

namespace Ring.Util
{
    internal static class NamingConvention
    {

        /// <summary>
        /// Transform a string to PascalCase 
        /// </summary>
        public static string ToPascalCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            // from snake case 
            if (name.IndexOf(Constants.SnakeCaseSeparator) >= 0)
            {
                var result = ToCamelCase(name);
                return result.Length > 1 ? result[0] + result.Substring(1) : result.ToUpper();
            }
            //TODO implement other cases 
            return name;
        }

        /// <summary>
        /// Transform a string to camelCase 
        /// </summary>
        public static string ToCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            // from snake case
            if (name.IndexOf(Constants.SnakeCaseSeparator) >= 0)
            {
                var result = new StringBuilder(name.Length);
                if (name[0] != Constants.SnakeCaseSeparator) result.Append(char.ToLower(name[0]));
                var detectUnderscore = false;
                for (var i = 0; i < name.Length; ++i)
                    if (Constants.SnakeCaseSeparator.Equals(name[i])) detectUnderscore = true;
                    else
                    {
                        result.Append(detectUnderscore ? char.ToUpper(name[i]) : name[i]);
                        detectUnderscore = false;
                    }
                return result.ToString();
            }
            //TODO implement other cases 
            return name;
        }

        public static string ToSnakeCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            var result = new StringBuilder(char.IsLower(name[0]) ? name[0].ToString() : name[0].ToString().ToLower());
            if (name.IndexOf(Constants.Space) >= 0)
                name = name.Replace(Constants.Space, Constants.SnakeCaseSeparator).ToLower();
            for (var i = 1; i < name.Length; ++i)
                if (char.IsUpper(name[i])) result.Append(Constants.SnakeCaseSeparator.ToString() + char.ToLower(name[i]));
                else result.Append(name[i]);
            return result.ToString();
        }

        public static string ToKebabCase(string name)
        {
            return null;
        }
    }
}
