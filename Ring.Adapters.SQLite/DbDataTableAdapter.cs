using System.Data;
using Ring.Data;

namespace Ring.Adapters.SQLite
{
    public sealed class DbDataTableAdapter: IDbDataTable
    {

        private DataTable _dataTable;

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
        public string Rows(int index, string fieldName) => _dataTable.Rows[index][fieldName].ToString();
    }
}
