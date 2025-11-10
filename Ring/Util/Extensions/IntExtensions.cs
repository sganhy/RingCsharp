using Ring.Schema.Enums;
using System.Runtime.CompilerServices;

namespace Ring.Util.Extensions;

internal static class IntExtensions
{
	#region constants

	// entity type constants
	private const int TableId = (int)EntityType.Table;
	private const int FieldId = (int)EntityType.Field;
	private const int RelationId = (int)EntityType.Relation;
	private const int IndexId = (int)EntityType.Index;
	private const int ViewId = (int)EntityType.View;
	private const int SchemaId = (int)EntityType.Schema;
	private const int SequenceId = (int)EntityType.Sequence;
	private const int LanguageId = (int)EntityType.Language;
	private const int TablespaceId = (int)EntityType.Tablespace;
	private const int ParameterId = (int)EntityType.Parameter;
	private const int AliasId = (int)EntityType.Alias;
	private const int ConstraintId = (int)EntityType.Constraint;
	private const int SearchableColumnId = (int)EntityType.SearchableColumn;
	private const int TimeZoneColumnId = (int)EntityType.TimeZoneColumn;

	// field searchable constants
	private const int FieldStIgnoreCaseId = (int)SearchableType.IgnoreCase;
	private const int FieldStIngoreCaseAndDiacriticsId = (int)SearchableType.IgnoreDiacritic;

	// field types constants
	private const int FieldTypeLongId = (int)FieldType.Long;
	private const int FieldTypeIntId = (int)FieldType.Int;
	private const int FieldTypeShortId = (int)FieldType.Short;
	private const int FieldTypeByteId = (int)FieldType.Byte;
	private const int FieldTypeFloatId = (int)FieldType.Float;
	private const int FieldTypeDoubleId = (int)FieldType.Double;
	private const int FieldTypeStringId = (int)FieldType.String;
	private const int FieldTypeShortDateTimeId = (int)FieldType.Date;
	private const int FieldTypeDateTimeId = (int)FieldType.DateTime;
	private const int FieldTypeLongDateTimeId = (int)FieldType.DateTimeOffset;
	private const int FieldTypeByteArrayId = (int)FieldType.ByteArray;
	private const int FieldTypeBooleanId = (int)FieldType.Boolean;
	private const int FieldTypeLongStringId = (int)FieldType.LongString;

	// table types constants
	private const int TableTypeBusinessId = (int)TableType.Business;
	private const int TableTypeBusinessLogId = (int)TableType.BusinessLog;
	private const int TableTypeMetaId = (int)TableType.Meta;
	private const int TableTypeMetaIdId = (int)TableType.MetaId;
	private const int TableTypeFakeId = (int)TableType.Fake;
	private const int TableTypeMtmId = (int)TableType.Mtm;
	private const int TableTypeLogId = (int)TableType.Log;
	private const int TableTypeTestId = (int)TableType.Test;
	private const int TableTypeLexiconId = (int)TableType.Lexicon;
	private const int TableTypeLexiconItemId = (int)TableType.LexiconItem;
	private const int TableTypeSchemaCatalogId = (int)TableType.SchemaCatalog;
	private const int TableTypeTableCatalogId = (int)TableType.TableCatalog;
	private const int TableTypeTableSpaceCatalogId = (int)TableType.TableSpaceCatalog;
	private const int TableTypeLogicalId = (int)TableType.Logical;

	// relation types constants
	private const int RelationTypeOtopId = (int)RelationType.Otop;
	private const int RelationTypeOtmId = (int)RelationType.Otm;
	private const int RelationTypeMtmId = (int)RelationType.Mtm;
	private const int RelationTypeMtoId = (int)RelationType.Mto;
	private const int RelationTypeOtofId = (int)RelationType.Otof;


	// database provider
	private const int ProviderOracleId = (int)DatabaseProvider.Oracle;
	private const int ProviderPostgreSqlId = (int)DatabaseProvider.PostgreSql;
	private const int ProviderMySqlId = (int)DatabaseProvider.MySql;
	private const int ProviderInfluxDbId = (int)DatabaseProvider.InfluxDb;
	private const int ProviderSqlServerId = (int)DatabaseProvider.SqlServer;
	private const int ProviderSqlLiteId = (int)DatabaseProvider.SqlLite;

	// parameter type
	private const int SchemaVersionId = (int)ParameterType.SchemaVersion;
	private const int SchemaCreationTimeId = (int)ParameterType.SchemaCreationTime;
	private const int SchemaLastUpgradeId = (int)ParameterType.LastUpgrade;
	private const int DefaultLanguageId = (int)ParameterType.DefaultLanguage;
	private const int MinPoolSizeId = (int)ParameterType.MinPoolSize;
	private const int MaxPoolSizeId = (int)ParameterType.MaxPoolSize;
	private const int DbConnectionStringId = (int)ParameterType.DbConnectionString;
	private const int DbConnectionTypeId = (int)ParameterType.DbConnectionType;

	#endregion

	internal static DatabaseProvider ToDatabaseProvider(this int providerId)
	{
		// Code size: 57 (0x39)
		switch (providerId)
		{
			case ProviderOracleId: return DatabaseProvider.Oracle;
			case ProviderPostgreSqlId: return DatabaseProvider.PostgreSql;
			case ProviderMySqlId: return DatabaseProvider.MySql;
			case ProviderInfluxDbId: return DatabaseProvider.InfluxDb;
			case ProviderSqlServerId: return DatabaseProvider.SqlServer;
			case ProviderSqlLiteId: return DatabaseProvider.SqlLite;
		}
		return DatabaseProvider.Undefined;
	}

