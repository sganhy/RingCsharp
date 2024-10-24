using AutoFixture;
using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Tests.Data.Extensions;

public abstract class BaseExtensionsTest
{
    private readonly IFixture _fixture = new Fixture();

    internal Table GetTable(TableType tableType, string tableName, string schemaName)
    {
        var meta = new Meta(1061, (byte)EntityType.Table, _fixture.Create<int>(), (int)tableType, 8704, tableName,
            _fixture.Create<string>(), null, true);
        var metaItems = Array.Empty<Meta>();
        var segment = new ArraySegment<Meta>(metaItems, 0, metaItems.Length);
        var physicalName = $"{schemaName}.{tableName}";
        return meta.ToTable(segment, PhysicalType.Table, physicalName); 
    }

}
