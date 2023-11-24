﻿using AutoFixture;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Reflection;
using Index = Ring.Schema.Models.Index;

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
        fieldsById = fieldsById.OrderBy(o => o.Id).ToList();
        relations = relations.OrderBy(o => o.Name).ToList();

        var result = new Table(_fixture.Create<int>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(),
            _fixture.Create<string>(), TableType.Business, relations.ToArray(), fields.ToArray(), fieldsById.ToArray(), Array.Empty<Index>(), 12,
            PhysicalType.Table, true, true, true, true);
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
            _fixture.Create<string>(), TableType.Business, Array.Empty<Relation>(), Array.Empty<Field>(), Array.Empty<Field>(), 
            Array.Empty<Index>(), 12,
            PhysicalType.Table, true, true, true, true);
        var result = new Relation(_fixture.Create<int>(), name??_fixture.Create<string>(), _fixture.Create<string>(),
            _fixture.Create<RelationType>(), toTable, _fixture.Create<bool>(), _fixture.Create<bool>(),
            _fixture.Create<bool>(), _fixture.Create<bool>()) ;
        return result;
    }

    internal Meta GetMeta1Table()
    {
        var meta = new Meta
        {
            Id = 1061,
            Name = "skill",
            ObjectType = (byte)EntityType.Table,
            DataType = 0,
            Flags = 8704,
            Description = _fixture.Create<string>(),
            Active = true
        };
        return meta;
    }

    internal Meta[] GetMeta1TableItems()
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
            new Meta() { Id = 1, Name = "skill2book", ObjectType = (byte)EntityType.Relation, DataType=1021,Flags=786448 }
        };
        foreach (var meta in metaList) { 
            meta.Description = _fixture.Create<string>();
            meta.Active = true;
            meta.ReferenceId = 1061;
        }

        return metaList.ToArray();
    }

    internal Meta GetMeta2Table() => GetMeta1Table();

    internal Meta[] GetMeta2TableItems()
    {
        // empty field
        var metaList = new List<Meta>
        {
            new Meta() { Id = 1, Name = "skill2book", ObjectType = (byte)EntityType.Relation, DataType=1021,Flags=786448 }
        };
        foreach (var meta in metaList)
        {
            meta.Description = _fixture.Create<string>();
            meta.Active = true;
            meta.ReferenceId = 1061;
        }
        return metaList.ToArray();
    }

    internal Meta[] GetSchema1()
    {
        var result = new List<Meta>();
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Ring.Tests.Resources.meta.csv";


#pragma warning disable CS8600 
#pragma warning disable CS8604 
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        using (var reader = new StreamReader(stream))
        {
            var metaList = reader.ReadToEnd().Split("\n");
            foreach (var metaLine in metaList)
            {
                var line = metaLine.Split(',');
                if (line.Length < 6) continue;
                var meta = new Meta
                {
                    Id = int.Parse(line[0]),
                    ObjectType = byte.Parse(line[2]),
                    ReferenceId = int.Parse(line[3]),
                    DataType = int.Parse(line[4]),
                    Flags = long.Parse(line[5]),
                    Name = line[6],
                    Description = line[7],
                    Value = line[8],
                    Active = bool.Parse(line[9])
                };
                result.Add(meta);
            }
        }
#pragma warning restore CS8604 
#pragma warning restore CS8600 
        return result.ToArray();
    }

}