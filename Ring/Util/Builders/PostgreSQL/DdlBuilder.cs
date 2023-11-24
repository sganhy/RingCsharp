using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Text;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders.PostgreSQL;

internal sealed class DdlBuilder : BaseDdlBuilder
{
    private readonly static DatabaseProvider _currentProvider = DatabaseProvider.PostgreSql;
    private readonly static string StringCollageInformation = @"COLLATE ""C""";
    private readonly static string MtmPrefix = "@mtm_";
    private const char SchemaSeparator = '.';
    private readonly static char SpecialEntityPrefix = '@';
    private const int VarcharMaxSize = 65535;

    private readonly static Dictionary<FieldType, string> _dataType = new()
    {
        { FieldType.String,        "varchar" },
        { FieldType.LongString,    "text"    },
        { FieldType.Double,        "float8"  },
        { FieldType.Float,         "float4"  },
        { FieldType.Long,          "int8"    },
        { FieldType.Int,           "int4"    },
        { FieldType.Short,         "int2"    },
        { FieldType.Byte,          "int2"    },
        { FieldType.Boolean,       "bool"    },
        { FieldType.ShortDateTime, "date"    },
        { FieldType.ByteArray,     "bytea"   },
        { FieldType.DateTime,      "timestamp without time zone" },
        { FieldType.LongDateTime,  "timestamp with time zone" }
    };

    public override string Create(TableSpace tablespace)
    {
        var result = new StringBuilder();
        result.Append(DdlCreate)
            .Append(DdlTableSpace)
            .Append(GetPhysicalName(tablespace))
            .Append(@" LOCATION ")
            .Append('\'')
            .Append(tablespace.FileName)
            .Append('\'');
        return result.ToString();
    }

    protected override string GetDataType(Field field) =>
        GetDataType(_dataType[field.Type], field.Type, field.Size, VarcharMaxSize,
            field.Type == FieldType.String || field.Type == FieldType.LongString ?
            StringCollageInformation : null);

    protected override string GetDataType(Relation relation)
    {
        var pk = relation.ToTable.GetPrimaryKey();
        if (pk != null) return GetDataType(_dataType[pk.Type], FieldType.Long, 0, 0);
        return string.Empty;
    }

    public override string GetPhysicalName(Field field) =>
        _currentProvider.IsReservedWord(field.Name) ^ field.Name.StartsWith(SpecialEntityPrefix) ?
        string.Join(null, DefaultPhysicalNameSeparator, field.Name, DefaultPhysicalNameSeparator) : field.Name;

    public override string GetPhysicalName(Relation relation) =>
        _currentProvider.IsReservedWord(relation.Name) ?
        string.Join(null, DefaultPhysicalNameSeparator, relation.Name, DefaultPhysicalNameSeparator) : relation.Name;

    protected override string GetPhysicalName(TableSpace tablespace) => tablespace.Name;

    protected override string GetPhysicalName(DbSchema schema)
    {
#pragma warning disable CA1308 // Normalize strings to uppercase
        var physicalName = NamingConvention.ToSnakeCase(schema.Name).ToLowerInvariant();
#pragma warning restore CA1308 
        return _currentProvider.IsReservedWord(physicalName) ?
            string.Join(null, DefaultPhysicalNameSeparator, physicalName, DefaultPhysicalNameSeparator) :
            physicalName;
    }

    public override string GetPhysicalName(Table table, DbSchema schema)
    {
        var result = new StringBuilder(63); // schema name max length(30)  + table name max length(30) + 1 '.' + 2 '"'
#pragma warning disable CA1308 // Normalize strings to uppercase
        var tableName = NamingConvention.ToSnakeCase(table.Name).ToLowerInvariant();
#pragma warning restore CA1308 
        result.Append(GetPhysicalName(schema));
        result.Append(SchemaSeparator);

        switch (table.Type)
        {
            case TableType.Mtm:
                result.Append(DefaultPhysicalNameSeparator);
                result.Append(MtmPrefix);
                result.Append(tableName);
                result.Append(DefaultPhysicalNameSeparator);
                break;
            default:
                if (table.Name.StartsWith(SpecialEntityPrefix))
                {
                    result.Append(DefaultPhysicalNameSeparator);
                    result.Append(tableName);
                    result.Append(DefaultPhysicalNameSeparator);
                }
                else
                {
                    result.Append(DefaultTablePrefix);
                    result.Append(tableName);
                }
                break;
        }
        return result.ToString();
    }

    public override DatabaseProvider Provider => _currentProvider;

}
