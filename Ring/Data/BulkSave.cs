using Ring.Data.Models;
using Ring.Schema;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Globalization;
using Database = Ring.Schema.Models.Schema;

namespace Ring.Data;

public ref struct BulkSave
{
    private static readonly CultureInfo DefaultCulture = CultureInfo.InvariantCulture;
    private static readonly Database DefaultSchema = 
            Meta.GetEmptySchema(new Meta(-1, (byte) EntityType.Schema, 0,0, 0L, string.Empty, null,null,true), DatabaseProvider.Undefined);
    private readonly SpanList<SaveQuery> _queries;
    private Database _schema;

    internal BulkSave(Database schema)
    {
        _queries = new SpanList<SaveQuery>(32); // schema upgrade constructor
        _schema = schema;
    }
    public BulkSave()
    {
        _queries = new SpanList<SaveQuery>(16); // min bucket size = 16
        _schema = DefaultSchema;
    }

    internal readonly SpanList<SaveQuery> Queries => _queries;

    public void CancelRecord(Record? recordToCancel)
    {
        if (recordToCancel == null) return;
        var rcdList = FindAllQueriesByRecord(recordToCancel.Value);
        /*
        for (var i = 0; i < rcdList.Length; ++i)
            rcdList[i].UpdateCurrentRecord();
        */
    }

    #region private methods

    private readonly ReadOnlySpan<SaveQuery> FindAllQueriesByRecord(Record record)
    {
        var result = new List<SaveQuery>();
        for (var i = _queries.Count - 1; i >= 0; --i) if (record.Equals(_queries[i])) result.Add(_queries[i]);
        return new ReadOnlySpan<SaveQuery>(result.ToArray());
    }

    #endregion 
}
