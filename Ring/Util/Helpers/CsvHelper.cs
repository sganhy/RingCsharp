using System.Collections;
using System.Reflection;

namespace Ring.Util.Helpers;

internal sealed class CsvHelper : IEnumerable<string?[]>, IDisposable
{
	private readonly string _resourceNameSpace;
	private readonly string _resourceFile;
	private readonly int _columnCount;
	private readonly bool _compressed;
	private readonly Assembly _assembly;
	private bool _disposed;
	private CsvEnumerator? _currentEnumerator;

	public CsvHelper(string resourceNameSpace, string resourceFile, int columnCount, bool compressed = true)
		: this(Assembly.GetExecutingAssembly(), resourceNameSpace, resourceFile, columnCount, compressed) { }

	public CsvHelper(Assembly assembly, string resourceNameSpace, string resourceFile, int columnCount, bool compressed = true)
	{
		_columnCount = columnCount;
		_resourceNameSpace = resourceNameSpace;
		_resourceFile = resourceFile;
		_compressed = compressed;
		_disposed = false;
		_assembly = assembly;
	}

	public IEnumerator<string?[]> GetEnumerator()
	{
		// Clean up any previous enumerator before creating a new one
		_currentEnumerator?.Dispose();
		_currentEnumerator = new CsvEnumerator(_assembly, _resourceNameSpace, _resourceFile, _columnCount, _compressed);
		return _currentEnumerator;
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public void Dispose()
	{
		if (!_disposed)
		{
			_currentEnumerator?.Dispose();
			_currentEnumerator = null;
			_disposed = true;
		}
	}
}