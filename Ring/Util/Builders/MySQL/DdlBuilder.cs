using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using PostGreDdl = Ring.Util.Builders.PostgreSQL.DdlBuilder;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders.MySQL;

internal class DdlBuilder : BaseDdlBuilder
{
    private const int VarcharMaxSize = 65535;
    private readonly static DatabaseProvider _currentProvider = DatabaseProvider.MySql;
    private readonly static PostGreDdl _postGreDdl = new();
    private readonly static string PhysicalNameSeparator = "`";
    private readonly static char SpecialEntityPrefix = '@';
    private readonly static string StringCollageInformation = @"COLLATE ""C""";
    private readonly static Dictionary<FieldType, string> _dataType = new()
    {
        { FieldType.String,        "VARCHAR"   },
        { FieldType.LongString,    "LONGTEXT"  },
        { FieldType.Double,        "DOUBLE"    },
        { FieldType.Float,         "FLOAT"     },
        { FieldType.Long,          "BIGINT"    },
        { FieldType.Int,           "INT"       },
        { FieldType.Short,         "SMALLINT"  },
        { FieldType.Byte,          "TINYINT"   },
        { FieldType.Boolean,       "BOOLEAN"   },
        { FieldType.ShortDateTime, "DATE"      },
        { FieldType.ByteArray,     "VARBINARY" },
        { FieldType.DateTime,      "TIMESTAMP" },
        { FieldType.LongDateTime,  "TIMESTAMP" }
    };

    public override string Create(TableSpace tablespace) => tablespace.Name;

    public override DatabaseProvider Provider => _currentProvider;

    protected override string GetDataType(Field field) =>
        GetDataType(_dataType[field.Type], field.Type, field.Size, VarcharMaxSize,
            field.Type == FieldType.String || field.Type == FieldType.LongString ?
            StringCollageInformation : null);

    protected override string GetDataType(Relation relation) => 
        _currentProvider.IsReservedWord(relation.Name)  ? 
        string.Join(null, PhysicalNameSeparator, relation.Name, PhysicalNameSeparator) : relation.Name;

    public override string GetPhysicalName(Field field) =>
        _currentProvider.IsReservedWord(field.Name) ^ field.Name.StartsWith(SpecialEntityPrefix)?
        string.Join(null, PhysicalNameSeparator, field.Name, PhysicalNameSeparator) : field.Name;

    public override string GetPhysicalName(Relation relation) =>
        _currentProvider.IsReservedWord(relation.Name)?
        string.Join(null, PhysicalNameSeparator, relation.Name, PhysicalNameSeparator) : relation.Name;

    protected override string GetPhysicalName(TableSpace tablespace) => tablespace.Name;
    protected override string GetPhysicalName(DbSchema schema)
    {
#pragma warning disable CA1308 // Normalize strings to uppercase
        var mySqlPhysicalName = NamingConvention.ToSnakeCase(schema.Name).ToLowerInvariant();
#pragma warning restore CA1308 
        return _currentProvider.IsReservedWord(mySqlPhysicalName) ?
            string.Join(null, PhysicalNameSeparator, mySqlPhysicalName, PhysicalNameSeparator) : mySqlPhysicalName;
    }

    public override string GetPhysicalName(Table table, DbSchema schema) => 
        _postGreDdl.GetPhysicalName(table, schema).Replace(DefaultPhysicalNameSeparator, PhysicalNameSeparator);

}
