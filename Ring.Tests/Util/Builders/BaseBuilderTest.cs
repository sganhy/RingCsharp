﻿using AutoFixture;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.ComponentModel.Design.Serialization;
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

        for (var i = 0; i < numberOfRelation; i++) relations.Add(GetAnonymousRelation(RelationType.Mto, "skill2book"));

        // sort lists
        fields = fields.OrderBy(o => o.Name).ToList();
        fieldsById = fieldsById.OrderBy(o => o.Id).ToList();
        relations = relations.OrderBy(o => o.Name).ToList();

        var result = new Table(_fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(),
            _fixture.Create<string>(), TableType.Business, relations.ToArray(), fields.ToArray(), 
            new int[fields.Count+relations.Count], Array.Empty<Index>(), 12, PhysicalType.Table, true, true, true, true);
        result.LoadColumnMapper();
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

    internal Relation GetAnonymousRelation(RelationType relationType, string? name = null, bool notNull = true)
    {
        // generate primary key 
        Field primaryKey = FieldExtensions.GetDefaultPrimaryKey(null, FieldType.Long) ?? default!;

        var fieldList = new List<Field>() { primaryKey };
        var relationName = name == null ? _fixture.Create<string>() : name;
        var toTable = new Table(_fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(),
            _fixture.Create<string>(), TableType.Business, Array.Empty<Relation>(), fieldList.ToArray(), 
            new int[fieldList.Count], Array.Empty<Index>(), 12, PhysicalType.Table, true, true, true, true);
        toTable.LoadColumnMapper();
            
        // generate primary key 
        var result = new Relation(1000, relationName, _fixture.Create<string>(),
            relationType, toTable, notNull, _fixture.Create<bool>(),
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

    internal Meta[] GetMeta2TableItems()
    {
        var metaList = new List<Meta>
        {
            new Meta() { Id = 2, Name = "name", ObjectType = (byte)EntityType.Field, DataType=16,Flags=10493964 },
            new Meta() { Id = 3, Name = "sub_name", ObjectType = (byte)EntityType.Field, DataType=16,Flags=3932170 },
            new Meta() { Id = 4, Name = "is_group", ObjectType = (byte)EntityType.Field, DataType=23,Flags=6 },
            new Meta() { Id = 5, Name = "category", ObjectType = (byte)EntityType.Field, DataType=16,Flags=1048578 },
            new Meta() { Id = 6, Name = "armor_penality", ObjectType = (byte)EntityType.Field, DataType=3,Flags=6 },
            new Meta() { Id = 7, Name = "trained_only", ObjectType = (byte)EntityType.Field, DataType=23,Flags=6 },
            new Meta() { Id = 8, Name = "try_again", ObjectType = (byte)EntityType.Field, DataType=23,Flags=6 },
            new Meta() { Id = 1, Name = "id", ObjectType = (byte)EntityType.Field, DataType=2,Flags=2 },
            new Meta() { Id = 1, Name = "skill2ability", ObjectType = (byte)EntityType.Relation, DataType=1021,Flags=2883600 }
        };
        foreach (var meta in metaList)
        {
            meta.SetEntityDescription(_fixture.Create<string>());
            meta.SetEntityActive(true);
            meta.SetEntityRefId(1061);
        }

        return metaList.ToArray();
    }
    internal Meta GetMeta2Table()
    {
        var meta = new Meta
        {
            DataType = 0,
            Flags = 8704
        };
        meta.SetEntityId(1061);
        meta.SetEntityName("skill");
        meta.SetEntityType(EntityType.Table);
        meta.SetEntityActive(true);
        meta.SetEntityDescription(_fixture.Create<string>());
        return meta;
    }

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
                var meta = new Meta
                {
                    ObjectType = byte.Parse(line[2]),
                    DataType = int.Parse(line[4]),
                    Flags = long.Parse(line[5]),
                    Value = line[8]
                };
                meta.SetEntityRefId(int.Parse(line[3]));
                meta.SetEntityActive(bool.Parse(line[9]));
                meta.SetEntityId(int.Parse(line[0]));
                meta.SetEntityName(line[6]);
                meta.SetEntityDescription(line[7]);
                result.Add(meta);
            }
        }
#pragma warning restore CS8604 
#pragma warning restore CS8600 
        return result.ToArray();
    }

}
