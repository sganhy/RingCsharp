using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Text;
using Index = Ring.Schema.Models.Index;
using DbSchema = Ring.Schema.Models.Schema;
using System.Globalization;
using Ring.Schema;
using System.ComponentModel.DataAnnotations;

namespace Ring.Util.Builders;

internal abstract class BaseDdlBuilder : BaseSqlBuilder, IDdlBuilder
{
	protected static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	

    // entity
    protected static readonly string DdlView = @"VIEW";
	protected static readonly string DdlTable = @"TABLE ";  // final space character needed !
	protected static readonly string DdlConstraint = @"CONSTRAINT ";
	protected static readonly string DdlIndex = @"INDEX ";
	protected static readonly string DdlSequence = @"SEQUENCE";
	protected static readonly string DdlTableSpace = @"TABLESPACE ";
	protected static readonly string DdlSchema = @"SCHEMA ";
	protected static readonly string DdlPrimaryKey = @"PRIMARY KEY ";

	// options
	protected static readonly string DdlUnique = @"UNIQUE ";
	protected static readonly string DdlBitmap = @"BITMAP";
	protected static readonly string DdlHash = @"HASH";
	protected static readonly string DdlUsing = @"USING ";
	protected static readonly string DdlOn = @"ON ";

	// commands
	protected static readonly string DdlReference = @"REFERENCES";
	protected static readonly string DdlCreate = @"CREATE ";
	protected static readonly string DdlAlter = @"ALTER "; // final space character needed !
	protected static readonly string DdlDrop = @"DROP ";
	protected static readonly string DdlAdd = @"ADD ";
	protected static readonly string DdlColumn = @"COLUMN ";
	protected static readonly string DdlTruncate = @"TRUNCATE ";
	protected static readonly string DdlNotNull = @"NOT NULL";

	// format
	protected const char Indent = '\t';

	// prefixes 
	protected static readonly string DefaultTablePrefix = @"t_";
	protected static readonly string DefaultPrimaryKeyPrefix = @"pk_";
	protected static readonly string DefaultIndexPrefix = @"idx_";

	// conventions
	protected readonly static char SpecialEntityPrefix = '@';
	protected abstract string SearchableFieldPrefix { get; }
    protected abstract string? TimeZoneOffsetPrefix { get; }
	protected abstract string GetPhysicalName(Constraint constraint);
	public bool HasTimeZoneOffsetColumn => TimeZoneOffsetPrefix != null;

    protected BaseDdlBuilder() {}

	public string AlterAddColumn(Table table, IColumn column) // Code size: 90 (0x5a)
		=> new StringBuilder()
			.Append(DdlAlter)
			.Append(DdlTable)
			.Append(table.PhysicalName)
			.Append(SqlSpace)
			.Append(DdlAdd)
			.Append(column.PhysicalName)
			.Append(SqlSpace)
			.Append(GetDataType(column, true))
			.ToString();
	
	public string AlterDropColumn(Table table, IColumn column) // Code size: 80 (0x50)
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
		
	public string GetSecondColumn(Field field)
	{
        // Code size: 194 (0xc2)
        if (field.IsSearchable()) return Provider.IsReservedWord(field.Name) ^ field.Name.StartsWith(SpecialEntityPrefix) 
				? string.Join(null, StartPhysicalNameDelimiter, SearchableFieldPrefix, field.Name, EndPhysicalNameDelimiter) : 
			SearchableFieldPrefix + field.Name;
		// check DatabaseProvider
		if (TimeZoneOffsetPrefix != null && field.Type == FieldType.LongDateTime)
			return string.Join(null, StartPhysicalNameDelimiter, TimeZoneOffsetPrefix, field.Id.ToString(DefaultCulture), EndPhysicalNameDelimiter) ;
		return string.Empty; // a crash will be better here
	}

	public virtual string GetPhysicalName(EntityType entityType, string name)
	{
		switch (entityType)
		{
			case EntityType.Schema:
			case EntityType.Tablespace:
			case EntityType.Relation:
            case EntityType.Field:
#pragma warning disable CA1308 // Normalize strings to uppercase
                // build different convention connected to ==> Provider
                var physicalName = NamingConvention.ToSnakeCase(name)?.ToLowerInvariant() ?? string.Empty;
#pragma warning restore CA1308
				return name.StartsWith(SpecialEntityPrefix) ^ Provider.IsReservedWord(physicalName) ?
					string.Join(null, StartPhysicalNameDelimiter, physicalName, EndPhysicalNameDelimiter) : physicalName;

		}
		return string.Empty;
	}

	public string GetPhysicalName(Index index, Table table)
	{
		// Code size: 139 (0x8b)
		var result = new StringBuilder(33); 
		switch (table.Type)
		{
			//name:  idx_{table_id}_{index_id}
			case TableType.Business:
				result.Append(DefaultIndexPrefix).Append(table.Id).Append('_')
					.Append(index.Id.ToString("X2", CultureInfo.InvariantCulture));
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
					.Append(index.Id)
					.Append(EndPhysicalNameDelimiter);
				break;
		}
		return result.ToString();
	}
	public string GetPhysicalName(Table table, DbSchema schema)
	{
		// Code size: 212 (0xd4)
		var result = new StringBuilder(63); // schema name max length(30)  + table name max length(30) + 1 '.' + 2 '"'
#pragma warning disable CA1308 // Normalize strings to uppercase
		var tableName = NamingConvention.ToSnakeCase(table.Name)?.ToLowerInvariant();
#pragma warning restore CA1308 
		result.Append(GetPhysicalName(EntityType.Schema, schema.Name))
			.Append(SchemaSeparator);

		switch (table.Type)
		{
			case TableType.Mtm:
				result.Append(StartPhysicalNameDelimiter)
					.Append(MtmPrefix)
					.Append(tableName)
					.Append(EndPhysicalNameDelimiter);
				break;
			case TableType.SchemaCatalog:
			case TableType.TableCatalog:
			case TableType.TableSpaceCatalog:
				result.Append(tableName);
				break; 
			default:
				if (table.Name.StartsWith(SpecialEntityPrefix))
				{
					result.Append(StartPhysicalNameDelimiter)
						.Append(tableName)
						.Append(EndPhysicalNameDelimiter);
				}
				else
				{
					result.Append(TablePrefix)
						.Append(tableName);
				}
				break;
		}
		return result.ToString();
	}

