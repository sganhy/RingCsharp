using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Globalization;
using System.Text;
using DbSchema = Ring.Schema.Models.Schema;
using Index = Ring.Schema.Models.Index;

namespace Ring.Util.Builders;

internal abstract class BaseDdlBuilder : BaseSqlBuilder, IDdlBuilder
{
	protected static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;

	// entity
	protected static readonly string DdlView = @"VIEW";
	protected static readonly string DdlTable = @"TABLE ";  // final space character needed!
	protected static readonly string DdlConstraint = @"CONSTRAINT ";
	protected static readonly string DdlIndex = @"INDEX ";
	protected static readonly string DdlSequence = @"SEQUENCE";
	protected static readonly string DdlTableSpace = @"TABLESPACE ";
	protected static readonly string DdlSchema = @"SCHEMA ";
	protected static readonly string DdlPrimaryKey = @"PRIMARY KEY ";

	// options
	protected static readonly string DdlUnique = @"UNIQUE ";
	protected static readonly string DdlUsing = @"USING ";
	protected static readonly string DdlOn = @"ON ";

	// commands
	protected static readonly string DdlReference = @"REFERENCES";
	protected static readonly string DdlCreate = @"CREATE ";
	protected static readonly string DdlAlter = @"ALTER "; // final space character needed!
	protected static readonly string DdlDrop = @"DROP ";
	protected static readonly string DdlCheck = @"CHECK ";
	protected static readonly string DdlAdd = @"ADD ";
	protected static readonly string DdlColumn = @"COLUMN ";
	protected static readonly string DdlComment = @"COMMENT ";
	protected static readonly string DdlTruncate = @"TRUNCATE ";
	protected static readonly string DdlNotNull = @"NOT NULL";
	protected static readonly string DdlDefault = @"DEFAULT ";
	protected static readonly string DdlSet = @"SET ";
	protected static readonly string DdlIs = @"IS ";

	// system table names
	protected static readonly string TableCatalogTableName = TableType.TableCatalog.GetLogicalName();
	protected static readonly string TablespaceCatalogTableName = TableType.TablespaceCatalog.GetLogicalName();
	protected static readonly string SchemaCatalogTableName = TableType.SchemaCatalog.GetLogicalName();

	// format
	protected const char Indent = '\t';

	// prefixes 
	protected static readonly string DefaultTablePrefix = @"t_";
	protected static readonly string DefaultPrimaryKeyPrefix = @"pk_";
	protected static readonly string DefaultCheckPrefix = @"ck_";
	protected static readonly string DefaultIndexPrefix = @"idx_";

	// conventions
	protected static readonly char LogSpecialEntityPrefix = TableTypeExtensions.SystemTablePrefix; // systeme table logical name prefix
	protected abstract char PhysSpecialEntityPrefix { get; }
	protected abstract string SearchableFieldPrefix { get; }
	protected abstract string AlterColumnStatment { get; }
	protected abstract string? TimeZoneOffsetPrefix { get; }
	protected abstract string GetCatalogPhysicalName(TableType tableType);
	protected abstract string GetSchemaPhysicalName(TableType tableType);
	protected abstract string GetPhysicalName(TableType tableType, Field field); // get field physical name eg. "table_schema" or "table_name" for information_schema.tables
	public bool HasTimeZoneOffsetColumn => TimeZoneOffsetPrefix is not null;
	internal Dictionary<FieldType, string> ProviderDataType => DataType;

	public string AlterAddColumn(Table table, in Column column) // Code size: 90 (0x5a)
		=> new StringBuilder()
			.Append(DdlAlter)
			.Append(DdlTable)
			.Append(table.PhysicalName)
			.Append(SqlSpace)
			.Append(DdlAdd)
			.Append(column.PhysicalName)
			.Append(SqlSpace)
			.Append(GetDataType(table, column, null))
			.ToString();
	
