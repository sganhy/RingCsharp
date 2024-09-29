using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Tests.Schema.Extensions;

public class TableExtensionsTest : BaseExtensionsTest
{
    [Fact]
    internal void GetField_AnonymousTable_FieldObject()
    {
        // arrange 
        var table = GetAnonymousTable(1000, 1000);
        var fields = table.Fields.OrderByDescending(x => x.Name);

        foreach (var field in fields)
        {
            // act 
            var result = TableExtensions.GetField(table, field.Name);

            // assert
            Assert.NotNull(result);
            Assert.Equal(result.Id, field.Id);
            Assert.Equal(result.Name, field.Name);
        }
    }

    [Fact]
    internal void GetFieldI_AnonymousTable_FieldObject()
    {
        // arrange 
        var table = GetAnonymousTable(20, 20);
        var fields = table.Fields.OrderByDescending(x => x.Name);

        foreach (var field in fields)
        {
            // act 
            var result = TableExtensions.GetField(table, field.Name.ToUpper(), StringComparison.CurrentCultureIgnoreCase);

            // assert
            Assert.NotNull(result);
            Assert.Equal(result.Id, field.Id);
            Assert.Equal(result.Name, field.Name);
        }

    }

    [Fact]
    internal void GetFieldI_OrdinalAnonymousTable_FieldObject()
    {
        // arrange 
        var table = GetAnonymousTable(25, 2);
        var fields = table.Fields.OrderByDescending(x => x.Name);

        foreach (var field in fields)
        {
            // act 
            var result = TableExtensions.GetField(table, field.Name, StringComparison.Ordinal);

            // assert
            Assert.NotNull(result);
            Assert.Equal(result.Id, field.Id);
            Assert.Equal(result.Name, field.Name);
        }
    }

    [Fact]
    internal void GetFieldI_AnonymousTable_Null()
    {
        // arrange 
        var table = GetAnonymousTable(0, 2);

        // act 
        var result = TableExtensions.GetField(table, "Test", StringComparison.CurrentCulture);

        // assert
        Assert.Null(result);
    }

    [Fact]
    internal void GetField_Id_FieldObject()
    {
        // arrange 
        var table = GetAnonymousTable(40, 10);
        var fields = table.Fields.OrderByDescending(x => x.Id);

        foreach (var field in fields)
        {
            // act 
            var result = TableExtensions.GetField(table, field.Id);

            // assert
            Assert.NotNull(result);
            Assert.Equal(result.Name, field.Name);
            Assert.Equal(result.Id, field.Id);
        }
    }

    [Fact]
    internal void GetFieldIndex_AnonymousTable_FieldObject()
    {
        // arrange 
        var table = GetAnonymousTable(200, 200);
        var expectedIndex = 88;
        var field = table.Fields[expectedIndex];

        // act 
        var result = TableExtensions.GetFieldIndex(table, field.Name);

        // assert
        Assert.Equal(expectedIndex, result);
    }


    [Fact]
    internal void GetRelation_AnonymousTable_RelationObject()
    {
        // arrange 
        var table = GetAnonymousTable(2, 40);
        var relations = table.Relations.OrderByDescending(x => x.Name);

        foreach (var relation in relations)
        {
            // act 
            var result = TableExtensions.GetRelation(table, relation.Name);

            // assert
            Assert.NotNull(result);
            Assert.Equal(result.Name, relation.Name);
            Assert.Equal(result.Id, relation.Id);
        }
    }

    [Fact]
    internal void GetRelationI_AnonymousTable_RelationObject()
    {
        // arrange 
        var table = GetAnonymousTable(2, 15);
        var relations = table.Relations.OrderByDescending(x => x.Name);

        foreach (var relation in relations)
        {
            // act 
            var result = TableExtensions.GetRelation(table, relation.Name.ToUpper(), StringComparison.OrdinalIgnoreCase);

            // assert
            Assert.NotNull(result);
            Assert.Equal(result.Name, relation.Name);
            Assert.Equal(result.Id, relation.Id);
        }
    }

    [Fact]
    internal void GetRelationI_AnonymousTable_Null()
    {
        // arrange 
        var table = GetAnonymousTable(2, 0);

        // act 
        var result = TableExtensions.GetRelation(table, "test", StringComparison.CurrentCultureIgnoreCase);
        
        // assert
        Assert.Null(result);
    }

    [Fact]
    internal void GetRelation_Id_FieldObject()
    {
        // arrange 
        var table = GetAnonymousTable(2, 15);
        var relations = table.Relations.OrderByDescending(x => x.Name);

        foreach (var relation in relations)
        {
            // act 
            var result = TableExtensions.GetRelation(table, relation.Id);

            // assert
            Assert.NotNull(result);
            Assert.Equal(result.Name, relation.Name);
            Assert.Equal(result.Id, relation.Id);
        }
    }

    [Fact]
    internal void GetRelationIndex_AnonymousRel_Index()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("book");

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);
        var index1 = table.GetRelationIndex("book2alignment");
        var index3 = table.GetRelationIndex("book2class");
        var index5 = table.GetRelationIndex("book2feat");
        var index7 = table.GetRelationIndex("book2rule");
        var index9 = table.GetRelationIndex("book2weapon");
        var indexNotFound = table.GetRelationIndex("Zorba");


        // assert
        Assert.Equal(1, index1);
        Assert.Equal(3, index3);
        Assert.Equal(5, index5);
        Assert.Equal(7, index7);
        Assert.Equal(9, index9);
        Assert.Equal(-1, indexNotFound);
    }

    [Fact]
    internal void GetField_Id_Null()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("book");

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);
        var field = table.GetField(int.MaxValue);

        // assert
        Assert.Null(field);
    }

    [Fact]
    internal void GetFieldIndex_Name_NotFound()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("domain");

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);
        var index = table.GetFieldIndex("Zorba??");

        // assert
        Assert.Equal(-1, index);
    }

    [Fact]
    internal void GetRelation_Name_Null()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("deity");

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);
        var relation = table.GetRelation("Zorba??");

        // assert
        Assert.Null(relation);
    }


    [Fact]
    internal void GetIndex_Base_IndexObject()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("class");

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);
        var index1 = table.GetIndex("base");

        // assert
        Assert.NotNull(index1);
        Assert.Equal("base", index1.Name);
    }

    [Fact]
    internal void GetIndex_Test1_Null()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("deity");

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);
        var index = table.GetIndex("test1");

        // assert
        Assert.Null(index);
    }


}
