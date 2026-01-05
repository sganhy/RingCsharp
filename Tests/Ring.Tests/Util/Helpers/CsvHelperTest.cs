using Ring.Util.Helpers;
using Xunit.Abstractions;

namespace Ring.Tests.Util.Helpers;

public sealed class CsvHelperTest : BaseTest
{
	public CsvHelperTest(ITestOutputHelper output) : base(output)
	{
	}

	[Fact]
	public void GetEnumerator_ParameterCsv_3FirstRowHeader()
	{
		// arrange 
		var csvHelper = new CsvHelper("Ring.Util.Resources.", "Parameter.gz", 3, true);

		// act 
		var row = csvHelper.FirstOrDefault();
		csvHelper.Dispose();

		// assert
		Assert.NotNull(row);
		Assert.Equal(3, row.Length);
		Assert.Equal("id", row[0]);
		Assert.Equal("@name", row[1]);
		Assert.Equal("Description", row[2]);
	}

	[Fact]
	public void GetEnumerator_ParameterCsv_8FirstRowHeader()
	{
		// arrange 
		var csvHelper = new CsvHelper("Ring.Util.Resources.", "Parameter.gz", 8, true);

		// act 
		var row = csvHelper.FirstOrDefault();
		csvHelper.Dispose();

		// assert
		Assert.NotNull(row);
		Assert.Equal(8, row.Length);
		Assert.Equal("id", row[0]);
		Assert.Equal("@name", row[1]);
		Assert.Equal("Description", row[2]);
		Assert.Equal("FieldType", row[3]);
		Assert.Equal("EntityType", row[4]);
		Assert.Equal("DefaultValue", row[5]);
		Assert.Equal("Value", row[6]);
		Assert.Null(row[7]);
	}

	[Fact]
	public void GetEnumerator_ParameterCsv_8FirstRowParameter()
	{
		// arrange 
		var csvHelper = new CsvHelper("Ring.Util.Resources.", "Parameter.gz", 8, true);
		var index = 0;
		string? lastCol = null;
		string? firstCol = null;

		// act 
		foreach (var row in csvHelper)
		{
			if (index == 1)
			{
				lastCol = row[7];
				firstCol = row[0];
			}
			index++;
		}
		csvHelper.Dispose();

		// assert
		Assert.NotNull(firstCol);
		Assert.Null(lastCol);
		Assert.Equal("1", firstCol);
	}

}
