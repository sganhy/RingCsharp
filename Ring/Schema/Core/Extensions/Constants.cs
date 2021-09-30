using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Schema.Core.Extensions
{
    internal static class Constants
    {
        /// <summary>
        /// metaData Extension
        /// </summary>
        internal const int TableId = (int)EntityType.Table;
        internal const int SchemaId = (int)EntityType.Schema;
        internal const int FieldId = (int)EntityType.Field;
        internal const int IndexId = (int)EntityType.Index;
        internal const int RelationId = (int)EntityType.Relation;
        internal const int ServiceId = (int)EntityType.Service;
        internal const int SequenceId = (int)EntityType.Sequence;
        internal const int LanguageId = (int)EntityType.Language;
        internal const int LexiconId = (int)EntityType.Lexicon;
        internal const int TablespaceId = (int)EntityType.TableSpace;
        internal static readonly Field DefaultPrimaryKey = Builders.Constants.DefaultPrimaryKey;
	    internal static readonly Field DefaultShortPrimaryKey = Builders.Constants.DefaultShortPrimaryKey;
	    internal static readonly Field DefaultIntPrimaryKey = Builders.Constants.DefaultIntPrimaryKey;
		internal static readonly Field DefaultClflyPrimaryKey = Builders.Constants.DefaultClflyPrimaryKey;

        internal const int DisabledCacheId = 0;
        internal const int MinimumCacheId = 2;

        /// <summary>
        /// MetaData Extension
        /// </summary>

        // default 
        internal static readonly string DefaultEntityName = string.Empty;
        internal static readonly string DefaultEntityDescription = DefaultEntityName;

        // meta data --> flags (bit position or bit shift)  
        internal const int FieldNotNullBitPosition = 1;
        internal const int FieldCaseSensitifBitPosition = 2;
        internal const int IndexUniqueBitPosition = 3;
        internal const int IndexBitmapBitPosition = 7;
        internal const int SchemaClfyBitPosition = 9;
        internal const int SchemaNativeBitPosition = 10;
        internal const int TableSpaceTableBitPosition = 10;
        internal const int TableSpaceIndexBitPosition = 11;
        internal const int TableSpaceReadonlyBitPosition = 12;
        internal const int FieldMultilingualBitPosition = 11;
        internal const int EntityEnabledBitPosition = 13;
        internal const int EntityBaselinedBitPosition = 14;
        internal const int TableReadonlyBitPosition = 15;
        internal const int RelationFirstBitPositionType = 32; // byte position for relations types (shilf rigth operation)

        internal const int FieldFirstBitPositionSize = 17;
        internal const int TableCacheIdBitPosition = 18;  // used for sequence too 
        internal const int TableChachedBitPosition = 19;
	    internal const int RelationNotNullBitPosition = 20;
	    internal const int RelationConstraintBitPosition = 21;
		internal const long DefaultFlags = 1L << (EntityEnabledBitPosition - 1);   // entity enabled

        internal static readonly string ReferenceIdNotFound = Builders.Constants.ReferenceIdNotFound;

        // metadata table 
        internal static readonly string MetaDataRefId = Helpers.Constants.MetaDataRefId;
        internal static readonly string MetaDataName = Builders.Constants.MetaDataName;
        internal static readonly string MetaDataSchemaId = Builders.Constants.MetaDataSchemaId;
        internal static readonly string MetaDataId = Builders.Constants.MetaDataId;
        internal static readonly string MetaDataObjectType = Builders.Constants.MetaDataObjectType;
        internal static readonly string MetaDataValue = Builders.Constants.MetaDataValue;
        internal static readonly string MetaDataDescription = Builders.Constants.MetaDataDescription;
        internal static readonly string MetaDataFlags = Builders.Constants.MetaDataFlags;

        /// <summary>
        /// Table Extension
        /// </summary>
        internal static readonly char SelectSeparator = Data.Helpers.Constants.SelectSeparator;
        internal const int FieldNameNotFound = int.MinValue; // index indentifying not found field
        internal const int RelationNameNotFound = int.MinValue; // relation not found 
        internal const int LexiconNotFound = int.MinValue; // table not found  

        // clause definition
        internal static readonly string SearchablePrefixClfy = Helpers.Constants.SearchablePrefixClfy;
        internal static readonly string PostgreConflictClause = "ON CONFLICT({0}) DO NOTHING";

        /// <summary>
        /// Tablespace Extension
        /// </summary>
        internal const char FolderSeperatorWin = '\\'; // table not found  
        internal const char FolderSeperatorUni = '/'; // table not found  

        /// <summary>
        /// Index Extension
        /// </summary>
        internal const char IndexKeySeparator = '|';
        internal const char PaddingIndexName = '0';
        internal static readonly char Underscore = '_';       // space between clause
        internal static readonly string IndexPrefixDefault = "idx" + Underscore;

        /// <summary>
        /// Sequence Extension
        /// </summary>

        /// <summary>
        /// Schema Extension
        /// </summary>
        internal static readonly string SequenceJobIdName = Builders.Constants.SequenceJobIdName;

        /// <summary>
        /// FieldType Extension
        /// </summary>
        internal const int NotDefinedFieldTypeId = Builders.Constants.NotDefinedFieldTypeId;
        internal static readonly string FieldDefaultDate = @"0001-01-01T00:00:00Z";
        internal static readonly string DefaultNumericValue = @"0";

    }
}
