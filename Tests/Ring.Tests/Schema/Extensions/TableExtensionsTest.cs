using Ring.Data;
using Ring.Schema;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Util.Builders;
using System.Globalization;
using System.Linq.Expressions;
using PostGDdlBuilder = Ring.Util.Builders.PostgreSQL.DdlBuilder;

namespace Ring.Tests.Schema.Extensions;

public class TableExtensionsTest : BaseTest
{
    private readonly IDdlBuilder _builder = new PostGDdlBuilder();

    public TableExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();

    [Fact]
    internal void GetField_AnonymousTable_FieldObject()
    {
        // arrange 
        var table = GetAnonymousTable(_builder, 160, 160);

        foreach (var field in table.Fields.OrderByDescending(x => x.Name))
        {
            // act 
            var result = TableExtensions.GetField(table, field.Name);

            // assert
            Assert.NotNull(result);
            Assert.Equal(result.Id, field.Id);
            Assert.Equal(result.Name, field.Name);
        }
    }

    /// <summary>
    /// CRASH SOMETIMES !
    /// </summary>
    [Fact]
    internal void GetFieldI_AnonymousTable_FieldObject()
    {
        // arrange 
        var table = GetAnonymousTable(_builder, 64, 10, 'А', 'Я'); // test on cyrilic alphabet

        // test not working on specific special character!
        foreach (var field in table.Fields.OrderByDescending(x => x.Name))
        {
            // act 
            var result = TableExtensions.GetField(table, field.Name.ToLower(CultureInfo.InvariantCulture),
                StringComparison.InvariantCultureIgnoreCase);

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
        var table = GetAnonymousTable(_builder, 25, 2);
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
        var table = GetAnonymousTable(_builder, 0, 2);

        // act 
        var result = TableExtensions.GetField(table, "Test", StringComparison.CurrentCulture);

        // assert
        Assert.Null(result);
    }

    [Fact]
    internal void GetField_Id_FieldObject()
    {
        // arrange 
        var table = GetAnonymousTable(_builder, 40, 10);
        foreach (var field in table.Fields.OrderByDescending(x => x.Id))
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
        var table = GetAnonymousTable(_builder, 200, 200);
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
        var table = GetAnonymousTable(_builder, 2, 40);
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
        // arrange - sometimes failing
        var table = GetAnonymousTable(_builder, 2, 85, 'А', 'Я');
        var relations = table.Relations.OrderByDescending(x => x.Name, StringComparer.Ordinal);

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
        var table = GetAnonymousTable(_builder, 2, 0);

        // act 
        var result = TableExtensions.GetRelation(table, "test", StringComparison.CurrentCultureIgnoreCase);

        // assert
        Assert.Null(result);
    }

    [Fact]
    internal void GetRelation_Id_FieldObject()
    {
        // arrange 
        var table = GetAnonymousTable(_builder, 2, 15);
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
    internal void GetColumn_BookTable_ColumnObject()
    {
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("book");

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);
        var column = table.GetColumn("isbn");

        // assert
        Assert.NotNull(column);
        Assert.Equal(6, column.Id);
    }

    [Fact]
    internal void GetColumn_ArmorTable_RelationColumnObject()
    {
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("armor");

		// act 
		Assert.NotNull(schema);
        Assert.NotNull(table);
        var column = table.GetColumn(1, EntityType.Relation);

        // assert
        Assert.NotNull(column);
        Assert.Equal(EntityType.Relation, column.Type);
    }

    [Fact]
    internal void GetColumn_WeaponTable_SearchableColumnObject()
    {
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("weapon");

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);
        var column = table.GetColumn(2, EntityType.SearchableColumn);

        // assert
        Assert.NotNull(column);
        Assert.Equal(EntityType.SearchableColumn, column.Type);
    }


