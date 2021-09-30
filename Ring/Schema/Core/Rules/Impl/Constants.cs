using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Schema.Core.Rules.Impl
{
    internal static class Constants
    {
        internal const long MaxValidation = 150L; // number max of validation displayed

        #region medata: name:  warning + error messages (500-599)
        internal static readonly string MetadaName = @"Invalid {0} Name";
        internal static readonly ValidationItem MetadataInvalidName = GetValidationInstance(
            501L,
            MetadaName,
            @"Invalid {0} name ""{1}"". A name can consist of any combination of letters(A to Z a to z), decimal digits(0 to 9) or underscore (_).",
            LogLevel.Error);

        internal static readonly ValidationItem MetadataEmptyName = GetValidationInstance(
            502L,
            MetadaName,
            @"{0} name cannot be empty.",
            LogLevel.Error);

        internal static readonly ValidationItem MetadataNameIsTooLong = GetValidationInstance(
            503L,
            MetadaName,
            @"{0} name ""{1}"" is too long (max length={2}).",
            LogLevel.Error);
        #endregion
        #region medata: id:  warning + error messages (600-799)
        private static readonly string MetadaId = @"Invalid {0} Id";
        internal static readonly ValidationItem MetadataEmptyId = GetValidationInstance(
            601L,
            MetadaId,
            @"{0} id cannot be empty.",
            LogLevel.Error);

        internal static readonly ValidationItem MetadataInvalidId = GetValidationInstance(
            602L,
            "Invalid number",
            @"{0} [{1}] id is invalid.",
            LogLevel.Error);

        internal static readonly ValidationItem MetadataOutOfRangeId = GetValidationInstance(
            603L,
            "Out of Range number",
            @"{0} [{1}] id is out of range (must be between " + int.MinValue + " to " + int.MaxValue + ").",
            LogLevel.Error);

        // medata: dataType:  warning + error messages 
        internal static readonly ValidationItem MetadataInvalidType = GetValidationInstance(
            701L,
            "Invalid {0} Type",
            @"Invalid {0} Type ""{1}""",
            LogLevel.Error);

        internal static readonly ValidationItem MetadataInvalidTo = GetValidationInstance(
            702L,
            "Invalid {0}",
            @"Target table name ""{0}"" is invalid",
            LogLevel.Error);

        internal static readonly ValidationItem MetadataDuplicateId = GetValidationInstance(
            703L,
            @"Duplicate table Id ""{0}""",
            @"table id ""{0}"" is already in use by table {1}",
            LogLevel.Error);

        internal static readonly ValidationItem MetadataDuplicateName = GetValidationInstance(
            705L,
            @"Duplicate relation or field",
            @"Duplicate relation or field ""{0}"" for table ""{1}""",
            LogLevel.Error);

        #endregion
        #region medata: relation:  warning + error messages (800-850)
        internal static readonly string MetadaRelationId = @"Invalid relation {0}";
        internal static readonly ValidationItem MetadataRelationInverse = GetValidationInstance(
            801L,
            MetadaRelationId,
            @"Inverse relation ""{0}"" doesn't exist.",
            LogLevel.Error);

        internal static readonly ValidationItem MetadataRelationTypeInverse = GetValidationInstance(
            802L,
            MetadaRelationId,
            @"Inverse relation type ""{0}"" is not valid.",
            LogLevel.Error);
        #endregion
        #region medata: index:  warning + error messages (851-899)
        internal static readonly ValidationItem MetadataDuplicateIndexDef = GetValidationInstance(
            851L,
            @"Duplicate index definition",
            @"Duplicate index definition ""{0}"" for table ""{1}""",
            LogLevel.Error);
        internal static readonly ValidationItem MetadataInvalidIndexDef = GetValidationInstance(
            852L,
            @"Invalid index definition",
            @"Field ""{0}"" doesn't exist for table ""{1}""",
            LogLevel.Error);
        #endregion

        internal static readonly string MsgCharSeparator = ", ";
        internal static readonly char IndexFieldSeparator = Builders.Constants.IndexFieldSeparator;

        internal const long NotDefinedFieldTypeId = Builders.Constants.NotDefinedFieldTypeId;             // (copy)
        internal const long TableIdNotFound = Builders.Constants.TableIdNotFound;                         // (copy)
        internal const int MaxSizeObjectName = Helpers.Constants.MaxSizeObjectName;                      // (copy)
        internal const int MaxSizeObjectNameWithPrefix = Helpers.Constants.MaxSizeObjectNameWithPrefix;  // (copy)

        #region private methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="loglevel"></param>
        /// <returns></returns>
        private static ValidationItem GetValidationInstance(long id, string name, string description, LogLevel loglevel)
        {
            return new ValidationItem(id, 0L, name, description, loglevel);
        }

        #endregion

    }
}
