using System;
using System.Data;
using Ring.Data;

namespace Ring.Adapters.PostgreSQL
{
    public sealed class DbDataTableAdapter: IDbDataTable
    {

        private readonly DataTable _dataTable;
        private static readonly string Iso8601Format = @"yyyy-MM-ddTHH:mm:ssZ";

        /// <summary>
        /// Create DbDataTableAdapter from DataTableCollection (take first DataTable) 
        /// </summary>
        /// <param name="dataTable"></param>
        public DbDataTableAdapter(DataTableCollection dataTable)
        {
            if (dataTable != null && dataTable.Count > 0) _dataTable = dataTable[0];
        }
        public void Dispose()
        {
            _dataTable.Dispose();
        }
        public int RowsCount => _dataTable?.Rows.Count ?? 0;

        public string Rows(int index, string fieldName)
        {
            var result = _dataTable.Rows[index][fieldName];
            if (result is DateTime) return ((DateTime) result).ToString(Iso8601Format);
            return result?.ToString();
        }
    }
}
