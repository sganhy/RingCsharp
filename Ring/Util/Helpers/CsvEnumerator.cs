using System.Collections;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace Ring.Util.Helpers;

internal sealed class CsvEnumerator : IEnumerator<string?[]>
{

	// BUGS (Claude source 4.6):
	//     1) Current property: _row is returned by reference from Current: High (not really a bug the aim is to redure allocations at initialization)
	//     2) Current property: re-parses on every access; Medium (Not a bug: Access is unique to simply source code)
	//     3) ParseLine(): current += c string concatenation inside a loop; Medium (Fixed)
	//     4) MoveNext(): Empty lines are not skipped; Low (Fixed)

	private readonly Stream _stream;
	private readonly GZipStream? _decompressionStream;
	private readonly StreamReader _reader;
	private string? _currentLine;
	private readonly int _columnCount;
	private readonly string?[] _row;
	private bool _disposed;

	/// <summary>
	///		Gets the parsed fields of the current input line.
	/// </summary>
	/// <remarks>Elements may be null to represent missing fields. The array is produced by parsing the current line when available.</remarks>
	public string?[] Current => _currentLine != null ? ParseLine(_currentLine) : Array.Empty<string?>();

    object IEnumerator.Current => Current;

    public CsvEnumerator(Assembly assembly, string resourceNameSpace, string resourceFile, int columnCount, bool compressed)
	{
		var resource = resourceNameSpace + resourceFile;
		ArgumentNullException.ThrowIfNull(assembly);
		var stream = assembly?.GetManifestResourceStream(resource);
		ArgumentNullException.ThrowIfNull(stream);
		_stream = stream;

		if (compressed)
		{
			_decompressionStream = new GZipStream(_stream, CompressionMode.Decompress);
			_reader = new StreamReader(_decompressionStream);
		}
		else
		{
			_reader = new StreamReader(_stream);
		}
		_row = new string?[columnCount];
		_columnCount = columnCount;
		_disposed = false;
	}

	public bool MoveNext()
	{
		_currentLine = _reader.ReadLine();
		return _currentLine != null;
	}

	public void Reset() => throw new NotSupportedException();

	public void Dispose()
	{
		if (!_disposed)
		{
			_reader.Dispose();
			_decompressionStream?.Dispose();
			_stream.Dispose();
			_disposed = true;
		}
	}

	#region private methods 

	private string?[] ParseLine(string line)
	{
		// Code size: 213 (0xd5)
		StringBuilder current = new();
		bool inQuotes = false;
		var i = 0;
		var elementIndex = 0;
		for (; i < line.Length && elementIndex < _columnCount; i++)
		{
			char c = line[i];
			if (c == '"')
			{
				if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
				{
					current.Append('"');
					i++; // Skip next quote
				}
				else
				{
					inQuotes = !inQuotes;
				}
			}
			else if (c == ',' && !inQuotes)
			{
				_row[elementIndex++] = current.ToString();
				current.Clear();
			}
			else
			{
				current.Append(c); // ← O(n²) allocation for each field character
			}
		}

		// Add the last field
		if (elementIndex < _columnCount) _row[elementIndex++] = current.ToString();

		// Differential clear: only clear unused columns from elementIndex onwards
		// This avoids clearing the entire array when we just parsed data into it
		for (var j = elementIndex; j < _columnCount; j++) _row[j] = null;

		return _row;
	}

	#endregion

}
