using System;
using System.IO;
using System.Xml;

namespace Ring.Schema.Builders
{
    internal abstract class EntityBuilder
    {

        /// <summary>
        /// Is clarify xml schema
        /// </summary>
        /// <param name="document">Xml document</param>
        protected bool IsClarifySChema(Stream document)
        {
            if (document == null) return false;
            var rootNodeName = GetRootName(document);
            if (string.IsNullOrEmpty(rootNodeName)) return false;
            return !Constants.SchemaName.Equals(rootNodeName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 
        /// Get root element name
        /// </summary>
        /// <param name="document">Stream on xml document</param>
        /// <returns>Element name</returns>
        private string GetRootName(Stream document)
        {
            if (document == null) return null;
            string result = null;
            document.Position = 0L; // start from begining
            var settings = new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                CheckCharacters = false
            };
            using (var xReader = XmlReader.Create(document, settings))
            {
                while (xReader.Read())
                {
                    // Only detect start elements.
                    if (!xReader.IsStartElement()) continue;
                    if (xReader.NodeType != XmlNodeType.Element || xReader.Name.IndexOf(Constants.SchemaName, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    result = xReader.Name;
                    break;
                }
            }
            return result;
        }

    }
}