	public string AlterDropColumn(Table table, in Column column) // Code size: 80 (0x50)
		=> new StringBuilder()
			.Append(DdlAlter)
			.Append(DdlTable)
			.Append(table.PhysicalName)
			.Append(SqlSpace)
			.Append(DdlDrop)
			.Append(DdlColumn)
			.Append(column.PhysicalName)
			.ToString();

	public string Drop(Table table) // Code size: 42 (0x2a)
		=> new StringBuilder()	
			.Append(DdlDrop)
			.Append(DdlTable)
			.Append(table.PhysicalName)
			.ToString();

	public string Truncate(Table table)
		=> new StringBuilder()
			.Append(DdlTruncate)
			.Append(DdlTable)
			.Append(table.PhysicalName)
			.ToString();

	protected string GetPhysicalName(ConstraintType type, Table toTable, int fieldId)
	{
		// Code size: 160 (0xa0)
		var result = new StringBuilder();
		switch (type)
		{
			case ConstraintType.Check:
				result.Append(DefaultCheckPrefix);
				//physicalName (business): ck_{table_id}_{field_id}
				if (toTable.Type == TableType.Business) result.Append(toTable.Id.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0'));
				else result.Append(toTable.Name); //else physicalName (non-business): ck_{table_name}_{field_id}
				result.Append('_');
				result.Append(fieldId.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0'));
				break;
			//name:  pk_{table_name}
			case ConstraintType.PrimaryKey:
				//pk_ (3) + table_name (max 27) + delimiters (2) = 32
				//apply short version of prefix 'pk'
				result.Append(DefaultPrimaryKeyPrefix).Append(toTable.Name); // check size for specific database
				break;
		}
		return GetPhysicalName(EntityType.Constraint,result.ToString());
	}

	public string Comment(Table table)
		=> new StringBuilder() // Code size: 114 (0x72)
			.Append(DdlComment)
			.Append(DdlOn)
			.Append(DdlTable)
			.Append(table.PhysicalName)
			.Append(SqlSpace)
			.Append(DdlIs)
			.Append(SqlQuote)
			.Append(EscapeString(table.Description) ?? string.Empty)
			.Append(SqlQuote)
			.ToString();

	public string Comment(Table table, in Column column)
		=> new StringBuilder() // Code size: 133 (0x85)
			.Append(DdlComment)
			.Append(DdlOn)
			.Append(DdlColumn)
			.Append(table.PhysicalName)
			.Append('.')
			.Append(column.PhysicalName)
			.Append(SqlSpace)
			.Append(DdlIs)
			.Append(SqlQuote)
			.Append(EscapeString(table.GetDescription(column)) ?? string.Empty)
			.Append(SqlQuote)
			.ToString();

	public virtual string GetPhysicalName(EntityType entityType, string name)
	{
		// Code size: 335 (0x14f)
		switch (entityType)
		{
			case EntityType.Table: return GetTablePhysicalName(Provider, name);
			case EntityType.Schema:
			case EntityType.Tablespace:
			case EntityType.Relation:
			case EntityType.Constraint:
			case EntityType.Field:
				{
					var physicalName = GetPhysicalName(Provider, name);
					return name.Contains(LogSpecialEntityPrefix) ^ Provider.IsReservedWord(physicalName) ?
						string.Join(null, StartPhysicalNameDelimiter, physicalName, EndPhysicalNameDelimiter) : physicalName;
				}
			case EntityType.SearchableColumn:
				return Provider.IsReservedWord(name) ^ name.StartsWith(LogSpecialEntityPrefix)
				? string.Join(null, StartPhysicalNameDelimiter, SearchableFieldPrefix, name, EndPhysicalNameDelimiter) :
					SearchableFieldPrefix + name;
			case EntityType.TimeZoneColumn:
				{
					var newValue = TimeZoneOffsetPrefix + name;
					return Provider.IsReservedWord(newValue) ^ newValue.StartsWith(LogSpecialEntityPrefix)
					? string.Join(null, StartPhysicalNameDelimiter, TimeZoneOffsetPrefix, name, EndPhysicalNameDelimiter) :
						TimeZoneOffsetPrefix + name;
				}
		}
		return string.Empty;
	}
	public string GetPhysicalName(Index index, Table table)
	{
		// Code size: 154 (0x9a)
		var result = new StringBuilder(33); 
		var indexId = index.Id.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0');
		switch (table.Type)
		{
			//name:  idx_{table_id}_{index_id}
			case TableType.Business:
				result.Append(DefaultIndexPrefix).Append(table.Id).Append('_')
					.Append(indexId);
				break;
			//name:  idx_{from_table_id}_{to_table_id}_{from_relation_id}
			case TableType.Mtm:
				//TODO
				break;
			//name:  idx_{table_name}_{index_id}
			default:
				result.Append(StartPhysicalNameDelimiter)
					.Append(DefaultIndexPrefix)
					.Append(table.Name)
					.Append('_')
					.Append(indexId)
					.Append(EndPhysicalNameDelimiter);
				break;
		}
		return result.ToString();
	}

	public string GetPhysicalName(Table table, DbSchema schema)
	{
		// Code size: 97 (0x61)
		var result = new StringBuilder();
		if (table.Type.IsCatalog())	result.Append(GetSchemaPhysicalName(table.Type));
		else result.Append(GetPhysicalName(EntityType.Schema, schema.Name));
		return result.Append(SchemaSeparator).Append(GetPhysicalName(EntityType.Table, table.Name)).ToString();
	}

	public string GetPhysicalName(Field field, Table table) => GetPhysicalName(table.Type, field);
	protected abstract string MtmPrefix { get; }


	public abstract string Create(TableSpace tablespace);
	protected abstract Dictionary<FieldType, string> DataType { get; }
	protected abstract int VarcharMaxSize { get; }
	protected abstract string StringCollateInformation { get; }
	protected abstract string SchemaSeparator { get; }
	protected abstract string TablePrefix { get; }
	protected abstract string StartPhysicalNameDelimiter { get; }
	protected abstract string EndPhysicalNameDelimiter { get; }
	public virtual string Create(Index index, Table table, TableSpace? tablespace = null)
	{
		// Code size: 211 (0xd3)
		// CREATE INDEX title_idx ON films (title) WITH (deduplicate_items = off)
		var result = new StringBuilder();
		result.Append(DdlCreate);
		if (index.Unique) result.Append(DdlUnique);
		result.Append(DdlIndex)
			.Append(GetPhysicalName(index, table))
			.Append(SqlSpace)
			.Append(DdlOn)
			.Append(table.PhysicalName)
			.Append(SqlSpace)
			.Append('(');
		for (var i = 0; i<index.Columns.Length; ++i)
		{
			result.Append(index.Columns[i].PhysicalName)
				.Append(',');
		}
		result.Length--;
		result.Append(')');
		if (tablespace is not null)
		{
			result.Append(SqlSpace)
				.Append(DdlTableSpace)
				.Append(tablespace.Name);
		}
		return result.ToString();
	}
	public string Create(Constraint constraint, Table table, TableSpace? tablespace = null)
	{
		// Code size: 763 (0x2fb)
		var skipTableSpace = true;
		var result = new StringBuilder();
		result.Append(DdlAlter)
					.Append(DdlTable)
					.Append(table.PhysicalName)
					.Append(SqlSpace);
		switch (constraint.Type)
		{
			 case ConstraintType.PrimaryKey:
				result.Append(DdlAdd)
					.Append(DdlConstraint)
					.Append(GetPhysicalName(constraint))
					.Append(SqlSpace)
					.Append(DdlPrimaryKey)
					.Append('(')
					.Append(string.Join(',', table.GetPrimaryKey().ConvertAll(delegate (Column column)
					{
						return column.PhysicalName;
					})
					.ToArray()))
					.Append(')');
				skipTableSpace = false;
				break; 
			case ConstraintType.NotNull:
				//eg. alter table my_schema.my_table alter column my_column set not null;
				result.Append(AlterColumnStatment).Append(SqlSpace).Append(constraint.Columns[0].PhysicalName).Append(SqlSpace);
				if (Provider == DatabaseProvider.PostgreSql) result.Append(DdlSet);
				result.Append(DdlNotNull);
				break;
			case ConstraintType.Default:
				//eg. alter table public."@meta" alter column active SET DEFAULT 'True'
				result.Append(AlterColumnStatment).Append(SqlSpace).Append(constraint.Columns[0].PhysicalName).Append(SqlSpace);
				if (Provider == DatabaseProvider.PostgreSql) result.Append(DdlSet);
				result.Append(DdlDefault).Append(GetDefaultValue(table.Fields[constraint.Columns[0].RecordIndex]));
				break;
			case ConstraintType.Check:
				//eg. alter table public."@meta" ADD CONSTRAINT "ck_@meta_002" CHECK (object_type>=0 and object_type<=124);
				result.Append(DdlAdd)
						.Append(DdlConstraint)
						.Append(GetPhysicalName(constraint))
						.Append(SqlSpace)
						.Append(DdlCheck)
						.Append('(');
				if (constraint.MinValue.HasValue) result.Append(constraint.Columns[0].PhysicalName).Append('>').Append('=').Append(constraint.MinValue);
				result.Append(constraint.MinValue.HasValue && constraint.MaxValue.HasValue ? SqlAnd : string.Empty);
				if (constraint.MaxValue.HasValue) result.Append(constraint.Columns[0].PhysicalName).Append('<').Append('=').Append(constraint.MaxValue);
				result.Append(')');
				break; 
		}
		if (tablespace is not null && !skipTableSpace)
		{
			result
				.Append(SqlSpace)
				.Append(DdlUsing)
				.Append(DdlIndex)
				.Append(SqlSpace)
				.Append(DdlTableSpace)
				.Append(tablespace.Name);
		}
		return result.ToString();
	}
	public string Create(DbSchema schema)
	{
		var result = new StringBuilder();
		result.Append(DdlCreate)
			.Append(DdlSchema)
			.Append(schema.PhysicalName);
		return result.ToString();
	}
	public string Create(Table table, TableSpace? tablespace = null)
	{
		// Code size: 272 (0x110)
		var i = 0;
		var columnCount = table.Columns.Length;
		var result = new StringBuilder();
		var fieldInfoDico = GetFieldInfoDico(table);
		var relationInfoDico = GetRelationInfoDico(table);

		result.Append(DdlCreate)
			.Append(DdlTable)
			.Append(table.PhysicalName)
			.Append(SqlSpace)
			.Append('(')
			.Append(SqlLineFeed);

		while (i < columnCount)
		{
			var column = table.Columns[i];
			if (column.Type == EntityType.Field || column.Type == EntityType.SearchableColumn || column.Type == EntityType.TimeZoneColumn)
			{
				var field = fieldInfoDico[column.RecordIndex];
				Create(result, table, column, field, null);
			}
			if (column.Type == EntityType.Relation)
			{
				var relation = relationInfoDico[column.RecordIndex];
				Create(result, table, column, null, relation);
			}
			++i;
		}

		if (i > 0) result.Length -= 2;
		result.Append(')');
		if (tablespace is not null)
		{
			result.Append(SqlSpace)
				.Append(DdlTableSpace)
				.Append(tablespace.PhysicalName);
		}
		return result.ToString();
	}


	#region private methods 

	private string GetDataType(Table table, Column column, int? size)
	{
		// Code size: 125 (0x7d)
		var fieldType = column.FieldType;
		var result = new StringBuilder(DataType[fieldType]);
		var collateInformation = StringCollateInformation;
		if (fieldType == FieldType.String) result.Append(GetSizeInfo(size ?? table.GetField(column.Id)?.Size ?? 0));
		if (fieldType == FieldType.String || fieldType == FieldType.LongString) result.Append(SqlSpace).Append(collateInformation);
		return result.ToString();
	}

	private static string GetSizeInfo(int size) => $"({size})";

	private void Create(StringBuilder subResult, Table table, Column column, Field? field, Relation? relation)
	{
		// Code size: 82 (0x52)
		int? size = null;
		if (field is not null)
		{
			size = field.Size;
		}
		subResult.Append(Indent)
				 .Append(column.PhysicalName)
				 .Append(SqlSpace)
				 .Append(GetDataType(table, column, size))
				 .Append(',')
				 .Append(SqlLineFeed);
	}

	/// <summary>
	/// Return dictionary of fields by RecordIndex
	/// </summary>
	private static Dictionary<int, Field> GetFieldInfoDico(Table table)
	{
		// Code size: 54 (0x36)
		var result = new Dictionary<int, Field>(table.Fields.Length * 2);
		for (var i = 0; i < table.Fields.Length; ++i) 
		{
			var field = table.Fields[i];
			result.Add(i, field);
		}
		return result;
	}

	/// <summary>
	/// Return dictionary of fields by RecordIndex
	/// </summary>
	private static Dictionary<int, Relation> GetRelationInfoDico(Table table)
	{
		// Code size: 52 (0x34)
		var fieldCount = table.Fields.Length;
		var index=0;
		var result = new Dictionary<int, Relation>(table.Relations.Length);
		for (var i=0; i < table.Relations.Length; ++i)
		{
			var relation = table.Relations[i];
			if (relation.Type == RelationType.Mto || relation.Type == RelationType.Otop)
			{
				result.Add(index + fieldCount, relation);
				++index;

			}
		}
		return result;
	}

	private static string GetPhysicalName(DatabaseProvider provider, string name)
	{
		// Code size: 116 (0x74)
		// build different convention connected to ==> Provider
		switch (provider)
		{
			case DatabaseProvider.PostgreSql:
			case DatabaseProvider.MySql:
			case DatabaseProvider.SqlLite:
#pragma warning disable CA1308 // Normalize strings to uppercase
				return NamingConvention.ToSnakeCase(name)?.ToLowerInvariant() ?? string.Empty;
#pragma warning restore CA1308 // Normalize strings to uppercase
			case DatabaseProvider.SqlServer:
				return NamingConvention.ToCamelCase(name) ?? string.Empty;
			case DatabaseProvider.Oracle:
				return NamingConvention.ToSnakeCase(name)?.ToUpperInvariant() ?? string.Empty;
			default:
				return name;
		}
	}

	private string GetTablePhysicalName(DatabaseProvider provider, string name)
	{
		// special entities are not prefixed with "@"
		if (name.StartsWith(LogSpecialEntityPrefix))
		{
			// catalogs
			if (name == TableCatalogTableName) return GetCatalogPhysicalName(TableType.TableCatalog);
			if (name == TablespaceCatalogTableName) return GetCatalogPhysicalName(TableType.TablespaceCatalog);
			if (name == SchemaCatalogTableName) return GetCatalogPhysicalName(TableType.SchemaCatalog);

			return GetPhysicalName(provider, StartPhysicalNameDelimiter + name.Replace(LogSpecialEntityPrefix, PhysSpecialEntityPrefix) + EndPhysicalNameDelimiter);
		}
		else 
		{
			// business tables
			return GetPhysicalName(provider, TablePrefix + name);
		}
	}
	private static string GetPhysicalName(Constraint constraint)
	{
		return constraint.Name;
	}

	private static string GetDefaultValue(Field field)
	{
		return field.Type == FieldType.String || field.Type == FieldType.Boolean ? '\'' +  field.DefaultValue + '\'' : field.DefaultValue;
	}

	protected static Constraint GetConstraint(ConstraintType type, Table table, string physicalName, Column[] columns, int? minValue = null, int? maxValue = null)
	{ 
		return new Constraint(0, string.Empty, null,true, true,type, columns, minValue, maxValue);
	}


	#endregion

}


