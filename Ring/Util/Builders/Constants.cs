using Ring.Util.Models;
using System.Collections.Generic;

namespace Ring.Util.Builders
{
    internal static class Constants
    {

        internal static readonly char RelationSeparator = ':';
        internal const char MinDigitDelimiter = '0';
        internal const char MaxDigitDelimiter = '9';
        internal const char MergeRangeSplit = ':';

        /// <summary>
        /// RecordBuilder class constants
        /// </summary>
        internal static readonly string SheetHeaderTableName = @"table_name";
        internal static readonly string SheetHeaderFieldName = @"field_name";
        internal static readonly string SheetHeaderKeyFieldName = @"key";
        internal static readonly string SheetHeaderSourceName = @"value";
        internal const char KeySperator = ',';
        // id, lexicon_id, reference_id, value
        internal static readonly string LexiconItemLexiId = Schema.Builders.Constants.LexiconItemLexiId;
        internal static readonly string MetaDataId = Schema.Builders.Constants.MetaDataId;
        internal static readonly string MetaDataRefId = Schema.Builders.Constants.MetaDataRefId;
        internal static readonly string MetaDataValue = Schema.Builders.Constants.MetaDataValue;

        /// <summary>
        /// ExcelFieldBuilder class constants
        /// </summary>
        internal static readonly ExcelField[] DefaultExcelField = new ExcelField[0];
        internal static readonly KeyValuePair<string, Dictionary<int, string>>[] DefaultColumnArray = new KeyValuePair<string, Dictionary<int, string>>[0];

    }
}
