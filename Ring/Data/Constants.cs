using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System;
using System.Collections.Generic;

namespace Ring.Data
{
    internal static class Constants
    {
        /// <summary>
        /// BulkRetrieve class constants
        /// </summary>
        internal static readonly string ErrInvalidIndex = @"This BulkRetrieve does not have a level #{0} to retrieve results for.";
        internal static readonly string ErrInvalidPageSize = @"An invalid page size was detected (page size should be greater than 0).";
        internal static readonly string ErrInvalidPageNumber = @"An invalid page number was detected (page size should be greater than 1).";
        internal static readonly string ErrInvalidNumber = "The value '{0}' is not a valid number.";
        internal static readonly string ErrInvalidDate = "Date/time entered is not valid.";
        internal static readonly string ErrInvalidList = "An invalid list was detected (at {0}).";
        internal static readonly string ErrIndexAlreadyExist = @"ThisIndex {0} already exist.";
        internal static readonly string ErrInvalidListType = @"An invalid list type was detected (at {0}).";
        internal static readonly string ErrInvalidOperation = @"No conversion proc for 'unknown data type' (from 'collection header' to 'long integer').";
        internal static readonly string ErrCriteriaListEmpty = "The list of criteria can't be empty.";
        internal static readonly string ErrParentEntryIndex = "The parent index '{0}' for the traversal map number '{1}' is invalid.";
        internal static readonly string ErrTraverseFromRoot = "Circular traversal map for traversal map number '{0}' is invalid.";
        internal static readonly string ErrInvalidSchemaName = "Schema name {0} is not valid.";

        /// <summary>
        /// Record class constants
        /// </summary>
        internal static readonly string ErrUnknowFieldName = @"FieldExtension name '{0}' does not exist for object type '{1}'.";
        internal static readonly string ErrUnknowRecordType = @"This Record object has an unknown RecordType.  The RecordType \r\nproperty must be set before performing this operation.";
        internal static readonly string ErrInvalidArgumentField = @"Wrong type or arguments for this kind of operation.";
        internal static readonly string ErrInvalidObject = @"Object type '{0}' is not valid.";
        internal static readonly string ErrUnknowRelName = @"Relation name '{0}' does not exist for object type '{1}'.";
        internal static readonly string ErrInvalidDateField = @"FieldExtension name '{0}' is not a date/time.";
        internal static readonly string ErrInvalidIntField = @"FieldExtension name '{0}' is not a integer.";

        // record - display 
        internal static readonly string DisplayObject = @" object:";
        internal static readonly string FieldIndent = @"  ";
        internal static readonly string DefaultDisplaySpace = @" ";
        internal static readonly string DefaultDisplayString = @"''";
        internal static readonly string DefaultDisplayDate = "?/?/? ?:?:?";
        internal static readonly string DefaultDisplayNumber = @"0";
        internal static readonly string DisplayStringSeparator = "'";
        internal static readonly char DisplayDatePadding = '0';
        internal static readonly char DisplayDateSeparator = '/';
        internal static readonly char DisplayTimeSeparator = ':';
        internal static readonly string BooleanTrue = "1";
        internal static readonly string BooleanFalse = "0";

        // record - dateTime format 
        internal static readonly string RcdFormatYear = @"0000";
        internal static readonly string RcdFormatMonth = @"00";
        internal static readonly string RcdFormatDay = RcdFormatMonth;
        internal static readonly string RcdFormatHour = RcdFormatMonth;
        internal static readonly string RcdFormatMinute = RcdFormatMonth;
        internal static readonly string RcdFormatSecond = RcdFormatMonth;
        internal static readonly string RcdFormatMilliSecond = @"000";
        internal static readonly char RcdTimeSeperator = 'T';
        internal static readonly char RcdHourSeperator = ':';
        internal static readonly char RcdMilliSecondSeperator = '.';
        internal static readonly char RcdDateSeperator = '-';
        internal static readonly char RcdTimeZoneInfo = 'Z';

        // record - default value
        internal const int FieldNameNotFound = Schema.Core.Extensions.Constants.FieldNameNotFound; // field not found (copy)
        internal const int RelationNameNotFound = Schema.Core.Extensions.Constants.RelationNameNotFound; // field not found (copy)
        internal static readonly string RcdDefaultId = @"0";

        // record - bit manipulation
        internal const int RcdDefaultShiftLeft = 6; // to replace divided by 64
        internal const int Mask64Bits = 0x3F;       // 111111b

