using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Reflection;
using Index = Ring.Schema.Models.Index;
using Ring.Schema;
using Bogus;
using Ring.Util.Builders;

namespace Ring.Tests;

public abstract class BaseTest
{
    protected readonly Faker _faker = new();
    private readonly ITestOutputHelper _output;

    protected BaseTest(ITestOutputHelper output)
    {
        _output = output;
    }

    protected void LogAssert(string message) => _output.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "| ASSERT  | " + message);

    protected void LogAct(string message) => _output.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|   ACT   | " + message);

    protected void LogArrange(string message) => _output.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "| ARRANGE | " + message);

    internal Table GetAnonymousTable(IDdlBuilder builder, int numberOfField = 0, int numberOfRelation = 0,
        char minChar = char.MinValue, char maxChar = char.MaxValue)
    {
        var tableId = _faker.Random.Number(100, int.MaxValue);
        var tableType = TableType.Business;
        var fields = new List<Meta>();
        var relations = new List<Meta>();

        for (var i = 0; i < numberOfRelation; i++) relations.Add(GetAnonymousRelation(null, minChar, maxChar).ToMeta(tableId));
        for (var i = 0; i < numberOfField; i++) fields.Add(GetAnonymousField(i+1, minChar, maxChar).ToMeta(tableId)); // avoid Id collisions

        // sort lists
        fields = fields.OrderBy(o => o.Name, StringComparer.Ordinal).ToList();
        relations = relations.OrderBy(o => o.Name, StringComparer.Ordinal).ToList();

        // flags
        var flags = 0L;
        flags = Meta.SetEntityBaseline(flags, true);
        flags = Meta.SetTableCached(flags, true);
        flags = Meta.SetTableReadonly(flags, true);

        // Meta(int id, byte objectType, int referenceId, int dataType, long flags, string name, string? description, string? value, bool active)
        var table = new Meta(tableId, (byte)EntityType.Table, 0, (int)tableType, flags, _faker.Random.String(), _faker.Random.String(), null, true);
        fields.AddRange(relations);
        var items = new ArraySegment<Meta>(fields.ToArray());
        var result = table.ToTable(items, PhysicalType.Table, builder, builder.GetPhysicalName(EntityType.Table, table.Name), 0);

        // load relations 
        if (result is not null && numberOfRelation>0)
        {
            // we need a table with at least one field (pk)
            var tableTarget = GetAnonymousTable(builder, 1, 0);
            for (var i = 0; i < numberOfRelation; i++)
#pragma warning disable CS8601 // Possible null reference assignment.
                result.Relations[i] = relations[i].ToRelation(tableTarget);
#pragma warning restore CS8601
        }

        return result;
    }

    internal Field GetAnonymousField(int? id, char minChar = char.MinValue, char maxChar = char.MaxValue) =>
        new(id is null ? _faker.Random.Number(int.MinValue, int.MaxValue): id.Value, _faker.Random.String(null, minChar, maxChar),
            _faker.Random.String(), _faker.PickRandom<FieldType>(), _faker.Random.Number(int.MinValue, int.MaxValue),
            _faker.Random.String(), _faker.Random.String(), _faker.PickRandom<SearchableType>(), _faker.Random.Bool(), _faker.Random.Bool(),
            _faker.Random.Bool(), _faker.Random.Bool(), _faker.Random.Bool());

    internal Column GetAnonymousColumn() =>
        new(_faker.PickRandom<EntityType>(), _faker.PickRandom<FieldType>(), _faker.Random.String(), _faker.PickRandom<SearchableType>(), 
            _faker.Random.Number(int.MinValue, int.MaxValue), _faker.Random.Number(int.MinValue, int.MaxValue));

    internal Relation GetAnonymousRelation(string? name = null, char minChar = char.MinValue, char maxChar = char.MaxValue)
    {
        var toTable = new Table(_faker.Random.Number(int.MinValue, int.MaxValue), _faker.Random.String(null, minChar, maxChar), _faker.Random.String(),
            _faker.Random.String(), _faker.Random.String(), TableType.Business, Array.Empty<Relation>(), Array.Empty<Field>(),
            Array.Empty<Column>(), Array.Empty<Index>(), Array.Empty<Constraint>(), 12, PhysicalType.Table, 0, 0, new CacheId(), true, true, true, true, _faker.Random.Bool(), true, _faker.Random.Bool());
        return new Relation(_faker.Random.Number(100, int.MaxValue), name ?? _faker.Random.String(null, minChar, maxChar), 
            _faker.Random.String(),  _faker.PickRandom<RelationType>(), toTable, FieldType.Long, _faker.Random.Bool(), _faker.Random.Bool(),
            _faker.Random.Bool(), _faker.Random.Bool());
    }
    internal Meta GetMeta1Table(TableType tableType)
    {
        return new Meta(1061, (byte)EntityType.Table, _faker.Random.Number(int.MinValue, int.MaxValue), (int)tableType, 8704, "skill",
            _faker.Random.String(), null, true);
    }

    internal Meta[] GetMeta1TableItems()
    {
        var metaList = new List<Meta>
        {
            { GetMeta(2,"name", EntityType.Field,16, 10493964L, true, 1061) },
            { GetMeta(3,"sub_name", EntityType.Field,16, 3932170L, true, 1061) },
            { GetMeta(4,"is_group", EntityType.Field,23, 6L, true, 1061) },
            { GetMeta(5,"category", EntityType.Field,16, 1048578L, true, 1061) },
            { GetMeta(6,"armor_penality", EntityType.Field,3, 6L, true, 1061) },
            { GetMeta(7,"trained_only", EntityType.Field,23, 6L, true, 1061) },
            { GetMeta(8,"try_again", EntityType.Field,23, 6L, true, 1061) },
            { GetMeta(1,"id", EntityType.Field,2, 2L, true, 1061) },
            { GetMeta(1,"skill2book", EntityType.Relation,1021, 786448L, true, 1061) }
        };
        return metaList.ToArray();
    }

    internal Meta GetMeta2Table() => GetMeta1Table(TableType.Mtm);

    internal Meta[] GetMeta2TableItems()
    {
        // empty field
        var metaList = new List<Meta>
        {
            { GetMeta(1, "skill2book", EntityType.Relation, 1021, 786448L, true, 1061) }
        };
        return metaList.ToArray();
    }

    internal static Meta[] GetSchema1()
    {
        var result = new List<Meta>();
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Ring.Tests.Resources.meta.csv";

#pragma warning disable CS8600 
#pragma warning disable CS8604 
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        using (StreamReader reader = new(stream))
        {
            var metaList = reader.ReadToEnd().Split("\n");
            foreach (var metaLine in metaList)
            {
                var line = metaLine.Split(',');
                if (line.Length < 6) continue;
                Meta meta = new(int.Parse(line[0]), byte.Parse(line[2]), int.Parse(line[3]), int.Parse(line[4]), long.Parse(line[5]), line[6],
                    line[7], string.IsNullOrWhiteSpace(line[8]) ? null : line[8], bool.Parse(line[9]));
                result.Add(meta);
            }
        }
#pragma warning restore CS8604 
#pragma warning restore CS8600 
        return result.ToArray();
    }

    private Meta GetMeta(int id, string name, EntityType entityType, int dataType, long flags, bool active, int? referenceId = null)
        => new(id, (byte)entityType, referenceId ?? _faker.Random.Number(int.MinValue, int.MaxValue), dataType, flags,
            name, _faker.Random.String(), _faker.Random.String(), active);


}
