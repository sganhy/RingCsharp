using Ring.Data.Enums;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Builders;
using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Database = Ring.Schema.Models.Schema;

namespace Ring.Data;

public readonly struct BulkSave : IBulkSave
{
	private const byte FirstCancelOperationId = (byte)SaveQueryType.FirstCancelOperation;
	private static readonly SaveQuery EmptySaveQuery = new(GetDefaultType(), SaveQueryType.Undefined, GetDefaultDmlBuilder(), Array.Empty<string?>(), 0);
	private readonly BulkSaveInfo _info;

	/// <summary>
	///	 Ctor
	/// </summary>
	internal BulkSave(Database schema) => _info = new BulkSaveInfo(32, schema);
	public BulkSave() => _info = new BulkSaveInfo(4);

	internal SpanList<SaveQuery> Queries => _info.Queries;

	/// <summary>
	/// The CancelRecord method removes one record from the BulkSave object only (not from the database) and 
	/// is used prior to the Save method or in the Form_Save callback.
	/// </summary>
	/// <param name="recordToCancel">The record you want to remove from the BulkSave object</param>
	public void CancelRecord(Record? recordToCancel)
	{
		if (recordToCancel is not null)
		{
			var count = _info.Queries.Count;
			for (var i=0; i<count; ++i)
				if (recordToCancel.Value.EqualTo(_info.Queries[i]))
					ReplaceQueryType(i, _info.Queries[i].Type.CancelOperation());
		}
	}

	/// <summary>
	/// If you provide the optional parameter ObjectType, this method returns the number of records of the specified type 
	/// in a BulkSave object. If you do not provide the object type, this method returns the count of all of the records 
	/// in the BulkSave object.
	/// </summary>
	/// <returns>This method returns an integer value indicating the number of records of the specified type in this object</returns>
	public int CountByType(string? objectType)
	{
		var result = 0;
		if (objectType is not null)
		{
			var count = _info.Queries.Count;
			for (var i = 0; i < count; ++i)
			{
				// may be compare schema too ? 
				var query = _info.Queries[i];
				if ((query.Type == SaveQueryType.InsertRecord || query.Type == SaveQueryType.UpdateRecord || query.Type == SaveQueryType.DeleteRecord) &&
					string.Equals(query.Table.Name, objectType, StringComparison.OrdinalIgnoreCase)) ++result;
			}
		}
		return result;
	}

#pragma warning disable IDE0251 // Make member 'readonly'

	/// <summary>
	/// The DeleteRecord  method is used to delete a record in the database.
	/// </summary>
	/// <param name="record">Specify the record you want to delete from the database.</param>
	public void DeleteRecord(Record record)
	{
		// cannot use DeleteRecordById() coz of @meta objects
		if (record.Table is null) return;
		if (record.IsNew && record.Table.Type == TableType.Business) return;
		_info.Queries.Add(new SaveQuery(record.Table, SaveQueryType.DeleteRecord, _info.Schema.DmlBuilder, record.Data, record.Offset));
	}

	public void DeleteRecordById(string recordType, long id)
	{   /*
		var rcd = new Record { RecordType = recordType };
		rcd.SetField(id);
		if (!rcd.IsNew)
			_data.Add(new BulkSaveQuery(null, BulkSaveType.DeleteRecord, rcd, rcd.Copy(), null));
		*/
	}

	/// <summary>
	/// The GetRecordByIndex method returns the record in the BulkSave object that is associated with the index value you provide.
	/// </summary>
	/// <param name="index">The index value of the record.</param>
	/// <param name="objectType">The index value of the record.</param>
	/// <returns>Returns the record at the specified index</returns>
	public Record? GetRecordByIndex(int index, string objectType)
	{
		int currentIndex = 0;
		if (index >= 0 && index < _info.Queries.Count)
		{
			/*
			var query = _info.Queries[index];
			if ((query.Type == SaveQueryType.InsertRecord || query.Type == SaveQueryType.UpdateRecord || query.Type == SaveQueryType.DeleteRecord) &&
				_data[i].CurrentRecord.RecordType.IndexOf(objectType, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				if (currentIndex == index) return _data[i].RefRecord;
				++currentIndex;
			}
			*/
		}
		return null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void InsertRecord(Record record)
	{
		if (record.Table is null) ThrowRecordUnknownRecordType();
		if (record.Table.Readonly) return; // throw exception here ??
		if (record.Table.Type == TableType.Business) ++_info.IdCount;
		if (record.IsNew) _info.Queries.Add(new SaveQuery(record.Table, SaveQueryType.InsertRecord, _info.Schema.DmlBuilder, record.Data, record.Offset));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void UpdateRecord(Record record)
	{
		if (record.Table is null) ThrowRecordUnknownRecordType();
		if (record.Table.Readonly) return; // throw exception here !!
		if (!record.IsNew) _info.Queries.Add(new SaveQuery(record.Table, SaveQueryType.UpdateRecord, _info.Schema.DmlBuilder, record.Data, record.Offset));
	}

	public readonly override int GetHashCode() => GetHashCode(this);
	public static bool operator == (BulkSave left, BulkSave right) => left.Equals(right);
	public static bool operator != (BulkSave left, BulkSave right) => !(left == right);
	public override bool Equals(object? obj) => obj is BulkSave && Equals(obj);
	public bool Equals(BulkSave other)
	{
		if (_info.Schema.Id == other._info.Schema.Id && _info.IdCount == other._info.IdCount && _info.Queries.Count == other._info.Queries.Count)
		{
			return GetHashCode(this) == GetHashCode(other);
		}
		return false;
	}

	public void Clear()
	{
        // Code size: 93 (0x5d)
        var count = _info.Queries.Count;
		var span = _info.Queries.AsSpan();
		// remove all  references to array
		for (var i = 0; i < count; ++i) span[i] = EmptySaveQuery;
		// reset Queries._count
		_info.Queries.Clear();
		_info.IdCount = 0;
	}

	public void Save()
	{	
	}

	internal void Save(IConnection connection, bool noTransaction=false)
	{
        // Code size: 77 (0x4d)
        var queryCount = _info.Queries.Count;

		if (queryCount == 0) return;

		// generate id
		if (_info.IdCount > 0) GenerateId(connection);

		//TODO if more than 100K multiple transactions
		//TODO throw exception ==> invalid insert into with id==0
		if (queryCount == 1 || noTransaction) SaveWithoutTransactions(connection);
		else if (queryCount > 1) SaveWithTransaction(connection);
		
		// clear bucket 
		Clear();
	}

	#region private methods

	private void ReplaceQueryType(int index, SaveQueryType saveQueryType)
#pragma warning restore IDE0251
	{
		var prevQuery = _info.Queries[index];
		_info.Queries[index] = new SaveQuery(prevQuery.Table, saveQueryType, prevQuery.Builder,
			prevQuery.Data, prevQuery.Offset);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	[DoesNotReturn]
	private static void ThrowRecordUnknownRecordType() => throw new ArgumentException(ResourceHelper.GetMessage(ResourceType.RecordUnkownRecordType));

	private void GenerateId(IConnection connection)
	{ 

	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SaveWithoutTransactions(IConnection connection)
	{
		// Code size: 69 (0x45)
		foreach (var query in _info.Queries.AsReadOnlySpan())
		{
			var type = query.Type;
			var typeId = (byte)type;
			// callvirt instance int64 Ring.Data.IRingConnection::Execute
			if (typeId < FirstCancelOperationId) connection.Execute(query); 
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SaveWithTransaction(IConnection connection)
	{
        // Code size: 91 (0x5b)
        connection.BeginTransaction();
        foreach (var query in _info.Queries.AsReadOnlySpan())
		{
            var type = query.Type;
            var typeId = (byte)type;
			if (typeId < FirstCancelOperationId)
			{
                // callvirt instance int64 Ring.Data.IRingConnection::Execute
                var returnValue = connection.Execute(query);
				if (returnValue < 0L)
				{
					connection.Rollback();
					return;
				}
            }
        }
		connection.Commit();
    }

	private static Table GetDefaultType()
	{
		var metaTable = new Meta(-1, (byte)EntityType.Table, 0, (int)TableType.Undefined, 0L, string.Empty, null, null, true);
		var metaArray = new Meta[] { new(0, (byte)EntityType.Field, 0, 0, 0L, string.Empty, null, null, true) };
		return metaTable.ToTable(new ReadOnlySpan<Meta>(metaArray), PhysicalType.Undefined, GetDefaultDdlBuilder(), string.Empty, -1) !; // cannot be null here!!
	}

	private static IDmlBuilder GetDefaultDmlBuilder() => new Util.Builders.PostgreSQL.DmlBuilder();
    private static IDdlBuilder GetDefaultDdlBuilder() => new Util.Builders.PostgreSQL.DdlBuilder();

    private static int GetHashCode(in BulkSave bulkSave)
	{
		var span = bulkSave._info.Queries.AsReadOnlySpan();
		var hash = 0;
		foreach (var query in span) hash += SaveQueryExtensions.GetHashCode(query);
		return hash;
	}

	#endregion
}