        /// <summary>
        /// PathEvaluator class constants
        /// </summary>
        internal static readonly string ErrPathInvalidFocusType = @"Focus type {0} is not valid.";
        internal static readonly string ErrPathInvalidSchemaId = @"Schema id {0} is not valid.";
        internal static readonly string ErrPathInvalidRelationName =
            "For the current path '{0}', '{1}' is not a valid relation for the given object type '{2}'.";
        internal static readonly string ErrPathInvalidFieldName =
            "For the current path '{0}', '{1}' is not a valid field for the given object type '{2}'.";

        // pathEvaluator - others
        internal static readonly string RootKey = @"";
        internal const char PathSeparator = ':';
        private static readonly string FocusObject = "focus_obj";
        internal static readonly string TimeBombTableName = "time_bomb";
        internal static readonly string TimeBombReference = FocusObject + @"2" + TimeBombTableName;
        internal static readonly PathEvaluatorResult DefaultPathEvaluatorResult = new PathEvaluatorResult(FieldType.NotDefined, null);
        internal static readonly string LastFloatDigit = @".0";
        internal static readonly int LastFloatDigitLen = LastFloatDigit.Length;

        /// <summary>
        /// Bulksave class constants
        /// </summary>
        internal static readonly string BulksErrorMsg = "The given object must be a permanent database object for this operation to succeed.";
        internal static readonly string BulksReadOnlyErrorMsg = "The given object {0} cannot be modified.";
        internal static readonly string BulksErrUnknowRecordType = ErrUnknowRecordType;

        // fake tables
        internal static readonly Table FakeStringTable = TableBuilder.GetFake(ItemType.String);
        internal static readonly Table FakeLongTable = TableBuilder.GetFake(ItemType.Long);

        /// <summary>
        /// List class constants
        /// </summary>

        internal static readonly string DefaultFieldName = @"value"; // see LoadTable()

        // list - error messages
        internal static readonly string ErrInvalidType = @"Invalid type '{0}'.";
        internal static readonly string ErrMethodNotSupported = @"Method not supported for object type '{0}'";
        internal static readonly string ErrOutOfRange = @"Index {0} is out of range for list; should be between 0 and {1}.";
        internal static readonly string ErrReplaceInvalidType = @"Cannot replace an element of type '{0}' to a list of type '{1}'.";
        internal static readonly string ErrAppendInvalidType = @"Cannot add an element of type '{0}' to a list of type '{1}'.";

        internal const int CommandLineTimeOut = 200;
        internal const int ItemNotFound = -1;

        // enum cache
        internal static readonly Dictionary<string, ItemType> ListTypeEnumsName = GetListTypeName();  // string name in lowercase

        internal static readonly string Iso8601Format = @"yyyy-MM-ddTHH:mm:ssZ";

        /// <summary>
        /// Import class
        /// </summary>
        internal static readonly string InfoStartImport = @"Starting data import";
        internal static readonly string WarnSchemaNotExistsDesc = @"Schema name {0} doesn't exist!";
        internal static readonly string InfoStartImportDesc = @"File='{0}'";
        internal static readonly string InfoEndImport = @"End data import";
        internal static readonly string InfoEndImportDesc = @"Insert count={0}; update count={1}";
        internal static readonly string LexiconItemLexiId = Schema.Builders.Constants.LexiconItemLexiId;
	    internal static readonly string WarnMsgMissingValue = "Missing value";
	    internal static readonly string WarnMsgMissingFieldDesc = "{0} Field name '{1}' cannot be null";
	    internal static readonly string WarnMsgMissingReldDesc = "{0} Relation name '{1}' cannot be null";
		//internal static readonly string WarnMsgDuplicateKey = "Duplicate key";
		//internal static readonly string WarnDescDuplicateKey = "Duplicate key at line {0} and {1}";

		// ISO 8601 dateTime
		internal static readonly string Digit2Format = @"00";
        internal static readonly string Digit4Format = @"0000";
        internal static readonly string DateFormat = @"{0}-{1}-{2}T{3}:{4}:{5}Z";
        internal static readonly DateTime BaseDate = new DateTime(1899, 12, 30);

        #region private methods

        private static Dictionary<string, ItemType> GetListTypeName()
        {
            var listTypeEnums = (ItemType[])Enum.GetValues(typeof(ItemType));
            var result = new Dictionary<string, ItemType>(listTypeEnums.Length * 4); // multiply by four to reduce collisions
            for (var i = 0; i < listTypeEnums.Length; ++i)
            {
                var type = listTypeEnums[i];
                var name = type.ToString().Trim().ToLower();
                if (!result.ContainsKey(name)) result.Add(name, type);
            }
            return result;
        }

        #endregion 

    }
}
