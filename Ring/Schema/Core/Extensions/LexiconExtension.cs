using Ring.Schema.Models;
using System.Collections.Generic;

namespace Ring.Schema.Core.Extensions
{
    internal static class LexiconExtension
    {
        /// <summary>
        /// Get index of language not case sensitive ==> O(n log n) 
        /// </summary>
        /// <param name="lexicon">Current table</param>
        /// <param name="name">Language Name</param>
        /// <returns>Return index position otherwise int.MinValue (Constants.LexiconNotFound)</returns>
        public static int GetLanguageIndex(this Lexicon lexicon, string name)
        {
            if (lexicon == null || name == null || lexicon.Languages == null) return Constants.LexiconNotFound;
            int indexerLeft = 0, indexerRigth = lexicon.Languages.Length - 1;
            name = name.ToUpper();
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
                var indexerCompare = string.CompareOrdinal(name, lexicon.Languages[indexerMiddle].NameUpper);
                if (indexerCompare == 0) return indexerMiddle;
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return Constants.LexiconNotFound;
        }

        /// <summary>
        /// Get translation dictionary 
        /// </summary>
        public static Dictionary<string, string> GetTranslation(this Lexicon lexicon, string name)
        {
            var languageIndex = GetLanguageIndex(lexicon, name);
            return languageIndex < lexicon.Translations.Length ? lexicon.Translations[languageIndex] : null;
        }

        /// <summary>
        /// Is using key field
        /// </summary>
        public static bool IsKeyField(this Lexicon lexicon) => lexicon?.ToField != null && lexicon.ToField.Id == lexicon.FromField.Id;

        /// <summary>
        /// Is unique field 
        /// </summary>
        public static bool IsUniqueField(this Lexicon lexicon) => lexicon?.ToField != null;

    }
}
