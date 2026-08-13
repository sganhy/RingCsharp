using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Text;

namespace Ring.Util.Builders.PostgreSQL;

internal sealed class DdlBuilder : BaseDdlBuilder
{
	// BUGS (Claude source 4.6):
	//     1) GetPhysicalName(Constraint): StringBuilder capacity too small; Severity: Medium (Done)
	//     2) _dataType: DateTimeOffset maps to timestamp without time zone — Semantic bug; Severity: Medium (Not a bug)

	private readonly Dictionary<FieldType, string> _dataType = new()
	{
		{ FieldType.String,        "varchar"},
		{ FieldType.LongString,    "text"},
		{ FieldType.Double,        "float8"},
		{ FieldType.Float,         "float4"},
		{ FieldType.Long,          "int8"},
		{ FieldType.Int,           "int4"},
		{ FieldType.Short,         "int2"},
		{ FieldType.Byte,          "int2"},
		{ FieldType.Boolean,       "bool"},
		{ FieldType.Date, "date"},
		{ FieldType.ByteArray,     "bytea"},
		{ FieldType.DateTime,      "timestamp without time zone"},
		{ FieldType.DateTimeOffset,  "timestamp without time zone"}
	};

	public DdlBuilder() : base() { }

	public sealed override DatabaseProvider Provider => DatabaseProvider.PostgreSql;
	protected sealed override string StringCollateInformation => @"COLLATE ""C""";
	protected sealed override string MtmPrefix => TableType.Mtm.GetLogicalName(); // physical name prefix for many-to-many tables
	protected sealed override string TimeZoneOffsetPrefix => "@tz_offset_";
	protected sealed override int VarcharMaxSize => 65535;
	protected sealed override Dictionary<FieldType, string> DataType => _dataType;
	protected sealed override string SchemaSeparator => ".";
	protected sealed override char PhysSpecialEntityPrefix => TableType.NonBusinessTable.GetLogicalName()[0];
	protected sealed override string AlterColumnStatment => "ALTER COLUMN";
	protected sealed override string StartPhysicalNameDelimiter => "\"";
	protected sealed override string EndPhysicalNameDelimiter => StartPhysicalNameDelimiter;
	protected sealed override string TablePrefix => DefaultTablePrefix;
	protected sealed override string SearchableFieldPrefix => "s_";

	public sealed override string GetPhysicalName(EntityType entityType, string name) => base.GetPhysicalName(entityType, name);
	public sealed override string Create(TableSpace tablespace) // Code size: 77 (0x4d)
		=> new StringBuilder()
			.Append(DdlCreate)
			.Append(DdlTableSpace)
			.Append(tablespace.PhysicalName)
			.Append(@" LOCATION ")
			.Append('\'')
			.Append(tablespace.FileName)
			.Append('\'').ToString();
				
	protected override string GetCatalogPhysicalName(TableType tableType) 
	{
		switch (tableType)
		{
			case TableType.TableCatalog: return "tables";
			case TableType.TablespaceCatalog: return "tablespaces";
			case TableType.SchemaCatalog: return "schemas";
		}
		return string.Empty;
	}

	protected override string GetSchemaPhysicalName(TableType tableType)
	{
		return "information_schema";
	}

	protected override string GetPhysicalName(TableType tableType, Field field)
	{
		switch (tableType)
		{
			case TableType.TableCatalog:
				if ("name".Equals(field.Name, StringComparison.Ordinal)) return "table_name";
				else return "table_schema";
			case TableType.TablespaceCatalog:
			case TableType.SchemaCatalog: 
				return field.Name;
		}
		return string.Empty;
	}

	protected override Constraint? HasCheckConstraint(Table table, Column column) 
	{
		// Code size: 72 (0x48)
		if (column.FieldType == FieldType.Byte && column.Type == EntityType.Field)
		{
			var result = new Constraint(ConstraintType.Check, table, GetPhysicalName(ConstraintType.Check, table, column.Id), sbyte.MinValue, sbyte.MaxValue);
			result.Columns.Add(column);
			return result;
		}
		return null;
	}

}
