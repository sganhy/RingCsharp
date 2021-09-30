namespace Ring.Schema.Models
{
    internal sealed class TableSpace : BaseEntity
    {
        internal readonly string FileName;
        internal readonly bool IsIndex;
        internal readonly bool IsReadonly;
        internal readonly bool IsTable;
        internal readonly int SchemaId;
        internal readonly string TableName;

        internal TableSpace(int id, string name, string description, bool isIndex, bool isTable, string tableName,
            string fileName, int schemaId,
            bool isReadonly, bool active, bool baseline)
            : base(id, name, description, active, baseline)
        {
            IsIndex = isIndex;
            IsTable = isTable;
            IsReadonly = isReadonly;
            TableName = tableName;
            FileName = fileName;
            SchemaId = schemaId;
        }
    }
}