using Bogus;
using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using System.Linq.Expressions;
using Xunit.Abstractions;

namespace Ring.Tests.Schema.Extensions;

public class RelationExtensionsTest : BaseTest
{
    public RelationExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

    [Fact]
    public void ToMeta_Relation1_MetaObject()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("rule");
        var relation1 = table?.GetRelation("rule2book");

        // act 
        Assert.NotNull(relation1);
        Assert.NotNull(table);
        var meta = RelationExtensions.ToMeta(relation1, table.Id);

        // assert
        Assert.Equal(1, meta.Id);
        Assert.Equal(EntityType.Relation, meta.GetEntityType());
        Assert.Equal(1054, meta.ReferenceId);
        Assert.Equal(1021, meta.DataType);
        Assert.Equal("test=test", meta.Description);
        Assert.Equal(524304, meta.Flags);
        Assert.Equal("rule2book", meta.Name);
        Assert.Equal("book2rule", meta.Value);
        Assert.True(meta.Active);
    }

    [Fact]
    public void ToMeta_Relation2_MetaObject()
    {
        // arrange
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("alignment_descriptor");
        var relation2 = table?.GetRelation("align_descriptor2alignment");

        // act - test an mtm relationship
        Assert.NotNull(relation2);
        Assert.NotNull(table);
        var meta = RelationExtensions.ToMeta(relation2, table.Id);

        // assert
        Assert.Equal(1, meta.Id);
        Assert.Equal(EntityType.Relation, meta.GetEntityType());
        Assert.Equal(1013, meta.ReferenceId);
        Assert.Equal(1012, meta.DataType);
        Assert.Equal(786448, meta.Flags);
        Assert.Equal("align_descriptor2alignment", meta.Name);
        Assert.Equal("align2alignment_descriptor", meta.Value);
        Assert.True(meta.Active);
    }

    [Fact]
    public void GetMtmName_Relation1_MtmName()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("ability");
        var relation1 = table?.GetRelation("ability2book");
        const string expectedValue = "01011_01021_002";

        // act 
        Assert.NotNull(relation1);
        var result = RelationExtensions.GetMtmName(relation1);

        // assert
        Assert.NotNull(schema);
        Assert.NotNull(table);
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void GetMtmName_Relation2_MtmName()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("book");
        var relation2 = table?.GetRelation("book2ability");
        var expectedValue = "01011_01021_002";

        // act 
        Assert.NotNull(relation2);
        var result = RelationExtensions.GetMtmName(relation2);

        // assert
        Assert.NotNull(schema);
        Assert.NotNull(table);
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void GetMtmName_Relation3_MtmName()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("skill");
        var relation3 = table?.GetRelation("synergy2skill");
        var expectedValue = "01061_01061_003";

        // act 
        Assert.NotNull(relation3);
        var result = RelationExtensions.GetMtmName(relation3);

        // assert
        Assert.NotNull(schema);
        Assert.NotNull(table);
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void GetMtmName_Relation4_MtmName()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("skill");
        var relation3 = table?.GetRelation("skill2synergy");
        var expectedValue = "01061_01061_003";

        // act 
        Assert.NotNull(relation3);
        var result = RelationExtensions.GetMtmName(relation3);

        // assert
        Assert.NotNull(schema);
        Assert.NotNull(table);
        Assert.Equal(expectedValue, result);
    }

    [Fact]
    internal void SetInverseRelation_Meta_RelationObject()
    {
        // arrange 
        var meta1 = new Meta(_faker.Random.String());
        var meta2 = new Meta(_faker.Random.String());
        var meta3 = new Meta(_faker.Random.String());
        var relation1 = Meta.GetDefaultRelation(meta1, RelationType.Otm, TableType.Fake);
        var relation2 = Meta.GetDefaultRelation(meta2, RelationType.Mtm, TableType.Fake);
        var relation3 = Meta.GetDefaultRelation(meta3, RelationType.Mto, TableType.Fake);

        // act 
        relation1.SetInverseRelation(relation2); // we can only assign once
        relation1.SetInverseRelation(relation3);

        // assert
        Assert.True(ReferenceEquals(relation2, relation1.InverseRelation));
    }


    [Fact]
    internal void Initialized_Relation1_True()
    {
        // arrange 
        var meta1 = new Meta(_faker.Random.String());
        var meta2 = new Meta(_faker.Random.String());
        var relation1 = Meta.GetDefaultRelation(meta1, RelationType.Otm, TableType.Fake);
        var relation2 = Meta.GetDefaultRelation(meta2, RelationType.Mtm, TableType.Fake);

        // act 
        relation1.SetInverseRelation(relation2); // we can only assign once

        // assert
        Assert.True(relation1.Initialized());
    }

    [Fact]
    internal void Initialized_Relation2_False()
    {
        // arrange 
        var meta2 = new Meta(_faker.Random.String());
        var meta3 = new Meta(_faker.Random.String());
        var relation2 = Meta.GetDefaultRelation(meta2, RelationType.Mtm, TableType.Fake);
        var relation3 = Meta.GetDefaultRelation(meta3, RelationType.Mtm, TableType.Fake);

        // act 
        relation2.SetInverseRelation(relation3); // we can only assign once

        // assert
        Assert.False(relation2.Initialized());
    }

    [Fact]
    internal void Initialized_Relation3_True()
    {
        // arrange 
        var meta3 = new Meta(_faker.Random.String());
        var meta4 = new Meta(_faker.Random.String());
        var relation3 = Meta.GetDefaultRelation(meta3, RelationType.Mtm, TableType.Mtm);
        var relation4 = Meta.GetDefaultRelation(meta4, RelationType.Mtm, TableType.Fake);

        // act 
        relation3.SetInverseRelation(relation4); // we can only assign once

        // assert
        Assert.True(relation3.Initialized());
    }

    [Fact]
    internal void Initialized_Relation4_False()
    {
        // arrange 
        var meta4 = new Meta(_faker.Random.String());
        var relation4 = Meta.GetDefaultRelation(meta4, RelationType.Mtm, TableType.Fake);
        // act 
        // assert
        Assert.False(relation4.Initialized());
    }


    [Fact]
    internal void Initialized_Schema1_AllTrue()
    {
        // arrange - all relations should be initialized
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);

        // act 
        Assert.NotNull(schema);
        foreach (var table in schema.TablesByName)
        {
            foreach (var relation in table.Relations)
            {
                // assert
                Assert.True(relation.Initialized());
            }
        }
    }

    [Fact]
    internal void GetHashCode_RelationHashEqual_True()
    {
        // arrange 
        var metaName = _faker.Random.String();
        var meta1 = new Meta(metaName);
        var meta2 = new Meta(metaName);
        var relation1 = Meta.GetDefaultRelation(meta1, RelationType.Mtm, TableType.Fake);
        var relation2 = Meta.GetDefaultRelation(meta2, RelationType.Mtm, TableType.Fake);

        // act 
        var hash1 = RelationExtensions.Hash(relation1);
        var hash2 = RelationExtensions.Hash(relation2);

        // assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    internal void GetHashCode_RelationHashEqual_False()
    {
        // arrange -- TODO faile sometimes , to be investigated!!! 3 times already
        var metaName = _faker.Random.String(15,'A','z');
        var meta1 = new Meta(metaName);
        var meta2 = new Meta(metaName);
        var meta3 = new Meta(metaName + metaName);
        
        var relation1 = Meta.GetDefaultRelation(meta1, RelationType.Mto, TableType.Fake);
        LogAssert($"relation1 = {relation1.Name} - {relation1.Type}");
        var relation2 = Meta.GetDefaultRelation(meta2, RelationType.Mtm, TableType.Fake);
        LogAssert($"relation2 = {relation2.Name} - {relation2.Type}");
        var relation3 = Meta.GetDefaultRelation(meta3, RelationType.Mtm, TableType.Fake);
        LogAssert($"relation3 = {relation3.Name} - {relation3.Type}");

        // act 
        var hash1 = RelationExtensions.Hash(relation1);
        LogAssert($"hash1 = {hash1}");
        var hash2 = relation2.GetHashCode();
        LogAssert($"hash2 = {hash2}");
        var hash3 = RelationExtensions.Hash(relation3);
        LogAssert($"hash3 = {hash3}");

        // assert
        Assert.NotEqual(hash1, hash2);
        LogAssert($"{hash1} != {hash2}");
        Assert.NotEqual(hash1, hash3);
        LogAssert($"{hash1} != {hash3}");
        Assert.NotEqual(hash2, hash3);
        LogAssert($"{hash2} != {hash3}");
    }


    [Fact]
    internal void Equals_2AnonymousRelations_False()
    {
        // arrange 
        var metaName = _faker.Random.String(15, 'A', 'z');
        var meta1 = new Meta(metaName);
        var meta2 = new Meta(metaName + metaName);
        var meta3 = new Meta(metaName );
        var meta4 = new Meta(metaName);

        var relation1 = Meta.GetDefaultRelation(meta1, RelationType.Mto, TableType.Fake);
        LogAssert($"relation1 = {relation1.Name} - {relation1.Type}");
        var relation2 = Meta.GetDefaultRelation(meta2, RelationType.Mtm, TableType.Fake);
        LogAssert($"relation2 = {relation2.Name} - {relation2.Type}");
        var relation3 = Meta.GetDefaultRelation(meta3, RelationType.Mtm, TableType.Fake);
        LogAssert($"relation3 = {relation3.Name} - {relation3.Type}");
        var relation4 = Meta.GetDefaultRelation(meta3, RelationType.Mto, TableType.Fake);
        LogAssert($"relation4 = {relation4.Name} - {relation4.Type}");


        // act 
        var result1 = relation1 == relation3;
        var result2 = relation3.Equals(relation1);
        var result3 = relation1.Equals((object)relation2);
        var result4 = relation4 != relation1;
        var result5 = relation1.Equals(meta4);

        // assert
        Assert.False(result1);
        Assert.False(result2);
        Assert.False(result3);
        Assert.False(result4);
        Assert.False(result5);
    }
}
