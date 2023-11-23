using Ring.Schema.Enums;
using Ring.Schema.Extensions;

namespace Ring.Tests.Schema.Extensions;

public class DatabaseProviderExtensionsTest
{

    [Fact]
    internal void IsReservedWord_PostgreSql_False()
    {
        // arrange 
        var input = "Test22";

        // act 
        var result = DatabaseProvider.PostgreSql.IsReservedWord(input);

        // assert
        Assert.False(result);
    }


    [Fact]
    internal void IsReservedWord_PostgreSql_True()
    {
        // arrange 
        var input = "oFFsET";

        // act 
        var result = DatabaseProvider.PostgreSql.IsReservedWord(input);

        // assert
        Assert.True(result);
    }

    [Fact]
    internal void IsReservedWord_MySql_False()
    {
        // arrange 
        var input = "oFFsET";

        // act 
        var result = DatabaseProvider.MySql.IsReservedWord(input);

        // assert
        Assert.False(result);
    }

    [Fact]
    internal void IsReservedWord_MySql_True()
    {
        // arrange 
        var input = "cUBe";

        // act 
        var result = DatabaseProvider.MySql.IsReservedWord(input);

        // assert
        Assert.True(result);
    }

    [Fact]
    internal void IsReservedWord_SqlServer_False()
    {
        // arrange 
        var input = "oFFsET";

        // act 
        var result = DatabaseProvider.SqlServer.IsReservedWord(input);

        // assert
        Assert.False(result);
    }

    [Fact]
    internal void IsReservedWord_SqlServer_1_True()
    {
        // arrange 
        var input = "ORDInALITy";

        // act 
        var result = DatabaseProvider.SqlServer.IsReservedWord(input);

        // assert
        Assert.True(result);
    }

    [Fact]
    internal void IsReservedWord_SqlServer_2_True()
    {
        // arrange 
        var input = "INITIALIZe";

        // act 
        var result = DatabaseProvider.SqlServer.IsReservedWord(input);

        // assert
        Assert.True(result);
    }

    [Fact]
    internal void IsReservedWord_SQLite_False()
    {
        // arrange 
        var input = "ORDInALITy";

        // act 
        var result = DatabaseProvider.SqlLite.IsReservedWord(input);

        // assert
        Assert.False(result);
    }

    [Fact]
    internal void IsReservedWord_SQLite_True()
    {
        // arrange 
        var input = "cONFLiCT";

        // act 
        var result = DatabaseProvider.SqlLite.IsReservedWord(input);

        // assert
        Assert.True(result);
    }

    [Fact]
    internal void IsReservedWord_Undefined_True()
    {
        // arrange 
        var input = "ORDInALITy";

        // act 
        var result = DatabaseProvider.Undefined.IsReservedWord(input);

        // assert
        Assert.True(result);
    }


}
