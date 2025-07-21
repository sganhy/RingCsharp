using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

	public string AlterAddColumn(Table table, Column column) // Code size: 90 (0x5a)
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
	
	public string AlterDropColumn(Table table, Column column) // Code size: 80 (0x50)
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
	
	public virtual string GetPhysicalName(EntityType entityType, string name)
	{
		// Code size: 318 (0x13e)
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
			case EntityType.SearchableColumn:
				return Provider.IsReservedWord(name) ^ name.StartsWith(SpecialEntityPrefix)
				? string.Join(null, StartPhysicalNameDelimiter, SearchableFieldPrefix, name, EndPhysicalNameDelimiter) :
					SearchableFieldPrefix + name;
			case EntityType.TimeZoneColumn:
				{
					var newValue = TimeZoneOffsetPrefix + name;
					return Provider.IsReservedWord(newValue) ^ newValue.StartsWith(SpecialEntityPrefix)
					? string.Join(null, StartPhysicalNameDelimiter, TimeZoneOffsetPrefix, name, EndPhysicalNameDelimiter) :
						TimeZoneOffsetPrefix + name;
				}
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
	protected string GetDataType(Table table, Column column, int? size)
	{
		// Code size: 96 (0x60)
		var fieldType = column.FieldType;
		var maxSize = VarcharMaxSize;
        var result = new StringBuilder(DataType[fieldType]);
		var collateInformation = StringCollateInformation;

        if (fieldType == FieldType.String)
            result.Append(GetSizeInfo(size.HasValue ? size.Value : (table.GetField(column)?.Size ?? 0)));  // performance issue may be with GetField() ?
        if ((fieldType == FieldType.String || fieldType == FieldType.LongString) && collateInformation != null)
            result.Append(SqlSpace).Append(collateInformation);
        return result.ToString();
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
			result.Append(index.Columns[i].PhysicalName)
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
					.Append(string.Join(',', constraint.ToTable.GetPrimaryKey().ConvertAll(delegate (Column column)
					{
						return column.PhysicalName;
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
        // Code size: 166 (0xa6)
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
			if (column.Type == EntityType.Field || column.Type == EntityType.SearchableColumn)
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

    private void Create(StringBuilder subResult, Table table, Column column, Field? field, Relation? relation)
    {
		int? size = null;
		var notNull = string.Empty;
        if (field!=null)
        {
			size = field.Size;
			if ((field.IsPrimaryKey() || table.Type != TableType.Business) && field.NotNull)
                notNull = SqlSpace + DdlNotNull;
        }
        subResult.Append(Indent)
				 .Append(column.PhysicalName)
				 .Append(SqlSpace)
				 .Append(GetDataType(table, column, size))
                 .Append(notNull)
                 .Append(',')
				 .Append(SqlLineFeed);
    }

    private Constraint GetPrimaryKey(Table table)
	{
		// Code size: 28 (0x1c)
		var result =new Constraint(ConstraintType.PrimaryKey, table, string.Empty);
		return new(ConstraintType.PrimaryKey, table, GetPhysicalName(result));
	}

	/// <summary>
	/// Return dictionnary of fields by RecordIndex
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
    /// Return dictionnary of fields by RecordIndex
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

    #endregion

}


