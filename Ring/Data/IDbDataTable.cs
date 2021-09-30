using System;

namespace Ring.Data
{
    public interface IDbDataTable : IDisposable
    {
        int RowsCount { get; }

        /// <summary>
        /// Date/Time should be provide to ISO 8601 ('yyyy-MM-ddTHH:mm:ssZ')
        /// </summary>
        /// <param name="index"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        string Rows(int index, string fieldName);
    }
}
