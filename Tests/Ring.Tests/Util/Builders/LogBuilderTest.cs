using AutoFixture;
using Ring.Util.Builders;
using Ring.Util.Enums;

namespace Ring.Tests.Util.Builders;

public class LogBuilderTest
{
    private readonly IFixture _fixture;
    public LogBuilderTest() => _fixture = new Fixture();

    [Fact]
    internal void GetError_FileNotFound_LogObject()
    {
        // arrange 
        var sut = new LogBuilder();
        sut.SchemaId = _fixture.Create<int>();
        // act - execution line should be 19!!!!!!!!!!!!!!!!!
        var log = sut.GetError(LogType.FileNotFound, "test");
        // assert
        Assert.NotNull(log);
        Assert.Equal(19, log.LineNumber);
        Assert.Equal("GetError_FileNotFound_LogObject", log.Method);
        Assert.Equal("Ring.Tests.Util.Builders.LogBuilderTest", log.CallSite);
        Assert.Equal(19, log.Id);
    }

    [Fact]
    internal void GetWarning_FileNotFound_LogObject()
    {
        // arrange 
        var sut = new LogBuilder();
        sut.SchemaId = _fixture.Create<int>();
        sut.JobId = _fixture.Create<long>();
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
        var sut = new LogBuilder();
        sut.SchemaId = _fixture.Create<int>();
        sut.JobId = _fixture.Create<long?>();
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
