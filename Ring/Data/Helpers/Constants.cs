namespace Ring.Data.Helpers
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

        internal static readonly string Select = @"SELECT ";
        internal static readonly string From = @" FROM ";
        internal static readonly string Where = @" WHERE ";  // important keep space character !!
        internal static readonly char Space = ' ';
        internal static readonly char SelectSeparator = ',';
        internal static readonly string And = @" AND ";          // important keep space character !!
        internal static readonly string OrderBy = @" ORDER BY "; // important keep space character !!
        internal static readonly string OrderByDesc = @" DESC";  // important keep space character !!
        internal static readonly string OrderByAsc = @" ASC";    // important keep space character !!

        internal static readonly char StartParenthesis = '(';
        internal static readonly char EndParenthesis = ')';

        // operands
        internal static readonly char BindVariablePrefix = ':';
        internal static readonly char BindVariableName = 'B';
        internal static readonly string FirstBindVariableName = "B0";
        internal static readonly string SecondBindVariableName = "B1";
        internal static readonly string ThirdBindVariableName = "B2";
        internal static readonly string FourthBindVariableName = "B3";
        internal static readonly string FifthBindVariableName = "B4";
        internal static readonly string NullOperand = " Null ";

        // rowid - for each provider
        internal static readonly string SqliteRowid = "_rowid_";
        internal static readonly string PostgreRowid = "CTID";

        // operators 
        internal static readonly string Equal = @"=";
        internal static readonly string IsOpp = @" Is ";
        internal static readonly string NotEqual = @"!=";
        internal static readonly string Greater = @">";
        internal static readonly string GreaterOrEqual = @">=";
        internal static readonly string Less = @"<";
        internal static readonly string LessOrEqual = @"<=";
        internal static readonly string Like = @" Like ";
        internal static readonly string NotLike = @" Not Like ";
        internal static readonly string In = @" In ";
        internal static readonly string NotIn = @" Not In ";
        internal static readonly string NullRelation = @"0";

        internal const int MaxInElement = 255; // max element in a Operator.In filter

        // error message  
        internal static readonly string ErrUnknowRecordType = Data.Constants.ErrUnknowRecordType;

        // query - default value sqlite + sqlServer
        internal static readonly IDbParameter[] DefaultFilters = new IDbParameter[0];
        internal static readonly string MetaDataIdWhere = @"id=:B1 AND schema_id=:B2 AND object_type=:B3";
        internal static readonly string MetaDataWhere = @"id=:B1 AND schema_id=:B2 AND object_type=:B3 AND reference_id=:B4";
        internal static readonly string MetaDataSet = @"flags=:B5, description=:B6, value=:B7";
        internal static readonly string LeviconItemWhere = @"lexicon_id=:B0";
        internal static readonly string MetaDataIdSetField = @"value";
        internal static readonly string MetaDataIdSet = MetaDataIdSetField + @"=" + MetaDataIdSetField + " + :" + FirstBindVariableName;

        internal static readonly string DefaultSqlFullScan = Select + @"{0}" + From + "{1}";
        internal static readonly string DeleteBaseQuery = @"DELETE FROM {0} WHERE ";
        internal static readonly string DeleteQuery = DeleteBaseQuery + @"{1}=:B0";
        internal static readonly string DeleteMtmQuery = DeleteQuery + @" AND {2}=:B1";
        internal static readonly string DeleteMetaIdQuery = DeleteBaseQuery + MetaDataIdWhere;
        internal static readonly string DeleteMetaQuery = DeleteBaseQuery + MetaDataWhere;
        internal static readonly string DeleteLexiconItemQuery = DeleteBaseQuery + LeviconItemWhere;
        internal static readonly string InsertQuery = @"INSERT INTO {0} ({1}) VALUES ({2})";
        internal static readonly string UpdateQuery = @"UPDATE {0} SET {1} WHERE {2}";
        internal static readonly string UpdateQueryWithReturning = UpdateQuery + @" RETURNING {3}";

        // query sqlite
        internal static readonly string PageSizeLite = @" LIMIT {0}"; // keep space char



        // metadata property name
        internal static readonly string LexiconItemLexiId = Schema.Builders.Constants.LexiconItemLexiId;
        internal static readonly string MetaDataId = Schema.Builders.Constants.MetaDataId.ToLower();
        internal static readonly string MetaDataSchemaId = Schema.Builders.Constants.MetaDataSchemaId.ToLower();
        internal static readonly string MetaDataObjectType = Schema.Builders.Constants.MetaDataObjectType.ToLower();
        internal static readonly string MetaDataRefId = Schema.Builders.Constants.MetaDataRefId.ToLower();

    }

}
