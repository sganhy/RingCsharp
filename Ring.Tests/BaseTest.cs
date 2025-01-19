using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Reflection;
using Index = Ring.Schema.Models.Index;
using Ring.Schema;
using Bogus;
using Xunit.Abstractions;
using Microsoft.VisualStudio.TestPlatform.Utilities;

namespace Ring.Tests;

public abstract class BaseTest
{
    protected readonly Faker _faker = new();
    private readonly ITestOutputHelper _output;

    public BaseTest(ITestOutputHelper output)
    {
        _output = output;
    }

    protected void LogAssert(string message) =>
        _output.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "| ASSERT  | " + message);

    protected void LogAct(string message) =>
        _output.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "|   ACT   | " + message);

    protected void LogArrange(string message) =>
        _output.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "| ARRANGE | " + message);

    internal Table GetAnonymousTable(int numberOfField = 0, int numberOfRelation = 0,
        char minChar = char.MinValue, char maxChar = char.MaxValue)
    {
        var fields = new List<Field>();
        for (var i = 0; i < numberOfField; i++) fields.Add(GetAnonymousField(minChar, maxChar));
        var fieldsById = new List<Field>(fields);
        var relations = new List<Relation>();
        for (var i = 0; i < numberOfRelation; i++) relations.Add(GetAnonymousRelation(null, minChar, maxChar));

        // sort lists
        fields = fields.OrderBy(o => o.Name, StringComparer.Ordinal).ToList();
        fieldsById.Sort((t1, t2) => t1.Id.CompareTo(t2.Id));

        relations = relations.OrderBy(o => o.Name, StringComparer.Ordinal).ToList();
        var result = new Table(_faker.Random.Number(100, int.MaxValue), _faker.Random.String(), _faker.Random.String(), _faker.Random.String(),
            _faker.Random.String(), TableType.Business, relations.ToArray(), fields.ToArray(),
            new int[fields.Count + relations.Count], new IColumn[fields.Count + relations.Count], Array.Empty<Index>(), 12,
            PhysicalType.Table, 0, 0, true, true, true, true);
        result.LoadColumnMapper();
        result.LoadRelationRecordIndex();
        return result;
    }

    internal Field GetAnonymousField(char minChar = char.MinValue, char maxChar = char.MaxValue) =>
        new(_faker.Random.Number(int.MinValue, int.MaxValue), _faker.Random.String(null, minChar, maxChar),
            _faker.Random.String(), _faker.Random.String(), _faker.PickRandom<FieldType>(), _faker.Random.Number(int.MinValue, int.MaxValue),
            _faker.Random.String(), _faker.PickRandom<SearchableType>(), _faker.Random.Bool(), _faker.Random.Bool(),
            _faker.Random.Bool(), _faker.Random.Bool());

    internal Relation GetAnonymousRelation(string? name = null, char minChar = char.MinValue, char maxChar = char.MaxValue)
    {
        var toTable = new Table(_faker.Random.Number(int.MinValue, int.MaxValue), _faker.Random.String(null, minChar, maxChar), _faker.Random.String(),
            _faker.Random.String(), _faker.Random.String(), TableType.Business, Array.Empty<Relation>(), Array.Empty<Field>(), Array.Empty<int>(),
            Array.Empty<IColumn>(), Array.Empty<Index>(), 12, PhysicalType.Table, 0, 0, true, true, true, true);
        return new Relation(_faker.Random.Number(100, int.MaxValue), name ?? _faker.Random.String(null, minChar, maxChar), name ?? _faker.Random.String(null, minChar, maxChar),
            _faker.Random.String(),  _faker.PickRandom<RelationType>(), toTable, -1, FieldType.Long, _faker.Random.Bool(), _faker.Random.Bool(),
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
                    line[7], line[8], bool.Parse(line[9]));
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
