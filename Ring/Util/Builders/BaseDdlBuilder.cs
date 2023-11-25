using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Text;
using Index = Ring.Schema.Models.Index;
using DbSchema = Ring.Schema.Models.Schema;

namespace Ring.Util.Builders;

internal abstract class BaseDdlBuilder : BaseSqlBuilder, IDdlBuilder
{
    // entity
    protected static readonly string DdlView = @"VIEW";
    protected static readonly string DdlTable = @"TABLE ";  // final space character needed !
    protected static readonly string DdlConstraint = @"CONSTRAINT";
    protected static readonly string DdlIndex = @"INDEX";
    protected static readonly string DdlSequence = @"SEQUENCE";
    protected static readonly string DdlTableSpace = @"TABLESPACE ";
    protected static readonly string DdlSchema = @"SCHEMA ";

    // options
    protected static readonly string DdlUnique = @"UNIQUE";
    protected static readonly string DdlBitmap = @"BITMAP";
    protected static readonly string DdlHash = @"HASH";

    // commands
    protected static readonly string DdlReference = @"REFERENCES";
    protected static readonly string DdlCreate = @"CREATE ";
    protected static readonly string DdlAlter = @"ALTER "; // final space character needed !
    protected static readonly string DdlDrop = @"DROP ";
    protected static readonly string DdlAdd = @"ADD ";
    protected static readonly string DdlColumn = @"COLUMN ";
    protected static readonly string DdlTruncate = @"TRUNCATE ";
    protected static readonly string DdlnotNull = @"NOT NULL";

    // format
    protected static readonly string Indent = @"    ";

    // prefixes 
    protected static readonly string DefaultTablePrefix = @"t_";

    // conventions
    protected static readonly string DefaultPhysicalNameSeparator = "\"";
    protected readonly static char SpecialEntityPrefix = '@';

    public string AlterAddColumn(Table table, Field field)
    {
        var result = new StringBuilder();
        result.Append(DdlAlter)
            .Append(DdlTable)
            .Append(table.PhysicalName)
            .Append(SqlSpace)
            .Append(DdlAdd)
            .Append(GetPhysicalName(field))
            .Append(SqlSpace)
            .Append(GetDataType(field));
        return result.ToString();
    }

    public string AlterAddColumn(Table table, Relation relation)
    {
        var result = new StringBuilder();
        if (relation.Type == RelationType.Mto || relation.Type == RelationType.Otop)
        {
            result.Append(DdlAlter)
                .Append(DdlTable)
                .Append(table.PhysicalName)
                .Append(SqlSpace)
                .Append(DdlAdd)
                .Append(GetPhysicalName(relation))
                .Append(SqlSpace)
                .Append(GetDataType(relation));
        }
        return result.ToString();
    }

    public string AlterDropColumn(Table table, Field field)
    {
        var result = new StringBuilder();
        result.Append(DdlAlter)
            .Append(DdlTable)
            .Append(table.PhysicalName)
            .Append(SqlSpace)
            .Append(DdlDrop)
            .Append(DdlColumn)
            .Append(GetPhysicalName(field));
        return result.ToString();
    }

    public string AlterDropColumn(Table table, Relation relation)
    {
        var result = new StringBuilder();
        result.Append(DdlAlter)
            .Append(DdlTable)
            .Append(table.PhysicalName)
            .Append(SqlSpace)
            .Append(DdlDrop)
            .Append(DdlColumn)
            .Append(GetPhysicalName(relation));
        return result.ToString();
    }

    public string Drop(Table table)
    {
        var result = new StringBuilder();
        result.Append(DdlDrop)
            .Append(DdlTable)
            .Append(table.PhysicalName);
        return result.ToString();
    }

    public string Truncate(Table table)
    {
        var result = new StringBuilder();
        result.Append(DdlTruncate)
            .Append(DdlTable)
            .Append(table.PhysicalName);
        return result.ToString();
    }

    protected virtual string GetDataType(string dataType, FieldType fieldType, int size, int maxSize, string? collateInformation = null)
    {
        var result = new StringBuilder(dataType);
        if (fieldType == FieldType.String && size > 0 && size <= maxSize)
            result.Append(GetSizeInfo(size));
        if ((fieldType == FieldType.String || fieldType == FieldType.LongString) && collateInformation != null)
            result.Append(SqlSpace).Append(collateInformation);
        return result.ToString();
    }

    public string Create(Table table, TableSpace? tablespace = null)
    {
        int i;
        var result = new StringBuilder();
        result.Append(DdlCreate);
        result.Append(DdlTable);
        result.Append(table.PhysicalName);
        result.Append(SqlSpace);
        result.Append('(');
        result.Append(SqlLineFeed);
        for (i = 0; i < table.FieldsById.Length; ++i) Create(result, table, table.FieldsById[i]);
        if (i > 0 && table.Relations.Length == 0) result.Length = result.Length - 2;
        for (i = 0; i < table.Relations.Length; ++i) Create(result, table, table.Relations[i]);
        if (i > 0) result.Length = result.Length - 2;
        result.Append(')');
        if (tablespace != null)
        {
            result.Append(SqlSpace);
            result.Append(DdlTableSpace);
            result.Append(GetPhysicalName(tablespace));
        }
        return result.ToString();
    }

    public abstract DatabaseProvider Provider { get; }
    protected abstract string GetDataType(Field field);
    protected abstract string GetDataType(Relation relation);
    protected abstract string GetPhysicalName(TableSpace tablespace);
    public abstract string GetPhysicalName(DbSchema schema);
    public abstract string GetPhysicalName(Table table, DbSchema schema);
    public abstract string GetPhysicalName(Relation relation);
    public abstract string GetPhysicalName(Field field);
    public abstract string Create(TableSpace tablespace);

    private static string GetSizeInfo(int size) => $"({size})";
    private void Create(StringBuilder stringBuilder, Table table, Field field)
    {
        stringBuilder.Append(Indent);
        stringBuilder.Append(GetPhysicalName(field));
        stringBuilder.Append(SqlSpace);
        stringBuilder.Append(GetDataType(field));
        if (field.IsPrimaryKey() || table.Type != TableType.Business && field.NotNull)
        {
            stringBuilder.Append(SqlSpace);
            stringBuilder.Append(DdlnotNull);
        }
        stringBuilder.Append(',');
        stringBuilder.Append(SqlLineFeed);
    }
    private void Create(StringBuilder stringBuilder, Table table, Relation relation)
    {
        stringBuilder.Append(Indent)
            .Append(GetPhysicalName(relation))
            .Append(SqlSpace)
            .Append(GetDataType(relation));
        if (table.Type != TableType.Business && relation.NotNull)
        {
            stringBuilder.Append(SqlSpace)
                .Append(DdlnotNull);
        }
        stringBuilder.Append(',')
            .Append(SqlLineFeed);
    }

    public string Create(Index index, Table table, TableSpace? tablespace = null)
    {
        throw new NotImplementedException();
    }

    public string Create(Constraint constraint, TableSpace? tablespace = null)
    {
        throw new NotImplementedException();
    }

    public string Create(DbSchema schema)
    {
        var result = new StringBuilder();
        result.Append(DdlCreate)
            .Append(DdlSchema)
            .Append(GetPhysicalName(schema));
        return result.ToString();
    }


}


