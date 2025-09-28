using Bogus;
using Ring.Util.Builders;
using Ring.Util.Enums;

namespace Ring.Tests.Util.Builders;

public class LogBuilderTest
{
    private readonly Faker _faker = new();

    [Fact]
    internal void GetError_FileNotFound_LogObject()
    {
        // arrange 
        var sut = new LogBuilder
        {
            SchemaId = _faker.Random.Number(int.MinValue,int.MaxValue)
        };
        // act - execution line should be 20!!!!!!!!!!!!!!!!!
        var log = sut.GetError(LogType.FileNotFound, "test");
        // assert
        Assert.NotNull(log);
        Assert.Equal(20, log.LineNumber);
        Assert.Equal("GetError_FileNotFound_LogObject", log.Method);
        Assert.Equal("Ring.Tests.Util.Builders.LogBuilderTest", log.CallSite);
        Assert.Equal((int)LogType.FileNotFound, log.Id);
    }

    [Fact]
    internal void GetWarning_FileNotFound_LogObject()
    {
        // arrange 
        var sut = new LogBuilder();
        sut.SchemaId = _faker.Random.Number(int.MinValue,int.MaxValue);
        sut.JobId = _faker.Random.Long();
        // act
        var log = sut.GetWarning(LogType.FileNotFound, "test2");
        // assert
        Assert.NotNull(log);
        Assert.Equal("GetWarning_FileNotFound_LogObject", log.Method);
        Assert.Equal("Ring.Tests.Util.Builders.LogBuilderTest", log.CallSite);
        Assert.Equal(sut.JobId, log.JobId);
    }

    [Fact]
    internal void GetInfo_FileNotFound_LogObject()
    {
        // arrange 
        var sut = new LogBuilder
        {
            SchemaId = _faker.Random.Number(int.MinValue,int.MaxValue),
            JobId = _faker.Random.Bool() ? null: _faker.Random.Long()
        };
        var currentThreadId = Environment.CurrentManagedThreadId;
        // act
        var log = sut.GetInfo(LogType.FileNotFound);
        // assert
        Assert.NotNull(log);
        Assert.Equal("GetInfo_FileNotFound_LogObject", log.Method);
        Assert.Equal("Ring.Tests.Util.Builders.LogBuilderTest", log.CallSite);
        Assert.Equal(currentThreadId, log.ThreadId);
        Assert.Equal(sut.JobId, log.JobId);
    }

}
