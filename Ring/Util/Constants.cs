using System;
using System.Collections.Generic;

namespace Ring.Util
{
    internal static class Constants
    {

        /// <summary>
        /// NamingConvention class constants
        /// </summary>
        internal static readonly char SnakeCaseSeparator = '_';
        internal static readonly char Space = ' ';

        /// <summary>
        /// Hashtable class constants
        /// </summary>
        internal const int MinCapacity = 16;

        /// <summary>
        /// ObjectPool class constants
        /// </summary>
		internal static readonly string ArgObjectPoolExceptionName = @"size";
        internal static readonly string ArgObjectPoolExceptionMsg = @"The size of the pool must be greater than zero.";

        /// <summary>
        /// ExcelWorkbook class constants
        /// </summary>
        internal static readonly char PathSeparator = '/';
        internal static readonly string XmlRelationSuffix = @".rels";
        internal static readonly string XmlRelationshipNode = @"Relationship";
        internal static readonly string XmlRelationshipTarget = @"Target";
        internal static readonly string XmlFileExtension = @".XML";
        internal static readonly string XmlSharedStringsFile = @"sharedStrings.xml";
        internal static readonly string XmlWorkgroupFile = @"xl/workbook.xml";
        internal static readonly string XmlCoreFile = @"core.xml";
        internal static readonly string XmlCreatorNode = @"creator";
        internal static readonly string XmlCreated = @"created";
        internal static readonly string XmlModified = @"modified";
        internal static readonly string XmlSheetNode = @"sheet";
        internal static readonly string XmlSheetNameAttribute = @"name";
        internal static readonly string XmlSheetIdAttribute = @"id";
        internal static readonly string XmlStartSheetDocument = @"sheet";
        internal static readonly string XmlEndSheetDocument = @".xml";
        internal static readonly string XmlSheetDataRow = @"row";
        internal static readonly string XmlSheetDataRowAttribute = @"r";
        internal static readonly string XmlSheetDataColAttribute = @"c";
        internal static readonly string XmlSheetDataTypeAttribute = @"t";
        internal static readonly string XmlSheetFormatTypeAttribute = @"s";
        internal static readonly int XmlSheetFormatTypeDateTimeMin = 14;
        internal static readonly int XmlSheetFormatTypeDateTimeMax = 22;
        internal static readonly string XmlSheetDataTypeStringAttribute = @"s";
        internal static readonly string XmlSheetDataColValAttribute = @"v";
        internal static readonly string XmlSheetMergedCellInfo = @"mergeCell";
        internal static readonly string XmlSheetMergeRef = @"ref";

        /// <summary>
        /// Obfuscator class constants
        /// </summary>
        //internal static readonly string CharList = "0123456789abcdefghijklmnopqrstuvwxyz";
        internal static readonly char[] FillInCharsList = { '8', 'J', 'K', 'L', '4', 'M', 'T', 'Q', 'b', 'z', 'h' };

        // work in base 37
        internal static readonly char[] BaseChars = {
                            'm', 'a', '7', '1', 'B', '2', 'G',
                            '3', 'U', 'x', 'y', 'E', 'F', 'r',
                            '6', 'Y', 'R', 'V', 'W', 'A', '9',
                            'D', 'N', 'C', 'P', 'Z', '5', 'l',
                            'I', 'o', 'H', 'e', 'p', 'k', 'X',
                            'S'};
        internal static readonly Dictionary<char, int> CharValues = GetCharValues();
        internal const int MaxExternalkeySize = 12;
        internal static readonly int Radix = BaseChars.Length;
        internal static readonly Random Rand = new Random();

        #region private methods

        public static Dictionary<char, int> GetCharValues()
        {
            var result = new Dictionary<char, int>(BaseChars.Length);
            for (var i = 0; i < BaseChars.Length; ++i) result.Add(BaseChars[i], i);
            return result;
        }

        #endregion 

    }
}
