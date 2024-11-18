using Ring.Data.Enums;
using Ring.Data.Extensions;
using Ring.Data.Models;
using Ring.Schema;
using Ring.Schema.Enums;
using Ring.Util.Enums;
using Ring.Util.Helpers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Database = Ring.Schema.Models.Schema;

namespace Ring.Data;

public struct BulkSave : IBulkSave 
{
    private static readonly Database DefaultSchema = 
            Meta.GetEmptySchema(new Meta(-1, (byte) EntityType.Schema, 0,0, 0L, string.Empty, null,null,true), DatabaseProvider.Undefined);
    private SpanList<SaveQuery> _queries;
    private Database _schema;

    internal BulkSave(Database schema)
    {
        _queries = new SpanList<SaveQuery>(32); // schema upgrade constructor
        _schema = schema;
    }
    public BulkSave()
    {
        _queries = new SpanList<SaveQuery>(4); // min bucket size = 4
        _schema = DefaultSchema;
    }

    internal readonly SpanList<SaveQuery> Queries => _queries;

    /// <summary>
    /// The CancelRecord method removes one record from the BulkSave object only (not from the database) and 
    /// is used prior to the Save method or in the Form_Save callback.
    /// </summary>
    /// <param name="recordToCancel">The record you want to remove from the BulkSave object</param>
    public void CancelRecord(Record? recordToCancel)
    {
        if (recordToCancel != null)
        {
            var count = _queries.Count;
            for (var i=0; i<count; ++i)
                if (recordToCancel.Equals(_queries[i]))
                    ReplaceQueryType(i, _queries[i].Type.CancelOperation());
        }
    }

    /// <summary>
    /// If you provide the optional parameter ObjectType, this method returns the number of records of the specified type 
    /// in a BulkSave object. If you do not provide the object type, this method returns the count of all of the records 
    /// in the BulkSave object.
    /// </summary>
    /// <returns>This method returns an integer value indicating the number of records of the specified type in this object</returns>
    public readonly int CountByType(string? objectType)
    {
        var result = 0;
        if (objectType!=null)
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
        if (record.Table == null) return;
        if (record.IsNew && record.Table.Type == TableType.Business) return;
        _queries.Add(new SaveQuery(record.Table, SaveQueryType.DeleteRecord, _schema.DmlBuiler, record.Data, record.Offset));
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

    public void InsertRecord(Record record)
    {
        if (record.Table == null) ThrowRecordUnknownRecordType();
        if (record.Table.Readonly) return; // throw exception here !!
        if (record.IsNew) _queries.Add(new SaveQuery(record.Table, SaveQueryType.InsertRecord, _schema.DmlBuiler, record.Data, record.Offset));
    }

    public void UpdateRecord(Record record)
    {
        if (record.Table == null) ThrowRecordUnknownRecordType();
        if (record.Table.Readonly) return; // throw exception here !!
        if (!record.IsNew) _queries.Add(new SaveQuery(record.Table, SaveQueryType.UpdateRecord, _schema.DmlBuiler, record.Data, record.Offset));
    }

    public override int GetHashCode()
    {
        var span = _queries.AsSpan();
        var hash = 0;
        foreach (var query in span) hash += SaveQueryExtensions.GetHashCode(query);
        return hash;
    }
    public static bool operator ==(BulkSave left, BulkSave right) => left.Equals(right);
    public static bool operator !=(BulkSave left, BulkSave right) => !(left == right);
    public override readonly bool Equals(object? obj) => obj is Record record && Equals(record);
    public bool Equals(BulkSave other)
    {
        if (ReferenceEquals(_schema, other._schema))
        {
            return GetHashCode()== other.GetHashCode();
        }
        return false;
    }

    #region private methods

    private void ReplaceQueryType(int index, SaveQueryType saveQueryType)
#pragma warning restore IDE0251
    {
        var prevQuery = _queries[index];
        var newQuery = new SaveQuery(prevQuery.Table, saveQueryType, prevQuery.Builder, prevQuery.Data, prevQuery.Offset);
        _queries[index] = newQuery;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    [DoesNotReturn]
    private static void ThrowRecordUnknownRecordType() => 
        throw new ArgumentException(ResourceHelper.GetErrorMessage(ResourceType.RecordUnkownRecordType));


    #endregion
}
