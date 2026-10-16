using System.Collections;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace Ring.Util.Helpers;

internal sealed class CsvEnumerator : IEnumerator<string?[]>
{

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
	/// <remarks>Elements may be null to represent missing fields. The array is produced by parsing the current line when available[cite: 4]. Note: The underlying array instance is reused across iterations for performance optimization.</remarks>
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

	/// <summary>
	/// Advances the enumerator to the next non-empty CSV record.
	/// </summary>
	/// <returns>
	/// <c>true</c> if a record was found; otherwise, <c>false</c> when the end of the stream is reached[cite: 4].
	/// </returns>
	public bool MoveNext()
	{
		// Skip empty or whitespace-only lines automatically
		while ((_currentLine = _reader.ReadLine()) != null)
		{
			if (!string.IsNullOrWhiteSpace(_currentLine)) return true;
		}
		return false;
	}

	/// <remarks>
	/// The underlying resource stream must support seeking. For compressed resources, the decompression stream and reader are recreated because
	/// a <see cref="GZipStream"/> cannot be repositioned[cite: 4]. Reset() method is disabled[cite: 4].
	/// </remarks>
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
				current.Append(c);
			}
		}

		// Add the last field
		if (elementIndex < _columnCount) _row[elementIndex++] = current.ToString();

		// Differential clear: only clear unused columns from elementIndex onwards
		for (var j = elementIndex; j < _columnCount; j++) _row[j] = null;

		return _row;
	}

	#endregion

}