	/// <summary>
	/// 	Casting from int to TableType
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static TableType ToTableType(this int dataType)
	{
		// Code size: 141 (0x8d)
		switch (dataType)
		{
			case TableTypeBusinessId: return TableType.Business;
			case TableTypeBusinessLogId: return TableType.BusinessLog;
			case TableTypeMetaId: return TableType.Meta;
			case TableTypeMetaIdId: return TableType.MetaId;
			case TableTypeFakeId: return TableType.Fake;
			case TableTypeMtmId: return TableType.Mtm;
			case TableTypeLogId: return TableType.Log;
			case TableTypeTestId: return TableType.Test;
			case TableTypeLexiconId: return TableType.Lexicon;
			case TableTypeLexiconItemId: return TableType.LexiconItem;
			case TableTypeSchemaCatalogId: return TableType.SchemaCatalog;
			case TableTypeTableCatalogId: return TableType.TableCatalog;
			case TableTypeTableSpaceCatalogId: return TableType.TableSpaceCatalog;
			case TableTypeLogicalId: return TableType.Logical;
		}
		return TableType.Undefined;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static FieldType ToFieldType(this int dataType)
	{
		// Code size: 127 (0x7f)
		// high performance ! 
		// avoid boxing operation - add unit test on all field type enum fields
		switch (dataType)
		{
			case FieldTypeLongId: return FieldType.Long;
			case FieldTypeIntId: return FieldType.Int;
			case FieldTypeShortId: return FieldType.Short;
			case FieldTypeByteId: return FieldType.Byte;
			case FieldTypeFloatId: return FieldType.Float;
			case FieldTypeDoubleId: return FieldType.Double;
			case FieldTypeStringId: return FieldType.String;
			case FieldTypeShortDateTimeId: return FieldType.Date;
			case FieldTypeDateTimeId: return FieldType.DateTime;
			case FieldTypeLongDateTimeId: return FieldType.DateTimeOffset;
			case FieldTypeByteArrayId: return FieldType.ByteArray;
			case FieldTypeBooleanId: return FieldType.Boolean;
			case FieldTypeLongStringId: return FieldType.LongString;
		}
		return FieldType.Undefined;
	}


	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static SearchableType ToSearchableType(this int value)
	{
		// Code size: 16 (0x10)
		switch (value)
		{
			case FieldStIgnoreCaseId: return SearchableType.IgnoreCase;
			case FieldStIngoreCaseAndDiacriticsId: return SearchableType.IgnoreDiacritic;
		}
		return SearchableType.None;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static RelationType ToRelationType(this int flags)
	{
		// Code size: 47 (0x2f)
		// avoid boxing operation
		switch (flags)
		{
			case RelationTypeOtopId: return RelationType.Otop;
			case RelationTypeOtmId: return RelationType.Otm;
			case RelationTypeMtmId: return RelationType.Mtm;
			case RelationTypeMtoId: return RelationType.Mto;
			case RelationTypeOtofId: return RelationType.Otof;
		}
		return RelationType.Undefined;
	}

	internal static ParameterType ToParameterType(this int id)
	{
		// Code size: 79 (0x4f)
		switch (id)
		{
			case SchemaVersionId:  return ParameterType.SchemaVersion;
			case SchemaCreationTimeId: return ParameterType.SchemaCreationTime;
			case SchemaLastUpgradeId: return ParameterType.LastUpgrade;
			case DefaultLanguageId: return ParameterType.DefaultLanguage;
			case MinPoolSizeId: return ParameterType.MinPoolSize;
			case MaxPoolSizeId: return ParameterType.MaxPoolSize;
			case DbConnectionStringId: return ParameterType.DbConnectionString;
			case DbConnectionTypeId: return ParameterType.DbConnectionType;
		}
		return ParameterType.Undefined;
	}

	internal static EntityType ToEntityType(this int entityType) 
	{
		// Code size: 143 (0x8f)
		// avoid boxing operation
		switch (entityType)
		{
			case TableId: return EntityType.Table;
			case FieldId: return EntityType.Field;
			case RelationId: return EntityType.Relation;
			case IndexId: return EntityType.Index;
			case ViewId: return EntityType.View;
			case SchemaId: return EntityType.Schema;
			case SequenceId: return EntityType.Sequence;
			case LanguageId: return EntityType.Language;
			case TablespaceId: return EntityType.Tablespace;
			case ParameterId: return EntityType.Parameter;
			case AliasId: return EntityType.Alias;
			case ConstraintId: return EntityType.Constraint;
			case SearchableColumnId: return EntityType.SearchableColumn;
			case TimeZoneColumnId: return EntityType.TimeZoneColumn;
		}
		return EntityType.Undefined;
	}

    /// <summary>
    ///		Gets the length of the string representation of an Int32 value. 
    /// </summary>
    internal static int GetInt32Length(this int value)
	{
		// Code size: 137 (0x89)
		if (value == 0) return 1;
		if (value == int.MinValue) return 11; // "-2147483648"
		var length = value < 0 ? 1 : 0; // Sign
		value = Math.Abs(value);
		// Count digits
		if (value < 10) return length + 1;
		if (value < 100) return length + 2;
		if (value < 1000) return length + 3;
		if (value < 10000) return length + 4;
		if (value < 100000) return length + 5;
		if (value < 1000000) return length + 6;
		if (value < 10000000) return length + 7;
		if (value < 100000000) return length + 8;
		if (value < 1000000000) return length + 9;
		return length + 10;
	}

}