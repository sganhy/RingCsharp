using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Text;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders.PostgreSQL;

internal sealed class DdlBuilder : BaseDdlBuilder
{
    private const char SchemaSeparator = '.';
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

    public override DatabaseProvider Provider => DatabaseProvider.PostgreSql;
    protected override string StringCollateInformation => @"COLLATE ""C""";
    protected override string MtmPrefix => "@mtm_";
    protected override int VarcharMaxSize => 65535;

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
    protected override Dictionary<FieldType, string> DataType => _dataType;
    //TODO move to base class
    public override string GetPhysicalName(DbSchema schema)
    {
#pragma warning disable CA1308 // Normalize strings to uppercase
        var physicalName = NamingConvention.ToSnakeCase(schema.Name).ToLowerInvariant();
#pragma warning restore CA1308
        return schema.Name.StartsWith(SpecialEntityPrefix) || Provider.IsReservedWord(physicalName)?
            string.Join(null, DefaultPhysicalNameSeparator, physicalName, DefaultPhysicalNameSeparator) :
            physicalName;
    }

    protected override string GetPhysicalName(TableSpace tablespace) => tablespace.Name;

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
    
}
