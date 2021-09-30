using System;
using System.Text;
using System.Xml;

namespace Ring.Util.Helpers
{
    internal static class XmlHelper
    {
        /// <summary>
        /// Format attribute name by removing digits and specials characters
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static string FormatAttribute(string attributeName)
        {
            if (attributeName == null) return string.Empty;
            var result = new StringBuilder();
            char c;
            for (var i = 0; i < attributeName.Length; ++i)
            {
                c = attributeName[i];
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                    result.Append(c);

            }
            //return attributeName == null ? string.Empty : new string(attributeName.Where(char.IsLetterOrDigit).ToArray());
            return result.ToString();
        }

        /// <summary>
        /// Get Attribute value by key name  (case insensitive) 
        /// </summary>
        /// <param name="reader">Pointer in the stream</param>
        /// <param name="attributeName">Name of attribute key</param>
        /// <param name="nameSpaceIncluded">Compare with namespace (true by default)</param>
        /// <returns>Value -  return null if not found</returns>
        public static string GetAttributeValue(XmlReader reader, string attributeName, bool nameSpaceIncluded = true)
        {
            if (reader == null || !reader.HasAttributes) return null;
            var criteria = FormatAttribute(attributeName);
            reader.MoveToFirstAttribute();
            if (nameSpaceIncluded)
                do if (criteria.Equals(FormatAttribute(reader.Name), StringComparison.OrdinalIgnoreCase)) return reader.Value;
                while (reader.MoveToNextAttribute());
            else
                do if (criteria.Equals(FormatAttribute(RemoveNameSpaceInfo(reader.Name)), StringComparison.OrdinalIgnoreCase)) return reader.Value;
                while (reader.MoveToNextAttribute());
            return null;
        }

        /// <summary>
        /// Remove namespace information
        /// </summary>
        /// <param name="attributeName">Name of attribute key</param>
        /// <returns>Value -  return null if not found</returns>
        public static string RemoveNameSpaceInfo(string attributeName)
        {
            if (attributeName == null) return null;
            var index = attributeName.LastIndexOf(':');
            return index >= 0 ? attributeName.Substring(index) : attributeName;
        }

    }
}