    [Fact]
    internal void GetColumnIndex_AnonymousTable_FindAllColumns()
    {
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = GetAnonymousTable(_builder, 512, 0);

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(table);

        for (var i = 0; i < table.Columns.Length; ++i)
        {
            var index = table.GetColumnIndex(table.Columns[i].Id, table.Columns[i].Type);
            // assert
            Assert.Equal(i, index);
        }
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

    [Fact]
    internal void HasPrimaryKey_LogTable_False()
    {
        // arrange 
        var schemaName = "@Test";
        var schBuilder = new SchemaBuilder();
        var config = new Configuration() { DefaultSchema = schemaName, MaxConnectionPoolSize = 2 };
        var schema = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
        var logTable = schema.GetTable("@log");

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(logTable);
        var result = logTable.HasPrimaryKey();

        // assert
        Assert.False(result);
    }

    [Fact]
    internal void GetPrimaryKey_MetaIdTable_IColumnList()
    {
        // arrange 
        var schemaName = "@Test2";
        var schBuilder = new SchemaBuilder();
        var config = new Configuration() { DefaultSchema = schemaName, MaxConnectionPoolSize = 2 };
        var schema = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
        var metaTable = schema.GetTable("@meta_id");

        // act 
        Assert.NotNull(schema);
        Assert.NotNull(metaTable);
        var result = metaTable.GetPrimaryKey();

        // assert
        Assert.Equal(3, result.Count);
        Assert.Equal("id", result[0].PhysicalName);
        Assert.Equal("schema_id", result[1].PhysicalName);
        Assert.Equal("object_type", result[2].PhysicalName);
    }

    [Fact]
    internal void GetHashCode_TableHashEqual_False()
    {
        // arrange 
        var schemaName = "@Test2";
        var schBuilder = new SchemaBuilder();
        var config = new Configuration() { DefaultSchema = schemaName, MaxConnectionPoolSize = 2 };
        var schema1 = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
        var schema2 = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
        var schema3 = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
        var schema4 = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
        var table1 = schema1.GetTable("@log");
        var table2 = schema2.GetTable("@log");
        var table3 = schema3.GetTable("@log"); // indentical
        var table4 = schema4.GetTable("@log"); // invert two columns
        var table5 = schema4.GetTable("@meta");

        Assert.NotNull(table1);
        Assert.NotNull(table2);
        Assert.NotNull(table3);
        Assert.NotNull(table4);
        Assert.NotNull(table5);

        // act 
        var hash1 = TableExtensions.Hash(table1);
        var hash2 = table2.GetHashCode();
        var hash3 = TableExtensions.Hash(table3);
        var hash4 = table4.GetHashCode();
        var hash5 = TableExtensions.Hash(table5);


        // assert
        Assert.Equal(hash1, hash2);
        Assert.Equal(hash1, hash3);
        Assert.Equal(hash1, hash4);
        Assert.NotEqual(hash1, hash5);
    }


    [Fact]
    internal void Equals_2AnonymousTables_False()
    {
        // arrange 
        var schemaName = "@Test2";
        var schBuilder = new SchemaBuilder();
        var config = new Configuration() { DefaultSchema = schemaName, MaxConnectionPoolSize = 2 };
        var schema1 = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
        var schema2 = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
        var schema3 = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
        var schema4 = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
        var table1 = schema1.GetTable("@log");
        var table2 = schema2.GetTable("@log");
        var table3 = schema3.GetTable("@log");
        var table4 = schema4.GetTable("@meta_id");
        var table5 = schema4.GetTable("@meta");
        var table6 = schema3.GetTable("@meta"); // inverse two fields 

        Assert.NotNull(table1);
        Assert.NotNull(table2);
        Assert.NotNull(table3);
        Assert.NotNull(table4);
        Assert.NotNull(table5);
        Assert.NotNull(table6);

        // copy one field field 
        table6.Fields[2] = table6.Fields[3];

        // act 
        var result1 = table1 == table5;
        var result2 = table2 != table3;
        var result3 = table5.IsEquivalentTo(table1);
        var result4 = table1.Equals((object)table5);
        var result5 = table1.Equals((object)table4);
        var result6 = table5.Equals(table6);

        // assert
        Assert.False(result1);
        Assert.False(result2);
        Assert.False(result3);
        Assert.False(result4);
        Assert.False(result5);
        Assert.False(result6);
    }

}
