using AutoFixture;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Extensions;
using System.ComponentModel.DataAnnotations;
using System;
using System.Reflection;
using System.Xml.Linq;
using Index = Ring.Schema.Models.Index;
using Ring.Schema;
using Fare;
using System.Threading.Tasks;

namespace Ring.Tests.Schema.Extensions;

public abstract class BaseExtensionsTest
{
    private readonly IFixture _fixture = new Fixture();

    internal Table GetAnonymousTable(int numberOfField = 0, int numberOfRelation = 0)
    {
        var fields = new List<Field>();
        for (var i = 0; i < numberOfField; i++) fields.Add(GetAnonymousField());
        var fieldsById = new List<Field>(fields);
        var relations = new List<Relation>();
        for (var i = 0; i < numberOfRelation; i++) relations.Add(GetAnonymousRelation());

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

    internal Field GetAnonymousField()
    {
        var result = new Field(_fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<FieldType>(),
            _fixture.Create<int>(), _fixture.Create<string?>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(),
            _fixture.Create<bool>(), _fixture.Create<bool>());
        return result;
    }

    internal Field GetAnonymousField(FieldType fieldType, int size)
    {
        var result = new Field(_fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(), fieldType, size, 
            _fixture.Create<string?>(), _fixture.Create<bool>(), _fixture.Create<bool>(), _fixture.Create<bool>(),
            _fixture.Create<bool>(), _fixture.Create<bool>());
        return result;
    }

    internal Relation GetAnonymousRelation(string? name=null)
    {
        var toTable = new Table(_fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(),
            _fixture.Create<string>(), TableType.Business, Array.Empty<Relation>(), Array.Empty<Field>(), Array.Empty<int>(), Array.Empty<IColumn>(),
            Array.Empty<Index>(), 12, PhysicalType.Table, true, true, true, true);
        var result = new Relation(_fixture.Create<int>(), name??_fixture.Create<string>(), _fixture.Create<string>(),
            _fixture.Create<RelationType>(), toTable, -1, _fixture.Create<bool>(), _fixture.Create<bool>(),
            _fixture.Create<bool>(), _fixture.Create<bool>()) ;
        return result;
    }
    internal Meta GetMeta1Table(TableType tableType)
    {
        return new Meta(1061, (byte)EntityType.Table, _fixture.Create<int>(), (int)tableType, 8704, "skill", 
            _fixture.Create<string>(), null, true);
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

    internal Meta[] GetSchema1()
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
    {
        return new(id, (byte)entityType, referenceId ?? _fixture.Create<int>(), dataType, flags,
            name, _fixture.Create<string>(), _fixture.Create<string>(), active);
    }

}
