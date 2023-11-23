using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Text;
using DbSchema = Ring.Schema.Models.Schema;
using Ring.Util;
using System.Globalization;

namespace Ring.Sql.PostgreSQL;

internal sealed class DdlBuilder : BaseDdlBuilder
{
    private readonly static DatabaseProvider _currentProvider = DatabaseProvider.PostgreSql;
    private readonly static string StringCollageInformation = @"COLLATE ""C""";
    private readonly static string PhysicalNameSeparator = "\"";
    private readonly static string MtmPrefix = "@mtm_";
    private readonly static string SchemaSeparator = ".";
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
        if (pk!=null)  return GetDataType(_dataType[pk.Type], FieldType.Long, 0, 0);
        return string.Empty;
    }

    public override string GetPhysicalName(Field field) =>
        _currentProvider.IsReservedWord(field.Name) ^ field.Name.StartsWith(SpecialEntityPrefix) ? 
        string.Join(null, PhysicalNameSeparator, field.Name, PhysicalNameSeparator) : field.Name;

    public override string GetPhysicalName(Relation relation) =>
        _currentProvider.IsReservedWord(relation.Name) ? 
        string.Join(null, PhysicalNameSeparator, relation.Name, PhysicalNameSeparator) : relation.Name;

    protected override string GetPhysicalName(TableSpace tablespace) => tablespace.Name;

    protected override string GetPhysicalName(DbSchema schema)
    {   
        var physicalName = NamingConvention.ToSnakeCase(schema.Name).ToLower(CultureInfo.InvariantCulture);
        return _currentProvider.IsReservedWord(physicalName) ? 
            string.Join(null, PhysicalNameSeparator, physicalName, PhysicalNameSeparator) : 
            physicalName;
    }

    public override string GetPhysicalName(Table table, DbSchema schema)
    {
        var result = new StringBuilder(63);
        result.Append(GetPhysicalName(schema));
        result.Append(SchemaSeparator);
        
        switch (table.Type)
        {
            case TableType.Mtm:
                result.Append(PhysicalNameSeparator);
                result.Append(MtmPrefix);
                result.Append(table.Name);
                result.Append(PhysicalNameSeparator);
                break; 
            default:
                if (table.Name.StartsWith(SpecialEntityPrefix))
                {
                    result.Append(PhysicalNameSeparator);
                    result.Append(table.Name);
                    result.Append(PhysicalNameSeparator);
                }
                else {
                    result.Append(DefaultTablePrefix.ToLower(CultureInfo.InvariantCulture));
                    result.Append(table.Name);
                }
                break;
        }
        return result.ToString();
    }

    public override DatabaseProvider Provider => _currentProvider;

}
