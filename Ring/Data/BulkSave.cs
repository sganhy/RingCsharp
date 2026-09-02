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

public sealed class BulkSave : IBulkSave
{
	private static readonly SaveQuery EmptySaveQuery = new(GetDefaultType(), SaveQueryType.Undefined, Array.Empty<string?>(), 0);
	private SpanList<SaveQuery> _queries; // cannot set _queries as readonly!
	private readonly Database _schema;
	private int _idCount; // count of insert queries for each table, used to generate id before save

	internal SpanList<SaveQuery> Queries => _queries;

	internal BulkSave(Database schema)
	{
		// Code size: 33 (0x21)
		_queries = new SpanList<SaveQuery>(16); // min bucket size = 16
		_schema = schema;
		_idCount = 0;
	}

	/// <summary>
	/// The CancelRecord method removes one record from the BulkSave object only (not from the database) and 
	/// is used prior to the Save method or in the Form_Save callback.
	/// </summary>
	/// <param name="recordToCancel">The record you want to remove from the BulkSave object</param>
	public void CancelRecord(Record? recordToCancel)
	{
		if (recordToCancel is not null)
		{
			var count = _queries.Count;
			for (var i=0; i<count; ++i)
				if (recordToCancel.Value.EqualTo(_queries[i]))
					ReplaceQueryType(i, _queries[i].Type.CancelOperation());
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
			var count = _queries.Count;
			for (var i = 0; i < count; ++i)
			{
				// may be compare schema too ? 
				var query = _queries[i];
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
		_queries.Add(new SaveQuery(record.Table, SaveQueryType.DeleteRecord, record.Data, record.Offset));
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
		if (index >= 0 && index < _queries.Count)
		{
			/*
			var query = _queries[index];
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
		if (record.Table.Type == TableType.Business) ++_idCount;
		if (record.IsNew) _queries.Add(new SaveQuery(record.Table, SaveQueryType.InsertRecord, record.Data, record.Offset));
	}

	internal void ForceInsert(Record record) =>	_queries.Add(new SaveQuery(record.Table, SaveQueryType.InsertRecord, record.Data, record.Offset));

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void UpdateRecord(Record record)
	{
		if (record.Table is null) ThrowRecordUnknownRecordType();
		if (record.Table.Readonly) return; // throw exception here !!
		if (!record.IsNew) _queries.Add(new SaveQuery(record.Table, SaveQueryType.UpdateRecord, record.Data, record.Offset));
	}

	public override int GetHashCode() => this.Hash();
	public static bool operator ==(BulkSave left, BulkSave right) => left.Equals(right);
	public static bool operator !=(BulkSave left, BulkSave right) => !left.Equals(right);
	public override bool Equals(object? obj) => obj is BulkSave bulkSave && Equals(bulkSave);
	public bool Equals(BulkSave? other) => other is not null
		&& _schema.Id == other._schema.Id
		&& _queries.Count == other._queries.Count
		&& this.Hash() == other.Hash(); // Code size: 68 (0x44)

	public void Clear()
	{
        // Code size: 93 (0x5d)
        var count = _queries.Count;
		var span = _queries.AsSpan();
		// remove all  references to array
		for (var i = 0; i < count; ++i) span[i] = EmptySaveQuery;
		// reset Queries._count
		_queries.Clear();
	}

	public void Save()
	{	
	}

	internal void Save(IConnection connection, bool noTransaction=false)
	{
		// Code size: 77 (0x4d)
		var queryCount = _queries.Count;

		if (queryCount == 0) return;

		// generate id
		if (_idCount > 0) GenerateId(connection);

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
		var prevQuery = _queries[index];
		_queries[index] = new SaveQuery(prevQuery.Table, saveQueryType, prevQuery.Data, prevQuery.Offset);
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
		var builder = _schema.DmlBuilder;
		var encoding = connection.ClientEncoding;

		foreach (var query in _queries.AsReadOnlySpan())
		{
			// callvirt instance int64 Ring.Data.IRingConnection::Execute
			var sql = query.ToSql(builder);
			if (sql is not null)
			{
				var byteCount = encoding.GetByteCount(sql);
				//if (typeId < FirstCancelOperationId) connection.Execute(query); 
				var error  = connection.Execute(query, sql, byteCount);
				if (error is not null)
				{
					int oi = 0;
					++oi;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void SaveWithTransaction(IConnection connection)
	{
        // Code size: 91 (0x5b)
        connection.BeginTransaction();
        foreach (var query in _queries.AsReadOnlySpan())
		{
            var type = query.Type;
            var typeId = (byte)type;
			//if (typeId < FirstCancelOperationId)
			{
                // callvirt instance int64 Ring.Data.IRingConnection::Execute
				 //var resull = connection.Execute(query, );
				/*var returnValue = connection.Execute(query);
				if (returnValue < 0L)
				{
					connection.Rollback();
					return;
				}
				*/
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


	
	#endregion
}