	protected abstract string MtmPrefix { get; }
	protected string GetDataType(IColumn column, bool firstColumn)
	{
		// Code size: 96 (0x60)
		var fielType = column.FieldType;
		if (!firstColumn && fielType == FieldType.LongDateTime) return DataType[FieldType.Short];
		return GetDataType(DataType[fielType], fielType, column.Size, VarcharMaxSize,
			fielType == FieldType.String || fielType == FieldType.LongString ?
			StringCollateInformation : null);
	}
	protected string GetDataType(Relation relation)
	{
		// Code size: 43 (0x2b)
		if (relation.FieldType != FieldType.Undefined) 
			return GetDataType(DataType[relation.FieldType], FieldType.Long, 0, 0);
		return string.Empty;
	}
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
			result.Append(index.Columns[i])
				.Append(',');
		}
		result.Length--;
		result.Append(')');
		if (tablespace != null)
		{
			result.Append(SqlSpace)
				.Append(DdlTableSpace)
				.Append(tablespace.Name);
		}
		return result.ToString();
	}
	public string Create(Constraint constraint, TableSpace? tablespace = null)
	{
        // Code size: 255 (0xff)
        var result = new StringBuilder();
		result.Append(DdlAlter)
					.Append(DdlTable)
					.Append(constraint.ToTable.PhysicalName)
					.Append(SqlSpace)
					.Append(DdlAdd)
					.Append(DdlConstraint)
					.Append(constraint.PhysicalName)
					.Append(SqlSpace);
		switch (constraint.Type)
		{
			 case ConstraintType.PrimaryKey:
				result.Append(DdlPrimaryKey)
					.Append('(')
					.Append(string.Join(',', constraint.ToTable.GetPrimaryKey().ConvertAll(delegate (IColumn column)
					{
						return column.Name;
					})
					.ToArray()))
					.Append(')');
			 break; 
		}
		if (tablespace != null)
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
		// Code size: 211 (0xd3)
		var i = 0;
		var columnCount = table.Columns.Length;
		var result = new StringBuilder();
		result.Append(DdlCreate)
			.Append(DdlTable)
			.Append(table.PhysicalName)
			.Append(SqlSpace)
			.Append('(')
			.Append(SqlLineFeed);
		while (i < columnCount)
		{
			var column = table.Columns[i];
			/*
			if (column.Type == EntityType.Field) Create(result, table, (Field)column);
			else Create(result, table, (Relation)column);
			*/
			++i;
		}
		if (i > 0) result.Length -= 2;
		result.Append(')');
		if (tablespace != null)
		{
			result.Append(SqlSpace)
				.Append(DdlTableSpace)
				.Append(tablespace.PhysicalName);
		}
		return result.ToString();
	}
	public Constraint[] GetConstraints(Table table) 
	{
		var result = new List<Constraint>();
		if (table.HasPrimaryKey()) result.Add(GetPrimaryKey(table));

        return result.ToArray();
	}

    #region private methods 
    private static string GetSizeInfo(int size) => $"({size})";

	private void Create(StringBuilder subResult, Table table, Field field, bool firstColumn = true)
	{
		// Code size: 160 (0xa0)
		subResult.Append(Indent)
			.Append(firstColumn? field.PhysicalName : GetSecondColumn(field))
			.Append(SqlSpace)
			.Append(GetDataType(field, firstColumn));
		if ((field.IsPrimaryKey() || table.Type != TableType.Business) && field.NotNull)
		{
			subResult.Append(SqlSpace).Append(DdlNotNull);
		}
		subResult.Append(',').Append(SqlLineFeed);
		if (firstColumn)
			if (field.IsSearchable() || (field.Type == FieldType.LongDateTime && !string.IsNullOrEmpty(TimeZoneOffsetPrefix)))
			{
				// recursive call 4 searchable fields or longDateTime !!
				Create(subResult, table, field, false);
			}
	}
	private void Create(StringBuilder stringBuilder, Table table, Relation relation)
	{
		stringBuilder.Append(Indent)
			.Append(relation.PhysicalName)
			.Append(SqlSpace)
			.Append(GetDataType(relation));
		if (table.Type != TableType.Business && relation.NotNull)
		{
			stringBuilder.Append(SqlSpace)
				.Append(DdlNotNull);
		}
		stringBuilder.Append(',')
			.Append(SqlLineFeed);
	}

	private static string GetDataType(string dataType, FieldType fieldType, int size, int maxSize, string? collateInformation = null)
	{
        // Code size: 70 (0x46)
        var result = new StringBuilder(dataType);
		if (fieldType == FieldType.String && size > 0 && size <= maxSize)
			result.Append(GetSizeInfo(size));
		if ((fieldType == FieldType.String || fieldType == FieldType.LongString) && collateInformation != null)
			result.Append(SqlSpace).Append(collateInformation);
		return result.ToString();
	}

	private Constraint GetPrimaryKey(Table table)
	{
        // Code size: 28 (0x1c)
        var result =new Constraint(ConstraintType.PrimaryKey, table, string.Empty);
        return new(ConstraintType.PrimaryKey, table, GetPhysicalName(result));
    }

    #endregion

}


