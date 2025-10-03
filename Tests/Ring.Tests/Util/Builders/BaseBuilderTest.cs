using Bogus;
using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Builders;
using System.Reflection;
using Index = Ring.Schema.Models.Index;

namespace Ring.Tests.Util.Builders;

public class BaseBuilderTest
{
    private readonly Faker _faker = new();

    internal Table GetAnonymousTable(IDdlBuilder builder, int numberOfField = 0, int numberOfRelation = 0)
    {
        var fields = new List<Field>();
        for (var i = 0; i < numberOfField-1; i++)
            fields.Add(GetAnonymousField(GetAnonymousFieldType(), _faker.Random.Number(int.MinValue,int.MaxValue), i + 10));

        // add pk
        Field pk = FieldExtensions.GetDefaultPrimaryKey(null, FieldType.Short) ?? default!;
        fields.Add(pk);
        var relations = new List<Relation>();
        for (var i = 0; i < numberOfRelation; i++) relations.Add(GetAnonymousRelation(RelationType.Mto, i + 20, "skill2book_"  + i.ToString()));

        var result = new Table(_faker.Random.Number(int.MinValue,int.MaxValue), _faker.Random.String(), _faker.Random.String(), _faker.Random.String(),
            _faker.Random.String(), TableType.Business, relations.ToArray(), fields.ToArray(),
            new Column[fields.Count + relations.Count], Array.Empty<Index>(), 12, 
            PhysicalType.Table, 0, 0, true, true, true, true, true).ToMeta(0);
        var metaTable = GetFirstMeta(result, EntityType.Table);
        var table = metaTable.ToTable(new ArraySegment<Meta>(result), PhysicalType.Table, builder, _faker.Random.String(), 0); // load Columns

        // add relations 
        if (table is not null)
        {
            for (var i = 0; i < numberOfRelation; ++i) table.Relations[i] = relations[i];

            // Array.Sort(fields, (x, y) => 
            fields = fields.OrderBy(o => o.Name, StringComparer.Ordinal).ToList(); // compare ordinal here !! string.CompareOrdinal()
            relations = relations.OrderBy(o => o.Name, StringComparer.Ordinal).ToList();
        }
        return table;
    }

    internal TableSpace GetAnonymousTableSpace(string name) =>
        new (_faker.Random.Number(int.MinValue,int.MaxValue), name, name, _faker.Random.String(), _faker.Random.Bool(), _faker.Random.Bool(),
            _faker.Random.Bool(), _faker.Random.WordsArray(8), _faker.Random.String(), _faker.Random.Bool(), _faker.Random.Bool());
        

    internal Field GetAnonymousField(FieldType fieldType, int size, int? id = null, string? name = null) =>
        new (id ?? _faker.Random.Number(int.MinValue,int.MaxValue), name ?? _faker.Random.String(),
            _faker.Random.String(), fieldType, size, _faker.Random.Bool()?  null : _faker.Random.String(), _faker.PickRandom<SearchableType>(), _faker.Random.Bool(), 
            _faker.Random.Bool(), _faker.Random.Bool(), _faker.Random.Bool(), _faker.Random.Bool());

    internal Relation GetAnonymousRelation(RelationType relationType, int id, string? name = null, bool notNull = true)
    {
        // generate primary key 
        Field primaryKey = FieldExtensions.GetDefaultPrimaryKey(null, FieldType.Long) ?? default!;

        var fieldList = new List<Field>() { primaryKey };
        var relationName = name == null ? _faker.Random.String() : name;
        var toTable = new Table(_faker.Random.Number(int.MinValue,int.MaxValue), _faker.Random.String(), _faker.Random.String(), _faker.Random.String(),
            _faker.Random.String(), TableType.Business, Array.Empty<Relation>(), fieldList.ToArray(),
            new Column[fieldList.Count], Array.Empty<Index>(), 12, PhysicalType.Table, 0, 0,
            true, true, true, true, true);
        // generate primary key 
        var result = new Relation(id, relationName, _faker.Random.String(), relationType, toTable, primaryKey.Type, 
            notNull, _faker.Random.Bool(), _faker.Random.Bool(), _faker.Random.Bool());

        return result;
    }

    internal FieldType GetAnonymousFieldType() => _faker.PickRandomWithout(FieldType.Undefined);

    internal Meta[] GetMeta2TableItems(bool addMtmRelationship)
    {
        var metaList = new List<Meta>
        {
            { GetMeta(2,"name", EntityType.Field,16, 10493996L, true, 1061) },
            { GetMeta(3,"sub_name", EntityType.Field,16, 3932170L, true, 1061) },
            { GetMeta(4,"is_group", EntityType.Field,23, 6L, true, 1061) },
            { GetMeta(5,"category", EntityType.Field,16, 1048578L, true, 1061) },
            { GetMeta(6,"armor_penality", EntityType.Field,3, 6L, true, 1061) },
            { GetMeta(7,"trained_only", EntityType.Field,23, 6L, true, 1061) },
            { GetMeta(9,"try_again", EntityType.Field,23, 6L, true, 1061) },
            { GetMeta(1,"id", EntityType.Field,2, 2L, true, 1061) },
            { GetMeta(11,"skill2book", EntityType.Relation,1021, 2883600L, true, 1061)}
        };
        // 2,0,2,1011,1021,786448,ability2book,,book2ability,true
        if (addMtmRelationship)
        {
            metaList.Add(GetMeta(8, "ability2book", EntityType.Relation, 1021, 786448L, true, 1061));
        }

        return metaList.ToArray();
    }
    internal Meta GetMeta2Table(TableType tableType) =>
        new (1061, (byte)EntityType.Table, _faker.Random.Number(int.MinValue,int.MaxValue), (int)tableType, 8704, 
            "skill", _faker.Random.String(), _faker.Random.String(), true);

    internal static Meta[] GetSchema1()
    {
        var result = new List<Meta>();
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Ring.Tests.Resources.meta.csv";


#pragma warning disable CS8600 
#pragma warning disable CS8604 
        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        using (StreamReader reader = new StreamReader(stream))
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

    private Meta GetMeta(int id, string name, EntityType entityType, int dataType, long flags, bool active, int? referenceId=null)
    {
        return new(id, (byte)entityType, referenceId??_faker.Random.Number(int.MinValue,int.MaxValue), dataType, flags,
            name, _faker.Random.String(), _faker.Random.String(), active);
    }

    private Meta GetFirstMeta(Meta[] metas, EntityType entityType)
    {
        var i = 0;
        while (i < metas.Length)
        {
            var et = metas[i].GetEntityType();
            if (et == entityType) return metas[i];
            ++i; 
        }
        return new Meta("Test");
    }


}
