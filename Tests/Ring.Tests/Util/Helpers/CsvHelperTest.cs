using Bogus;
using Ring.Util.Helpers;
using System.Reflection;

namespace Ring.Tests.Util.Helpers;

public sealed class CsvHelperTest : BaseTest
{
	private static readonly Assembly CurrentAssembly = typeof(CsvHelperTest).Assembly;

	public CsvHelperTest(ITestOutputHelper output) : base(output) {}

	[Fact]
	public void ReadsEmbeddedMetaCsv_ValidatesRowCountAndColumns()
	{
		// arrange
		// Path matches "Ring.Tests.Resources.meta.csv"
		string resourceNamespace = "Ring.Tests.Resources.";
		string resourceFile = "test_csv_helper.csv";
		int expectedColumnCount = 10;
		int parsedRowCount = 0;

		// act
		using var helper = new CsvHelper(CurrentAssembly, resourceNamespace, resourceFile, columnCount: expectedColumnCount, compressed: false);
		
		// assert
		foreach (var row in helper)
		{
			parsedRowCount++;

			// Assert row structure
			Assert.NotNull(row);
			Assert.Equal(expectedColumnCount, row.Length);

			// Column 0 (Id) should be a valid integer
			Assert.False(string.IsNullOrWhiteSpace(row[0]), $"Row {parsedRowCount} has missing Id.");
			Assert.True(int.TryParse(row[0], out _), $"Row {parsedRowCount} Id '{row[0]}' is not a valid integer.");
		}

		Assert.True(parsedRowCount > 0, "The embedded test_csv_helper.csv resource contained no data rows.");
		Assert.Equal(6,parsedRowCount);
	}

	[Fact]
	public void ReadsTestCsvHelper_HandlesQuotesNullsAndNegativeNumbers()
	{
		// Arrange
		const string resourceNamespace = "Ring.Tests.Resources.";
		const string resourceFile = "test_csv_helper.csv";
		const int expectedColumnCount = 10;

		// Act
		// CsvEnumerator reuses a single array for every row, so each row must be cloned before it is stored.
		using var helper = new CsvHelper(CurrentAssembly, resourceNamespace, resourceFile, columnCount: expectedColumnCount, compressed: false);
		var rows = helper.Select(r => (string?[])r.Clone()).ToList();

		// Dump of everything the parser produced, appended to every assertion message.
		string dump = Environment.NewLine + string.Join(Environment.NewLine,
			rows.Select((r, i) => $"[{i}] " + string.Join(" | ", r.Select(c => c is null ? "<null>" : $"'{c}'"))));

		// Assert
		Assert.True(rows.Count > 0, "No rows were read from the resource." + dump);
		Assert.All(rows, r => Assert.Equal(expectedColumnCount, r.Length));

		// 1. Escaped double quotes must be unescaped: "Default ""Language""" -> Default "Language"
		var rowWithQuotes = rows.FirstOrDefault(r => r[7] != null && r[7]!.Contains('"'));
		Assert.True(rowWithQuotes != null, "No row has a '\"' in column 7 (description)." + dump);
		Assert.Equal("Default \"Language\"", rows[0][7]);
		Assert.All(rows, r =>
		{
			Assert.DoesNotContain("\"\"", r[7] ?? string.Empty); // doubled quotes must have collapsed to single ones
			Assert.DoesNotContain("\"\"", r[8] ?? string.Empty);
		});

		// 2. Negative numbers must be read intact (column 5)
		var rowWithNegative = rows.FirstOrDefault(r => r[5] != null && r[5]!.StartsWith('-'));
		Assert.True(rowWithNegative != null, "No row has a negative value in column 5." + dump);
		Assert.True(long.TryParse(rowWithNegative![5], out long negativeValue), $"Column 5 '{rowWithNegative[5]}' is not a valid long." + dump);
		Assert.Equal(-122880L, negativeValue);

		// 3. Empty fields present in the line must be string.Empty, never null
		//    (rows 3 and 5 have an empty description, rows 4 and 5 have an empty value)
		Assert.Equal(string.Empty, rows[3][7]);
		Assert.Equal(string.Empty, rows[4][8]);
		Assert.Equal(string.Empty, rows[5][7]);
		Assert.Equal(string.Empty, rows[5][8]);
		Assert.All(rows, r => Assert.All(r, c => Assert.NotNull(c))); // every row in this file is complete, so no nulls

		// 4. Commas, '>' and quotes inside a quoted field must not split the field
		Assert.Equal("CHECK (Value > 0 AND Name LIKE \"%test%\")", rows[4][7]);
	}

	[Fact]
	public void Reset_RewindsToFirstRow_AndReplaysSameSequence()
	{
		// Arrange
		const string resourceNamespace = "Ring.Tests.Resources.";
		const string resourceFile = "test_csv_helper.csv";
		const int expectedColumnCount = 10;

		using var helper = new CsvHelper(CurrentAssembly, resourceNamespace, resourceFile, columnCount: expectedColumnCount, compressed: false);
		// Use a single enumerator: CsvHelper.GetEnumerator() disposes the previous one.
		using var enumerator = helper.GetEnumerator();

		// Act: first pass (rows are cloned because the enumerator reuses one array)
		var firstPass = new List<string?[]>();
		while (enumerator.MoveNext()) firstPass.Add((string?[])enumerator.Current.Clone());

		enumerator.Reset();

		var secondPass = new List<string?[]>();
		while (enumerator.MoveNext()) secondPass.Add((string?[])enumerator.Current.Clone());

		// Assert
		Assert.NotEmpty(firstPass);
		Assert.Equal(firstPass.Count, secondPass.Count);
		for (int i = 0; i < firstPass.Count; i++)
			Assert.Equal(firstPass[i], secondPass[i]);

		// Sanity check on known content: the first row after the reset is the first row of the file
		Assert.Equal("4", secondPass[0][0]);
		Assert.Equal("@language", secondPass[0][6]);
		Assert.Equal("Default \"Language\"", secondPass[0][7]);
	}

	[Fact]
	public void Reset_InTheMiddleOfEnumeration_RestartsFromFirstRow()
	{
		// Arrange
		using var helper = new CsvHelper(CurrentAssembly, "Ring.Tests.Resources.", "test_csv_helper.csv", columnCount: 10, compressed: false);
		using var enumerator = helper.GetEnumerator();

		// Act: consume 3 rows, then reset
		Assert.True(enumerator.MoveNext());
		Assert.True(enumerator.MoveNext());
		Assert.True(enumerator.MoveNext());
		Assert.Equal("2", enumerator.Current[0]); // third row (id = 2)

		enumerator.Reset();

		// Assert: nothing is current right after a reset, and the next row is the first one again
		Assert.Empty(enumerator.Current);
		Assert.True(enumerator.MoveNext());
		Assert.Equal("4", enumerator.Current[0]);
		Assert.Equal("@language", enumerator.Current[6]);
	}

	[Fact]
	public void Reset_AfterDispose_ThrowsObjectDisposedException()
	{
		// Arrange
		using var helper = new CsvHelper(CurrentAssembly, "Ring.Tests.Resources.", "test_csv_helper.csv", columnCount: 10, compressed: false);
		var enumerator = helper.GetEnumerator();
		enumerator.MoveNext();
		enumerator.Dispose();

		// Act + Assert
		Assert.Throws<ObjectDisposedException>(() => enumerator.Reset());
		Assert.Throws<ObjectDisposedException>(() => enumerator.MoveNext());
	}


	[Fact]
	public void GetEnumerator_QuotedEmptyAndNegativeFields_ParsedCorrectly()
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
