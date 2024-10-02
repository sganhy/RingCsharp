using AutoFixture;
using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Reflection;
using Index = Ring.Schema.Models.Index;

namespace Ring.Tests.Util.Builders;

public class BaseBuilderTest
{
    private readonly IFixture _fixture = new Fixture();


    internal Table GetAnonymousTable(int numberOfField = 0, int numberOfRelation = 0)
    {
        var fields = new List<Field>();
        for (var i = 0; i < numberOfField - 1; i++)
            fields.Add(GetAnonymousField(GetAnonymousFieldType(), _fixture.Create<int>(), i + 10));

        // add pk
        Field pk = FieldExtensions.GetDefaultPrimaryKey(null, FieldType.Short) ?? default!;
        fields.Add(pk);
        var fieldsById = new List<Field>(fields);
        var relations = new List<Relation>();

        for (var i = 0; i < numberOfRelation; i++) relations.Add(GetAnonymousRelation(RelationType.Mto, i+20, "skill2book"));

        // sort lists
        fields = fields.OrderBy(o => o.Name).ToList();
        fieldsById.Sort((t1, t2) => t1.Id.CompareTo(t2.Id));
        relations = relations.OrderBy(o => o.Name).ToList();

        var result = new Table(_fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(),
            _fixture.Create<string>(), TableType.Business, relations.ToArray(), fields.ToArray(), 
            new int[fields.Count+relations.Count], new IColumn[fields.Count + relations.Count], Array.Empty<Index>(), 12, PhysicalType.Table, true, true, true, true);
        result.LoadColumnInformation();
        result.LoadRelationRecordIndex();
        return result;
    }

    internal TableSpace GetAnonymousTableSpace(string name)
    {
        var result = new TableSpace(_fixture.Create<int>(), name, _fixture.Create<string>(), _fixture.Create<bool>(), _fixture.Create<bool>(),
            _fixture.Create<bool>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<bool>(), _fixture.Create<bool>());
        return result;
    }

    internal Field GetAnonymousField(FieldType fieldType, int size, int? id = null, string? name = null)
    {
        var result = new Field(id ?? _fixture.Create<int>(), name ?? _fixture.Create<string>(), _fixture.Create<string>(), fieldType, size,
            _fixture.Create<string?>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(),
            _fixture.Create<bool>(), _fixture.Create<bool>());
        return result;
    }

    internal Relation GetAnonymousRelation(RelationType relationType, int id, string? name = null, bool notNull = true)
    {
        // generate primary key 
        Field primaryKey = FieldExtensions.GetDefaultPrimaryKey(null, FieldType.Long) ?? default!;

        var fieldList = new List<Field>() { primaryKey };
        var relationName = name == null ? _fixture.Create<string>() : name;
        var toTable = new Table(_fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(),
            _fixture.Create<string>(), TableType.Business, Array.Empty<Relation>(), fieldList.ToArray(), 
            new int[fieldList.Count], new IColumn[fieldList.Count], Array.Empty<Index>(), 12, PhysicalType.Table, true, true, true, true);
        toTable.LoadColumnInformation();
        toTable.LoadRelationRecordIndex();
        // generate primary key 
        var result = new Relation(id, relationName, _fixture.Create<string>(), relationType, toTable, -1, notNull, _fixture.Create<bool>(), 
            _fixture.Create<bool>(), _fixture.Create<bool>());

        return result;
    }

    internal FieldType GetAnonymousFieldType()
    {
        // generate primary key 
        var result = FieldType.Undefined;
        while (result == FieldType.Undefined)
        {
            result = _fixture.Create<FieldType>();
        }
        return result;
    }

    internal Meta[] GetMeta2TableItems(bool addMtmRelationship)
    {
        var metaList = new List<Meta>
        {
            { GetMeta(2,"name", EntityType.Field,16, 10493964L, true, 1061) },
            { GetMeta(3,"sub_name", EntityType.Field,16, 3932170L, true, 1061) },
            { GetMeta(4,"is_group", EntityType.Field,23, 6L, true, 1061) },
            { GetMeta(5,"category", EntityType.Field,16, 1048578L, true, 1061) },
            { GetMeta(6,"armor_penality", EntityType.Field,3, 6L, true, 1061) },
            { GetMeta(7,"trained_only", EntityType.Field,23, 6L, true, 1061) },
            { GetMeta(9,"try_again", EntityType.Field,23, 6L, true, 1061) },
            { GetMeta(1,"id", EntityType.Field,2, 2L, true, 1061) },
            { GetMeta(1,"skill2book", EntityType.Relation,1021, 2883600L, true, 1061)}
        };
        // 2,0,2,1011,1021,786448,ability2book,,book2ability,true
        if (addMtmRelationship)
        {
            metaList.Add(GetMeta(8, "ability2book", EntityType.Relation, 1021, 786448L, true, 1061));
        }

        return metaList.ToArray();
    }
    internal Meta GetMeta2Table(TableType tableType) =>
        new (1061, (byte)EntityType.Table, _fixture.Create<int>(), (int)tableType, 8704, "skill", _fixture.Create<string>(), _fixture.Create<string>(), true);

    internal Meta[] GetSchema1()
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
        return new(id, (byte)entityType, referenceId??_fixture.Create<int>(), dataType, flags,
            name, _fixture.Create<string>(), _fixture.Create<string>(), active);
    }

}
