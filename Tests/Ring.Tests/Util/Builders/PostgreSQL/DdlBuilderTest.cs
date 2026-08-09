using Bogus;
using Ring.Data;
using Ring.Schema;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Extensions;
using Ring.Schema.Models;
using Ring.Util.Builders;
using DdlBuilder = Ring.Util.Builders.PostgreSQL.DdlBuilder; // test only for PostgreSQL

namespace Ring.Tests.Util.Builders.PostgreSQL;

public sealed class DdlBuilderTest : BaseBuilderTest
{
	private readonly IDdlBuilder _sut = new DdlBuilder();
	private readonly Faker _faker = new();

    [Fact]
    public void Drop_Table1_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(_sut, 12, 2);
        var expectedSql = $"DROP TABLE {table.PhysicalName}";

        // act 
        var dql = _sut.Drop(table);

		// assert
		Assert.Equal(expectedSql, dql);
    }

    [Fact]
    public void Create_Table1_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(_sut, 12, 2);
        var expectedSql = $"CREATE TABLE {table.PhysicalName} (";

        // act 
        var dql = _sut.Create(table);

        // assert
        Assert.True(dql?.StartsWith(expectedSql));
        Assert.True(dql?.EndsWith(')'));
    }

    [Fact]
    public void Create_BusinessTable_DdlQuery()
    {
        // arrange 
        var metaTable = GetMeta2Table(TableType.Business);
        var metaItems = GetMeta2TableItems(true);
        var physicalName = _faker.Random.String();
        var segment = new ArraySegment<Meta>(metaItems, 0, metaItems.Length);
        var table2 = metaTable.ToTable(segment, PhysicalType.Table, _sut, physicalName, 0);
        Assert.NotNull(table2);
        table2.Relations[1] = GetAnonymousRelation(RelationType.Mto, 1, @"skill2book");
        table2.Relations[0] = GetAnonymousRelation(RelationType.Mtm, 8, @"ability2book");

        var expectedSql = $"CREATE TABLE {physicalName} (\n" + "\tid int2,\n" +
                "\tname varchar(80) COLLATE \"C\",\n" + "\ts_name varchar(80) COLLATE \"C\",\n" + 
                "\tsub_name varchar(30) COLLATE \"C\",\n" + "\tis_group bool,\n" +
                "\tcategory varchar(8) COLLATE \"C\",\n" + "\tarmor_penality int2,\n" + "\ttrained_only bool,\n" +
                "\ttry_again bool,\n\tskill2book int8)";

        // act 
        var ddl = _sut.Create(table2);

        // assert
        Assert.Equal(expectedSql, ddl);
    }

    [Fact]
    public void Create_MtmTable_DdlQuery()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        Assert.NotNull(schema);
        var table = schema.GetTable("skill");
        Assert.NotNull(table);
        var relation  = table.GetRelation("synergy2skill");
        Assert.NotNull(relation);
        var mtmTable = relation.ToTable;
        var expectedSql = $"CREATE TABLE {mtmTable.PhysicalName} (\n" + "\tskill2synergy int2,\n\tsynergy2skill int2)";

        // act 
        var ddl = _sut.Create(mtmTable);


        // assert
        Assert.Equal(expectedSql, ddl);
    }

    [Fact]
    public void Create_LexiconTable_DdlQuery()
    {
        // arrange 
        var metaTable = GetMeta2Table(TableType.Lexicon);
        var metaItems = GetMeta2TableItems(false);
        var physicalName = _faker.Random.String();
        var segment = new ArraySegment<Meta>(metaItems, 0, metaItems.Length);
        var table3 = metaTable.ToTable(segment, PhysicalType.Table, _sut, physicalName, 0);
        Assert.NotNull(table3);
        // load relation
        table3.Relations[0] = GetAnonymousRelation(RelationType.Mto, 8, @"skill2book");
        var expectedSql = $"CREATE TABLE {physicalName} (\n" + "\tid int2,\n" +
                "\tname varchar(80) COLLATE \"C\",\n" + "\ts_name varchar(80) COLLATE \"C\",\n" +
                "\tsub_name varchar(30) COLLATE \"C\",\n" + "\tis_group bool,\n" +
                "\tcategory varchar(8) COLLATE \"C\",\n" + "\tarmor_penality int2,\n" + "\ttrained_only bool,\n" +
                "\ttry_again bool,\n" + "\tskill2book int8)";

        // act 
        var ddl = _sut.Create(table3);

        // assert
        Assert.Equal(expectedSql, ddl);
    }

    [Fact]
    public void Create_LogTable_DdlQuery()
    {
        // arrange 
        var metaTable = GetMeta2Table(TableType.Log);
        var tablespaceName = _faker.Random.String();
        var tablespace = GetAnonymousTableSpace(tablespaceName);
        var metaItems = GetMeta2TableItems(false);
        var physicalName = _faker.Random.String();
        var segment = new ArraySegment<Meta>(metaItems, 0, metaItems.Length);
        var table4 = metaTable.ToTable(segment, PhysicalType.Table, _sut, physicalName, 0);

        Assert.NotNull(table4); // <-- test if null
		table4.Relations[0] = GetAnonymousRelation(RelationType.Mto, 11, @"skill2book", false);
        
        var expectedSql = $"CREATE TABLE {physicalName} (\n" + "\tid int2,\n" +
                "\tname varchar(80) COLLATE \"C\",\n" + "\ts_name varchar(80) COLLATE \"C\",\n" +
                "\tsub_name varchar(30) COLLATE \"C\",\n" + "\tis_group bool,\n" +
                "\tcategory varchar(8) COLLATE \"C\",\n" + "\tarmor_penality int2,\n" + "\ttrained_only bool,\n" +
                "\ttry_again bool,\n" + $"\tskill2book int8) TABLESPACE {tablespaceName}";

        // act 
        var ddl = _sut.Create(table4, tablespace);


        // assert
        Assert.Equal(expectedSql, ddl);
    }

    [Fact]
    public void Create_MetaTable_DdlQuery()
    {
        // arrange 
        var tablespaceName = _faker.Random.String();
        var tablespace = GetAnonymousTableSpace(tablespaceName);
        var builder = new SchemaBuilder();
        var config = new Configuration() { DefaultSchema = "test", MaxConnectionPoolSize = 1 };
        var schema = builder.GetMeta(DatabaseProvider.PostgreSql, config);
        var metaTable = schema.GetTable("@meta");
        var expectedSql = $"CREATE TABLE test.\"@meta\" (\n" + "\tid int4,\n" +
                "\tschema_id int4,\n" + "\tobject_type int2,\n" + "\treference_id int4,\n" +
                "\tdata_type int4,\n" + "\tflags int8,\n" + "\tname varchar(60) COLLATE \"C\",\n" +
                "\tdescription text COLLATE \"C\",\n" + $"\tvalue text COLLATE \"C\",\n\tactive bool) TABLESPACE {tablespaceName}";
        Assert.NotNull(metaTable);

        // act 
        var ddl = _sut.Create(metaTable, tablespace);

        // assert
        Assert.Equal(expectedSql, ddl);
    }

    [Fact]
    public void Create_TestTable_DdlQuery()
    {
        // arrange 
        var tablespaceName = _faker.Random.String();
        var tablespace = GetAnonymousTableSpace(tablespaceName);
        var builder = new SchemaBuilder();
        var config = new Configuration() { DefaultSchema = "test", MaxConnectionPoolSize = 1 };
        var schema = builder.GetMeta(DatabaseProvider.PostgreSql, config);
        var testTable = schema.GetTable("@test");
        // change test_11 field to not null!
        Assert.NotNull(testTable);
        var index = testTable.GetFieldIndex("test_11");
        testTable.Fields[index] = testTable.Fields[index].SetNotNull(true);
        var expectedSql = $"CREATE TABLE test.\"@test\" (\n" + "\ttest_0 int8,\n" +
                "\ttest_1 int4,\n" + "\ttest_2 int2,\n" + "\ttest_3 int2,\n" +
                "\ttest_4 float4,\n" + "\ttest_5 float8,\n" + "\ttest_6 varchar(16) COLLATE \"C\",\n" + "\ttest_7 varchar(512) COLLATE \"C\",\n" +
                "\ts_test_7 varchar(512) COLLATE \"C\",\n" + "\ttest_8 varchar(64) COLLATE \"C\",\n" + "\ts_test_8 varchar(64) COLLATE \"C\",\n" +
                "\ttest_9 date,\n" + "\ttest_10 timestamp without time zone,\n\ttest_11 timestamp without time zone,\n" +
                "\t\"@tz_offset_11\" int2,\n" + "\ttest_12 bytea,\n" + "\ttest_13 bool,\n\ttest_14 text COLLATE \"C\")" +
                $" TABLESPACE {tablespaceName}";

        // act 
        var ddl = _sut.Create(testTable, tablespace);

        // assert
        Assert.Equal(expectedSql, ddl);
    }

    [Fact]
    public void Create_TableSpace1_DdlQuery()
    {
        // arrange 
        var fileName = _faker.Random.String();
        var tablespaceName = _faker.Random.String();
        var tablespace = new TableSpace(_faker.Random.Number(int.MinValue,int.MaxValue), tablespaceName, tablespaceName, _faker.Random.String(), true, true, true,
            _faker.Random.WordsArray(11),
            fileName, true, true);
        var expectedSql = $"CREATE TABLESPACE {tablespaceName} LOCATION '{fileName}'";

        // act 
        var dql = _sut.Create(tablespace);

        // assert
        Assert.Equal(expectedSql, dql);
    }

    [Fact]
    public void Create_Index1_DdlQuery()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("book");
        var expectedResult = "CREATE INDEX idx_1021_02 ON rpg_sheet.t_book (s_title)";
        Assert.NotNull(table);

		// act 
		var dql = _sut.Create(table.Indexes[1], table);

        // assert
        Assert.Equal(expectedResult, dql);
    }

    [Fact]
    public void Create_IndexMeta_DdlQuery()
    {
        // arrange 
        var schBuilder = new SchemaBuilder();
        var schemaName = "@Test";
        var config = new Configuration() { DefaultSchema = schemaName, MaxConnectionPoolSize = 2 };
        var schema = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
        var table = schema.GetTable("@meta");
        var expectedResult = "CREATE UNIQUE INDEX \"idx_@meta_10\" ON \"@test\".\"@meta\" (id,schema_id,object_type,reference_id)";
        Assert.NotNull(table);

        // act 
        var dql = _sut.Create(table.Indexes[0], table);

        // assert
        Assert.Equal(expectedResult, dql);
    }

    [Fact]
    public void Create_IndexWithTablespace_DdlQuery()
    {
        // arrange 
        var schBuilder = new SchemaBuilder();
        var schemaName = "@Test";
        var config = new Configuration() { DefaultSchema = schemaName, MaxConnectionPoolSize = 2, 
            DefaultIndexStorage = "tblSpc_test" };
        var schema = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
        var tablespace = schema.TableSpaces[0];
        var table = schema.GetTable("@meta");
        var expectedResult = "CREATE UNIQUE INDEX \"idx_@meta_10\" ON \"@test\".\"@meta\" (id,schema_id,object_type,reference_id) TABLESPACE tblSpc_test";
        Assert.NotNull(table);
        Assert.NotNull(tablespace);

        // act 
        var dql = _sut.Create(table.Indexes[0], table, tablespace);

        // assert
        Assert.Equal(expectedResult, dql);
    }

    [Fact]
    public void Create_PkConstraintMeta_DdlQuery()
    {
        // arrange 
        var schBuilder = new SchemaBuilder();
        var schemaName = "@Test";
        var config = new Configuration() { DefaultSchema = schemaName, MaxConnectionPoolSize = 2 };
        var schema = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
        var table = schema.GetTable("@meta");
        Assert.NotNull(table);
        var pk = schema.DdlBuilder.GetConstraints(table).First(p => p.Type == ConstraintType.PrimaryKey);
        var expectedSql = "ALTER TABLE \"@test\".\"@meta\" ADD CONSTRAINT \"pk_@meta\" PRIMARY KEY (id,schema_id,object_type,reference_id)";

        // act 
        var dql = _sut.Create(pk);

        // assert
        Assert.Equal(expectedSql, dql);
    }

	[Fact]
	public void Create_NotNullConstraint_DdlQuery()
	{
		// arrange 
		var schBuilder = new SchemaBuilder();
		var schemaName = "@Test";
		var config = new Configuration() { DefaultSchema = schemaName, MaxConnectionPoolSize = 2 };
		var schema = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
		var table = schema.GetTable("@meta");
		Assert.NotNull(table);
		var notNull = schema.DdlBuilder.GetConstraints(table).First(p => p.Type == ConstraintType.NotNull);
		var expectedSql = "ALTER TABLE \"@test\".\"@meta\" ALTER COLUMN id SET NOT NULL";

		// act 
		var dql = _sut.Create(notNull);

		// assert
		Assert.Equal(expectedSql, dql);
	}

	[Fact]
	public void Create_CheckConstraint_DdlQuery()
	{
		// arrange 
		var schBuilder = new SchemaBuilder();
		var schemaName = "@Test";
		var config = new Configuration() { DefaultSchema = schemaName, MaxConnectionPoolSize = 2 };
		var schema = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
		var table = schema.GetTable("@meta");
		Assert.NotNull(table);
		var checks = schema.DdlBuilder.GetConstraints(table).Where(p => p.Type == ConstraintType.Check).ToArray();
		Assert.Equal(3, checks.Length);
		var check = checks[1];
		//eg. ALTER TABLE public."@meta" ADD CONSTRAINT "ck_@meta_002" CHECK (object_type between 0 and 124);
		var expectedSql = $"ALTER TABLE \"@test\".\"@meta\" ADD CONSTRAINT \"ck_@meta_002\" CHECK (object_type>={check.MinValue} AND object_type<={check.MaxValue})";

		// act 
		var dql = _sut.Create(check);

		// assert
		Assert.Equal(expectedSql, dql);
	}

	[Fact]
    public void Create_PkConstraintMetaId_DdlQuery()
    {
        // arrange 
        var schBuilder = new SchemaBuilder();
        var schemaName = "@Test";
        var config = new Configuration() { DefaultSchema = schemaName, MaxConnectionPoolSize = 2 };
        var schema = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
        var table = schema.GetTable("@meta_id");
        Assert.NotNull(table);
        var pk = schema.DdlBuilder.GetConstraints(table).First(p => p.Type == ConstraintType.PrimaryKey);
        var expectedSql = "ALTER TABLE \"@test\".\"@meta_id\" ADD CONSTRAINT \"pk_@meta_id\" PRIMARY KEY (id,schema_id,object_type)";

        // act 
        var dql = _sut.Create(pk);

        // assert
        Assert.Equal(expectedSql, dql);
    }

    [Fact]
    public void Truncate_Table1_DdlQuery()
    {
        // arrange 
        var table = GetAnonymousTable(_sut, 12, 2);
        var expectedSql = $"TRUNCATE TABLE {table.PhysicalName}";

        // act 
        var dql = _sut.Truncate(table);

        // assert
        Assert.Equal(expectedSql, dql);
    }


	[Fact]
    public void GetPhysicalName_Field1_FieldName()
    {
        // arrange 
        var field = GetAnonymousField(FieldType.DateTime, 12, 1, "liKe");
        var expectedSql = $"li_ke";

        // act 
        var result = _sut.GetPhysicalName(EntityType.Field, field.Name);

        // assert
        Assert.Equal(expectedSql, result);
    }

    [Fact]
    public void GetPhysicalName_Field2_FieldName()
    {
        // arrange 
        var field = GetAnonymousField(FieldType.String, 12, 1, "zorba_le_grec");
        var expectedSql = field.Name;

        // act 
        var result = _sut.GetPhysicalName(EntityType.Field, field.Name);

        // assert
        Assert.Equal(expectedSql, result);
    }

    [Fact]
    public void GetPhysicalName_Field3_FieldName()
    {
        // arrange 
        var field = GetAnonymousField(FieldType.Undefined, 12, 1, "@user");
        var expectedSql = $"\"{field.Name}\"";

        // act 
        var result = _sut.GetPhysicalName(EntityType.Field, field.Name);

        // assert
        Assert.Equal(expectedSql, result);
    }

    [Fact]
    public void GetPhysicalName_Relation1_RelationName()
    {
        // arrange 
        var relation = GetAnonymousRelation(RelationType.Otop, 5, "rETURnINg", false);
        var expectedSql = "r_e_t_u_rn_i_ng";

        // act 
        var result = _sut.GetPhysicalName(EntityType.Relation, relation.Name);

        // assert
        Assert.Equal(expectedSql, result);
    }

    [Fact]
    public void GetPhysicalName_Table1_TableName()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList,DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("campaign_setting");
        var expectedResult = "rpg_sheet.t_campaign_setting";

        // act 
        Assert.NotNull(table);
        Assert.NotNull(schema);
        var result = _sut.GetPhysicalName(table, schema);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void GetPhysicalName_Schema1_SchemaName()
    {
        // arrange 
        var meta = Meta.Create("RpgSheet");
        var expectedResult = "rpg_sheet";

        // act 
        var result = _sut.GetPhysicalName(EntityType.Schema, meta.Name);

        // assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void GetPhysicalName_Schema2_SchemaName()
    {
        // arrange 
        var meta = Meta.Create("@Test");
        var expectedResult = "\"@test\"";

        // act 
        var result = _sut.GetPhysicalName(EntityType.Schema, meta.Name);

        // assert
        Assert.Equal(expectedResult, result);
    }

	[Fact]
	public void GetPhysicalName_Table1_TableCatalogPhysicalName()
	{
		// arrange 
		var meta = Meta.Create("@Test");
		var expectedResult = "\"@test\"";

		// act 
		var result = _sut.GetPhysicalName(EntityType.Schema, meta.Name);

		// assert
		Assert.Equal(expectedResult, result);
	}

	[Fact]
    public void GetPhysicalName_MtmTable1_TableName()
    {
        // arrange 
        var metaTable = new Meta(_faker.Random.Number(int.MinValue,int.MaxValue), (byte)EntityType.Table, 0, (int)TableType.Mtm
            , 0L, TableType.Mtm.GetLogicalName("Test"), null, null, true);
        var emptyTable = Meta.GetDefaultTable(metaTable);
        var emptySchema = Meta.GetDefaultSchema(Meta.Create("Where"), DatabaseProvider.MySql);
        var ddlBuilder = DatabaseProvider.PostgreSql.GetDdlBuilder();
        var expectedValue = "\"where\".\"@mtm_test\"";

        // act 
        var physicalName = ddlBuilder.GetPhysicalName(emptyTable, emptySchema);

        // assert
        Assert.Equal(expectedValue, physicalName);
    }

    [Fact]
    public void GetConstraints_PkConstraint1_ConstraintName()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("feat");
        var ddlBuilder = DatabaseProvider.PostgreSql.GetDdlBuilder();
        var expectedResult = "pk_feat";
        Assert.NotNull(table);
        var constraint = new Constraint(ConstraintType.PrimaryKey, table, string.Empty);
        
        // act 
        var constraintPk = ddlBuilder.GetConstraints(table).First(p => p.Type == ConstraintType.PrimaryKey);

        // assert
        Assert.Equal(expectedResult, constraintPk.PhysicalName);
    }

    [Fact]
    public void GetPhysicalName_Index0DeityTable_IndexName()
    {
        // arrange 
        var metaList = GetSchema1();
        var schema = Meta.ToSchema(metaList, DatabaseProvider.PostgreSql);
        var table = schema?.GetTable("deity");
        var ddlBuilder = DatabaseProvider.PostgreSql.GetDdlBuilder();
        var expectedResult = "idx_1037_01";
        Assert.NotNull(table);

		// act 
		var physicalName = ddlBuilder.GetPhysicalName(table.Indexes[0], table);

        // assert
        Assert.Equal(expectedResult, physicalName);
    }

    [Fact]
    public void GetPhysicalName_Index0LogTable_IndexName()
    {
        // arrange 
        var schBuilder = new SchemaBuilder();
        var config = new Configuration() { DefaultSchema = "public", MaxConnectionPoolSize = 2 };
        var schema = schBuilder.GetMeta(DatabaseProvider.PostgreSql, config);
        var table = schema.GetTable("@log");
        var ddlBuilder = DatabaseProvider.PostgreSql.GetDdlBuilder();
        var expectedResult = "\"idx_@log_11\"";
		Assert.NotNull(table);

		// act 
		var physicalName = ddlBuilder.GetPhysicalName(table.Indexes[0], table);

        // assert
        Assert.Equal(expectedResult, physicalName);
    }

}