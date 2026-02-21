using System.Collections;
using System.IO.Compression;
using System.Reflection;

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
		// Code size: 211(0xd3)
		string? current = null;
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
					current += '"';
					i++; // Skip next quote
				}
				else
				{
					inQuotes = !inQuotes;
				}
			}
			else if (c == ',' && !inQuotes)
			{
				_row[elementIndex++] = current;
				current = null;
			}
			else
			{
				current += c;
			}
		}

		// Add the last field
		if (elementIndex < _columnCount) _row[elementIndex++] = current;

		// Differential clear: only clear unused columns from elementIndex onwards
		// This avoids clearing the entire array when we just parsed data into it
		for (var j = elementIndex; j < _columnCount; j++) _row[j] = null;

		return _row;
	}

	#endregion

}
