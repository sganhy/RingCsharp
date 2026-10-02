using Ring.Util.Builders;
using Ring.Util.Builders.PostgreSQL;
using System.Text;

namespace Ring.Tests.Util.Builders.PostgreSQL;

public sealed class TclBuilderTest : BaseBuilderTest
{
	private readonly ITclBuilder _sut;

	public TclBuilderTest()
	{
		_sut = new TclBuilder(Encoding.UTF8, true);
	}

	[Fact]
	internal void StartTransaction_NoInput_Begin()
	{
		// arrange
		var expectedResult = "BEGIN;";
		var expectedBinResult = Encoding.UTF8.GetBytes(expectedResult);

		// act 
		var result = _sut.StartTransaction;

		// assert
		Assert.Equal(expectedResult, result.Text);
		Assert.Equal(expectedBinResult, result.Encoded);
	}

	[Fact]
	internal void Commit_NoInput_Commit()
	{
		// arrange
		var expectedResult = "COMMIT;";
		var expectedBinResult = Encoding.UTF8.GetBytes(expectedResult);

		// act 
		var result = _sut.Commit;

		// assert
		Assert.Equal(expectedResult, result.Text);
		Assert.Equal(expectedBinResult, result.Encoded);
	}

	[Fact]
	internal void Rollback_NoInput_Rollback()
	{
		// arrange
		var expectedResult = "ROLLBACK;";
		var expectedBinResult = Encoding.UTF8.GetBytes(expectedResult);

		// act 
		var result = _sut.Rollback;

		// assert
		Assert.Equal(expectedResult, result.Text);
		Assert.Equal(expectedBinResult, result.Encoded);
	}

	[Fact]
	internal void TransactionalDdl_NoInput_True()
	{
		// arrange
		// act 
		var result = _sut.TransactionalDdl;

		// assert
		Assert.True(result);
	}

}
