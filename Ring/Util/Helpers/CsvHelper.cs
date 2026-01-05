using System.Collections;

namespace Ring.Util.Helpers;

internal class CsvHelper : IEnumerable<string?[]>, IDisposable
{
	private readonly string _resourceNameSpace;
	private readonly string _resourceFile;
	private readonly int _columnCount;
	private readonly bool _compressed;
	private bool _disposed;
	private CsvEnumerator? _currentEnumerator;

	public CsvHelper(string resourceNameSpace, string resourceFile, int columnCount, bool compressed = true)
	{
		_columnCount = columnCount;
		_resourceNameSpace = resourceNameSpace;
		_resourceFile = resourceFile;
		_compressed = compressed;
		_disposed = false;
	}
	public IEnumerator<string?[]> GetEnumerator() => new CsvEnumerator(_resourceNameSpace, _resourceFile, _columnCount, _compressed);
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	public void Dispose()
	{
		// Code size: 40 (0x28)
		if (!_disposed)
		{
			_currentEnumerator?.Dispose();
			_currentEnumerator = null;
			_disposed = true;
		}

	}
}