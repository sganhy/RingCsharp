using System.Collections;
using System.IO.Compression;
using System.Reflection;
using System.Text;

namespace Ring.Util.Helpers;

internal sealed class CsvEnumerator : IEnumerator<string?[]>
{
	private readonly bool _compressed;
	private readonly Stream _stream;
	private GZipStream? _decompressionStream;
	private StreamReader _reader;
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
		// Code size: 84 (0x54)
		var stream = assembly.GetManifestResourceStream(resourceNameSpace + resourceFile) ?? throw new ArgumentNullException(string.Empty);
		_stream = stream;
		_compressed = compressed;
		_columnCount = columnCount;
		_row = new string?[columnCount];
		_reader = CreateReader();
	}

	/// <summary>
	/// Advances the enumerator to the next non-empty CSV record.
	/// </summary>
	/// <returns>
	/// <c>true</c> if a record was found; otherwise, <c>false</c> when the end of the stream is reached[cite: 4].
	/// </returns>
	public bool MoveNext()
	{
		// Code size: 41 (0x29)
		// Skip empty or whitespace-only lines automatically
		while ((_currentLine = _reader.ReadLine()) != null)
		{
			if (!string.IsNullOrWhiteSpace(_currentLine)) return true;
		}
		return false;
	}

	/// <summary>
	/// Rewinds to the first record. The raw resource stream is seeked back to 0; the GZipStream (not repositionable) and the StreamReader are recreated on top of it.
	/// </summary>
	public void Reset()
	{
		// Code size: 82 (0x52)
		ObjectDisposedException.ThrowIf(_disposed, this);
		_reader.Dispose();                // leaveOpen: true, so _stream stays open
		_decompressionStream?.Dispose();  // idem
		_decompressionStream = null;
		_stream.Seek(0, SeekOrigin.Begin);
		_currentLine = null;
		_reader = CreateReader();
	}

	public void Dispose()
	{
		// Code size: 55 (0x37)
		if (!_disposed)
		{
			_reader.Dispose();
			_decompressionStream?.Dispose();
			_stream.Dispose();
			_disposed = true;
		}
	}

	#region private methods 

	private StreamReader CreateReader()
	{
		// Code size: 60 (0x3c)
		Stream source = _stream;
		if (_compressed)
		{
			_decompressionStream = new GZipStream(_stream, CompressionMode.Decompress, leaveOpen: true);
			source = _decompressionStream;
		}
		return new StreamReader(source, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
	}

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
