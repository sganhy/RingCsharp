using Ring.Util.Helpers;
using System.Reflection;

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

	[Fact]
	public void GetEnumerator_TestCsv_TestRows()
	{
		// arrange 
		var csvHelper = new CsvHelper(Assembly.GetExecutingAssembly(), "Ring.Tests.Util.Resources.", "Test.gz", 8, true);
		var result = new List<(string?, string?, string?, string?)>();		

		// act 
		foreach (var row in csvHelper) result.Add((row[0], row[1], row[2], row[3]));
		csvHelper.Dispose();

		// assert
		Assert.Equal(3, result.Count);
		var row1 = result[0];
		var row2 = result[1];
		var row3 = result[2];
		
		// row1: a1,a2,a3
		Assert.Equal("a1", row1.Item1);
		Assert.Equal("a2", row1.Item2);
		Assert.Equal("a3", row1.Item3);
		Assert.Null(row1.Item4);
		// row2: b1,"b2,b3","b3,b4"
		Assert.Equal("b1", row2.Item1);
		Assert.Equal("b2,b3", row2.Item2);
		Assert.Equal("b3,b4", row2.Item3);
		Assert.Null(row2.Item4);
		// row3: c1,c2,"c3",c4
		Assert.Equal("c1", row3.Item1);
		Assert.Equal("c2", row3.Item2);
		Assert.Equal("c3", row3.Item3);
		Assert.Equal("c4", row3.Item4);
	}

}
