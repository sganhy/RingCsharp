using Bogus;
using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Tests.Data.Extensions;

public abstract class BaseExtensionsTest
{
    private readonly Faker _faker = new();

    internal Table GetTable(TableType tableType, string tableName, string schemaName)
    {
        var meta = new Meta(1061, (byte)EntityType.Table, _faker.Random.Number(), (int)tableType, 8704, tableName, _faker.Random.String(), null, true);
        var metaItems = Array.Empty<Meta>();
        var segment = new ArraySegment<Meta>(metaItems, 0, metaItems.Length);
        var physicalName = $"{schemaName}.{tableName}";
        return meta.ToTable(segment, PhysicalType.Table, physicalName); 
    }

}
