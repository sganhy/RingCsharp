namespace Ring.Schema.Helpers
{
    /// <summary>
    /// Constants used by mappers 
    /// </summary>
    internal static class Constants
    {
        // common - sql 
        internal static readonly char Space = ' ';                                  // space between clause
        internal static readonly string DoubleSpace = Space + Space.ToString();     // space between clause
	    internal static readonly char Underscore = '_';                              // space between clause
	    internal static readonly char DoubleQuote = '"';                          
		internal static readonly char SchemaSeparator = '.';
        internal static readonly string CreateTable = "Create {0} table";           // clause create table with parameters
        internal static readonly string DropTable = "DROP table {0}"; // drop table not working for oracle 
	    private static readonly string AlterTable = "ALTER table {0} ";  
		internal static readonly string AlterTableAddColumn = AlterTable + "ADD COLUMN ";  // ALTER TABLE t_address ADD COLUMN IF NOT EXISTS zorro varchar(40);
        internal static readonly string AlterTableRemoveColumn = AlterTable + "DROP COLUMN ";
        internal static readonly string AlterIfNotExist = "IF not EXISTS ";
        internal static readonly string AlterIfExist = "IF EXISTS ";
        private static readonly string Index = "Index";                     // clause create Index
        internal static readonly string CreateIndex = @"Create " + Index;               // clause create Index
        internal static readonly string DropIndex = @"Drop " + Index + @" {0}{1}";
        internal static readonly string CreateUniqueIndex = "Create UNIQUE Index";  // clause create Unique Index
        internal static readonly string CreateSchema = "CREATE SCHEMA {0}";         // clause create Schema
        internal static readonly string TableSpace = @"TABLESPACE ""{0}"" ";
        internal static readonly string NotNull = @" not null";
        internal static readonly string TruncateTable = "TRUNCATE table {0}";
        internal static readonly string AnalyzeTable = "ANALYZE {0}";
        internal static readonly string VacuumTable = "VACUUM {0}";
	    internal static readonly string AddConstraint = AlterTable + @"ADD CONSTRAINT {1}";
		internal static readonly string IndexOnTable = " ON ";
        internal static readonly char StartClause = '(';
        internal static readonly char EndClause = ')';
        internal static readonly char EndStatement = ';';
        internal static readonly char CommaSeparator = ',';
	    internal static readonly string PrimaryKeyPrefix = "pk_";
	    internal static readonly string ForeignKeyPrefix = "fk_";
	    internal static readonly string PrimaryKey = @"Primary Key";
	    internal static readonly string ForeignKey = @"Foreign Key";
	    internal static readonly string Reference = @"ReferenceS";

		// sql - clarify 
		internal static readonly string SearchablePrefixClfy = 'S' + Underscore.ToString();

        // sql - sqlite
        internal static readonly string SqliteTableDelimiter = "[{0}]";             // table delimiter
        internal static readonly string SqliteString = @"text";                     // type string + dateTime
        internal static readonly string SqliteNumber = @"integer";                  // type number
        internal static readonly string SqliteFloat = @"real";                      // type float
        internal static readonly string SqliteFieldSize = @"CHECK(length(""{0}"")<={1})";
        private static readonly string SqliteNumberConstraint = @"CHECK(""{0}"" between {1} and {2})";
        internal static readonly string SqliteLongConstraint = string.Format(SqliteNumberConstraint, "{0}", long.MinValue, long.MaxValue);
        internal static readonly string SqliteIntConstraint = string.Format(SqliteNumberConstraint, "{0}", int.MinValue, int.MaxValue);
        internal static readonly string SqliteShortConstraint = string.Format(SqliteNumberConstraint, "{0}", short.MinValue, short.MaxValue);
        internal static readonly string SqliteByteConstraint = string.Format(SqliteNumberConstraint, "{0}", sbyte.MinValue, sbyte.MaxValue);
        internal static readonly string SqliteBoolConstraint = string.Format(SqliteNumberConstraint, "{0}", 0, 1);
        internal static readonly string SqliteDateConstraint = @"CHECK(datetime(""{0}"") IS NOT NULL OR ""{0}"" IS NULL)";
        internal static readonly string SqliteRelationRef = @"ReFeReNCe";

        // sql - postgre
        internal static readonly string PostgreLong = @"int8";
        internal static readonly string PostgreInt = @"int4";
        internal static readonly string PostgreShort = @"int2";
        internal static readonly string PostgreBool = @"bool";
        internal static readonly string PostgreDouble = @"float8";
        internal static readonly string PostgreFloat = @"float4";
        internal static readonly string PostgreString = @"varchar";
        internal static readonly string PostgreBigString = SqliteString;
        internal static readonly string PostgreDate = @"date";
        internal static readonly string PostgreDateTime = @"timestamp without time zone";
        internal static readonly string PostgreStringConstraint = @"({0})";
		
        internal static readonly string PostgreByteConstraint = string.Format(@"CHECK({0}>={1} and {0}<={2})", "{0}", sbyte.MinValue.ToString(), sbyte.MaxValue.ToString());
        internal const int PostgreMaxVarcharSize = 4000;   // table delimiter
        internal static readonly string PostgreToUpperCase = @"upper({0})";   // table delimiter
	    internal static readonly string CreateUnloggedTable = @"unlogged ";          // parameter unlogged
        internal static readonly string PostgreStorageParameters = @"WITH (autovacuum_enabled=false,oids=false)";          // parameter unlogged
        internal static readonly string PostgreCreateTableSpace = @"CREATE TABLESPACE ""{0}""";
        internal static readonly string PostgreTableSpaceLocation = @" LOCATION '{0}'";
        internal static readonly string PostgreTableSpaceIndex = "USING INDEX TABLESPACE {0}";   // add contraints tablespace

        internal const int MaxSizeObjectName = 30;
        internal const int MaxSizeObjectNameWithPrefix = MaxSizeObjectName - 2;

        // script formatting 
        internal const int ScriptTypePadding = 9;  // padding char (TAB)
        internal const int ScriptNamePadding = 29; // max length

        // metadata property name
        internal static readonly string MetaDataId = Builders.Constants.MetaDataId.ToLower();
        internal static readonly string MetaDataSchemaId = Builders.Constants.MetaDataSchemaId.ToLower();
        internal static readonly string MetaDataRefId = Builders.Constants.MetaDataRefId.ToLower();
        internal static readonly string MetaDataObjectType = Builders.Constants.MetaDataObjectType.ToLower();

    }

}
