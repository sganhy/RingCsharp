using Ring.Schema.Core.Rules;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Ring.Schema.Builders
{
    /// <summary>
    /// Constants used by mappers 
    /// </summary>
    internal static class Constants
    {

        /// <summary>
        /// Static contrustor
        /// </summary>
        static Constants()
        {
            // avoid The type initializer for '...SchemaExtension.Mappers.Constants' threw an exception.
        }

        private const char Underscore = '_';                // space between clause

        // entity property name
        internal static readonly string EntityName = @"name";  // to keep entity abstract 
        internal static readonly string EntityDecription = @"description";
        internal static readonly string EntityId = @"id";
        internal static readonly string SchemaName = @"schema";
        internal static readonly string RelationToObject = "to";
        internal static readonly string EntityFieldName = nameof(Field);
        internal static readonly string EntityRelationName = nameof(Relation);
        internal static readonly string EntityIndexName = nameof(Index);
        internal static readonly string EntitySchemaName = nameof(Schema);
        internal static readonly string EntityObjectName = nameof(Object);
        internal static readonly string EntityTableName = nameof(Table);
        internal static readonly string EntityTableSpaceName = nameof(TableSpace);

        // @meta property name - should be in lower case
        internal static readonly string MetaDataId = @"id";
        internal static readonly string MetaDataName = @"name";
        internal static readonly string MetaDataSchemaId = @"schema_id";
        internal static readonly string MetaDataDescription = EntityDecription.ToLower();
        internal static readonly string MetaDataRefId = @"reference_id";
        internal static readonly string MetaDataObjectType = @"object_type";
        internal static readonly string MetaDataDataType = @"data_type";
        internal static readonly string MetaDataActive = @"active";
        internal static readonly string MetaDataModifyStmp = @"modify_stmp";
        internal static readonly string MetaDataFlags = @"flags";
        internal static readonly string MetaDataValue = @"value";

        // @log property name - should be in lower case
        internal static readonly string MetaLogId = @"id";                    // int 
        internal static readonly string MetaLogEntryTime = @"entry_time";     // date_time  
        internal static readonly string MetaLogJobId = @"job_id";             // int 
        internal static readonly string MetaSchemaId = @"schema_id";          // int 
        internal static readonly string MetaLogLevel = @"level_id";           // int
        internal static readonly string MetaLogThreadId = @"thread_id";       // long 
        internal static readonly string MetaLogCallSite = @"call_site";       // string(255) 
        internal static readonly string MetaLogMethod = @"method";            // string(80) 
        internal static readonly string MetaLogLineNumber = @"line_number";   // int
        internal static readonly string MetaLogMessage = @"message";          // string(255) 
        internal static readonly string MetaLogDescription = @"description";  // string

        // @lexicon property name- should be in lower case
        internal static readonly string LexiconGuid = @"guid";                     // string(36) 
        internal static readonly string LexiconTableId = @"table_id";              // int
        internal static readonly string LexiconToFieldId = @"target_field_id";     // int
        internal static readonly string LexiconFromFieldId = @"source_field_id";   // int
        internal static readonly string LexiconRelationId = @"relation_id";        // int 
        internal static readonly string LexiconRelationValue = @"relation_value";  // long 

        // @lexicon_item property name- should be in lower case
        internal static readonly string LexiconItemLexiId = @"lexicon_id";         // int
        internal static readonly string LexiconItemRefId = MetaDataRefId;          // int
        internal static readonly string LexiconItemValue = MetaDataValue;          // string

        // @user property name- should be in lower case
        internal static readonly string UserSchemaId = MetaSchemaId;
        internal static readonly string UserLoginName = @"user_name";
        internal static readonly string UserEmail = @"email";
        internal static readonly string UserActive = MetaDataActive;
        internal static readonly string UserFieldPswrd = @"password";

        // default value
        private static readonly string PrimaryKeyDescription = @"Internal record number";
        private static readonly string ClfyPrimaryKeyObj = @"obj" + MetaDataId;    // @"objid";
        internal const int NotDefinedFieldTypeId = -1;

        internal static readonly string DefaultSchema = @"information_schema";
        internal static readonly string DefaultRefId = @"0";
        internal static readonly string DefaultIdValue = @"0";
        internal static readonly MetaData NullMetada = MetaDataBuilder.GetNullInstance();
        internal static readonly string MtmPrefix = @"@mtm_";
        internal static readonly string MtmSeperator = Underscore.ToString();
        internal static readonly char MtmPaddingChar = '0';

        internal const int TableIdNotFound = int.MinValue;
        internal static readonly string ReferenceIdNotFound = int.MinValue.ToString();
        internal static readonly string DefaultSchemaName = @"sa";

        internal const int DefaultSchemaPort = 9300;
        internal const int DefaultMetaSchemaId = 0;
        internal const long DefaultFlags = Core.Extensions.Constants.DefaultFlags;

        // field 
        internal const int FieldNameNotFound = Core.Extensions.Constants.FieldNameNotFound; // index indentifying not found field

        // schema  - - clfy schema 
        internal static readonly string SchemaConnString = @"connectionString";
        internal static readonly string SchemaDefaultPort = @"defaultPort";
        internal static readonly string SchemaSearchPath = @"searchPath";
        internal static readonly string SchemaDefaultLanguage = @"defaultLanguage";

        // table - 
        internal static readonly string TableNameClfySubject = @"sUbject";
        internal static readonly string TableNameReadonly = @"ReadOnly";
        internal static readonly string TableSpaceFile = @"file";

        // field name - clfy schema  
        internal static readonly string FieldNameTypeId = @"Typeid";
        internal static readonly string FieldNameDataType = @"DatabaseDataType";
        internal static readonly string FieldNameDataTypeDesc = @"commonDataType";
        internal static readonly string FieldNameSize = @"ArraySize";
        internal static readonly string FieldDataTypeArray = @"Array";
        internal static readonly string FieldNameNotNull = @"AllowNull";
        internal static readonly string FieldNameCaseInsensitif = @"CaseInsensitive";
        internal static readonly string FieldNameBaseLine = @"baseline";
        internal static readonly string FieldDefaultValue = @"default";
        internal static readonly string FieldDescription = @"comment";
        internal static readonly string IndexUnique = @"unique";
        internal static readonly string IndexFieldDefault = @"index_Field";
        internal static readonly string IndexFieldDefinition = IndexFieldDefault.Replace("_", string.Empty);
        internal static readonly char IndexFieldSeparator = ';';

        // field name - native schema
        internal static readonly string FieldNameDefaultSize = @"size";
        internal static readonly string FieldNameDefaultDataType = @"type";
        internal static readonly string FieldCaseSensitif = "case_sensitive";
        internal static readonly string FieldMultilingual = @"multilingual";
        internal static readonly string FieldNameDefaultCaseSensitif = FieldCaseSensitif.Replace("_", string.Empty);
        internal static readonly string FieldNotNull = "not_null";
        internal static readonly string FieldNameDefaultNotNull = FieldNotNull.Replace("_", string.Empty);

        // relation name - clfy schema  
        internal static readonly string RelNameRelationType = @"relationType";
        internal static readonly string RelNameToTable = @"TargetTable";
        internal static readonly string RelNameDefaultExclRelation = @"exclusive_relation";
        internal static readonly string RelNameDefaultInverseRelation = @"inverseRelation";
        internal static readonly string RelNameInverseRelation = RelNameDefaultInverseRelation.Replace("_", string.Empty);
	    internal static readonly string RelNotNull = FieldNotNull;
	    internal static readonly string RelContraint = "constraint";

		// relation name - native schema
		internal static readonly string RelNameDefaultRelationType = @"tYpe";

        // Validators 
        internal static readonly IValidationRule<MetaData>[] MetadaValidators = Core.Rules.Constants.MetadaValidators;
        internal static readonly string MetaTableName = @"@meta";
        internal static readonly string LexiconTableName = @"@lexicon";
        internal static readonly string LexiconItemTableName = @"@lexicon_item";
        internal static readonly string LexiconItemPhysTableName = @"@lexicon_itm";

        /// <summary>
        /// FieldBuilder class constants
        /// </summary>
        // enum cache
        internal static readonly Dictionary<string, int> FieldTypeEnumsName = GetFieldTypeName();
        internal static readonly Field DefaultPrimaryKey = FieldBuilder.GetInstance(0, EntityId.ToLower(), FieldType.Long, PrimaryKeyDescription);
	    internal static readonly Field DefaultShortPrimaryKey = FieldBuilder.GetInstance(0, EntityId.ToLower(), FieldType.Short, PrimaryKeyDescription);
	    internal static readonly Field DefaultIntPrimaryKey = FieldBuilder.GetInstance(0, EntityId.ToLower(), FieldType.Int, PrimaryKeyDescription);
		internal static readonly Field DefaultClflyPrimaryKey = FieldBuilder.GetInstance(0, ClfyPrimaryKeyObj, FieldType.Long, PrimaryKeyDescription);

        /// <summary>
        /// SequenceBuilder class constants
        /// </summary>
        internal const long DefaultJobIdValue = 101002L;
        internal static readonly string SequenceJobIdName = @"@job_id";
        internal static readonly string SequenceJobIdDesc = @"Unique job number assigned based on auto-numbering definition";
        internal const long DefaultLexiconIdValue = 10203L;
        internal static readonly string SequenceLexiconIdName = @"@lexicon_id";
        internal static readonly string SequenceLexiconIdDesc = @"Unique Lexicon Id";
        internal const long DefaultLanguageIdValue = 1001L;
        internal static readonly string SequenceLanguageIdName = @"@language_id";
        internal static readonly string SequenceLanguageIdDesc = @"Unique Language Id";
        internal const long DefaultUserIdValue = 11L;
        internal static readonly string SequenceUserIdName = @"@user_id";
        internal static readonly string SequenceUserIdDesc = @"Unique User Id";
        internal const int SequenceReservedRangeValue = 2;
        internal const long DefaultIndexIdValue = 1001L;
        internal static readonly string SequenceIndexIdName = @"@index_id";
        internal static readonly string SequenceIndexIdDesc = @"Unique Index Id";
        internal const long DefaultEventIdValue = 1001L;
        internal static readonly string SequenceEventIdName = @"@event_id";
        internal static readonly string SequenceEventIdDesc = @"Unique Event Id";

        /// <summary>
        /// TableBuilder class constants
        /// </summary>
        internal static readonly string MetaTableIdName = @"@meta_id";
        internal static readonly string UserTableName = @"@user";
        internal static readonly string MetaDictionary = @"@dictionary_table";
        internal static readonly string MetaDictionaryTableSpace = @"@dictionary_tablespace";
        internal static readonly string MetaDictionarySchema = @"@dictionary_schema";
        internal static readonly string MetaLogTableName = @"@log";

        internal const char TableNameSperator = '.';
        internal static readonly char[] TrimSeparatorList = { '"', '[', ']' };
        internal static readonly string MetaPhysicalTableNameSperator = @"""{0}"""; // postgresql, oracle
        internal static readonly string CommonPhysicalTableNameSperator = @"{0}";
        internal static readonly string SqliteDictionary = @"sqlite_master";
        internal static readonly string SqliteDictionaryName = @"name";
        internal static readonly string SqliteDictionaryType = @"type";
        internal static readonly string SqliteDictionaryTblName = @"tbl_name";
        internal static readonly string SqliteDictionaryRootPage = @"rootpage";
        internal static readonly string SqliteDictionarySql = @"sql";
        internal static readonly string OracleDictionary = @"dba_object";
        internal static readonly string PostgreDictionary = @"pg_catalog.pg_tables";
        internal static readonly string PostgreDictionaryTableSpace = @"pg_catalog.pg_tablespace";
        internal static readonly string PostgreDictionarySchema = @"pg_catalog.pg_namespace";
        internal static readonly string PostgreDictionaryName = @"tablename";
        internal static readonly string PostgreDictionaryTableSpaceName = @"spcname";
        internal static readonly string PostgreDictionarySchemaNameField = @"nspname";
        internal static readonly string PostgreDictionarySchemaName = @"schemaname";
        internal static readonly string PostgreDictionaryOwner = @"tableowner";
        internal static readonly string FakeRecordFieldValue = MetaDataValue;
        internal static readonly LexiconIndex[] DefaultLexiconIndexes = new LexiconIndex[0];

        internal const int MaxSizeObjectName = Helpers.Constants.MaxSizeObjectName;            // (copy)
        internal static readonly CacheId MtmCacheId = new CacheId(new object(), 0L, 0L, 0);   // (copy)
        internal static readonly string TablePrefixClfy = @"table" + Underscore;
        internal static readonly string TablePrefixDefault = @"t" + Underscore;

        /// <summary>
        /// LexiconBuilder class constants
        /// </summary>
        internal const char LexiconNameSperator = '.';

        /// <summary>
        /// LanguageBuilder class constants
        /// </summary>
        internal static readonly string DefaultLanguage = @"en-US";
        internal static readonly CultureInfo DefaultCultureInfo = new CultureInfo(DefaultLanguage);
        internal static readonly Dictionary<string, CultureInfo> Cultures = GetCultureInfo();

        #region private methods

        private static Dictionary<string, int> GetFieldTypeName()
        {
            var fieldTypeEnums = (FieldType[])Enum.GetValues(typeof(FieldType));
            var result = new Dictionary<string, int>(fieldTypeEnums.Length * 4); // multiply by four to reduce collisions
            for (var i = 0; i < fieldTypeEnums.Length; ++i)
            {
                var type = fieldTypeEnums[i];
                var name = type.ToString().Trim().ToLower();
                if (!result.ContainsKey(name)) result.Add(name, (int)type);
            }
            return result;
        }

        private static Dictionary<string, CultureInfo> GetCultureInfo()
        {
            var cultures = CultureInfo.GetCultures(CultureTypes.UserCustomCulture | CultureTypes.SpecificCultures);
            var result = new Dictionary<string, CultureInfo>(cultures.Length * 4);  // multiply by four to reduce collisions
            for (var i = 0; i < cultures.Length; ++i)
                if (!result.ContainsKey(cultures[i].Name.ToUpper()))
                    result.Add(cultures[i].Name.ToUpper(), cultures[i]);
            return result;
        }

        #endregion 
    }

}
