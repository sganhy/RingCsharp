using Ring.Data.Enums;
using Ring.Data.Models;
using Ring.Schema.Extensions;
using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Database = Ring.Schema.Models.Schema;

namespace Ring.Data;

public sealed class BulkRetrieve
{
	private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
	private readonly SpanList<RetrieveQuery> _queries;
    private Database? _schema;

    public BulkRetrieve()
    {
        _queries = new SpanList<RetrieveQuery>();
        _schema = null;
    }
	
    internal Database Schema
	{
		get { return _schema; }
		set { _schema = value; }
	}

	/// <summary>
	/// The SimpleQuery method creates an entry in a BulkRetrieve object, associates it
	/// with the specified entry index number, and places a query in that entry.
	/// </summary>
	/// <param name="entryIndex">The index to be associated with the query.</param>
	/// <param name="objectname">The object type of the database records to be retrieved by the simple query.</param>
	public void SimpleQuery(int entryIndex, string objectname)
	{
		// Code size: 105 (0x69) - no virtual call
		if (entryIndex > _queries.Count) ThrowInvalidIndexError(_queries.Count);
		if (entryIndex < _queries.Count) ThrowIndexAlreadyExistError(entryIndex);
		var table = _schema?.GetTable(objectname);
		if (table is null) ThrowInvalidObjectError(objectname);
		_queries.Add(new RetrieveQuery(table, RetrieveQueryType.SimpleQuery, -1));
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowInvalidIndexError(int dataCount) => // Code size: 36 (0x24)
		throw new ArgumentException(string.Format(DefaultCulture, ResourceHelper.GetMessage(ResourceType.BulkRetrieveInvalidIndex), dataCount.ToString(DefaultCulture)));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowIndexAlreadyExistError(int entryIndex) => // Code size: 30 (0x1e) - box operation
		throw new ArgumentException(string.Format(DefaultCulture, ResourceHelper.GetMessage(ResourceType.BulkRetrieveIndexAlreadyExist), entryIndex.ToString(DefaultCulture)));

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowInvalidObjectError(string objectname) => // Code size: 25 (0x19)
	throw new ArgumentException(string.Format(DefaultCulture, ResourceHelper.GetMessage(ResourceType.BulkRetrieveInvalidObject), objectname));
}
