namespace Ring.Schema.Models
{
    internal sealed class Index : BaseEntity
    {
        internal readonly bool Bitmap;
        internal readonly BaseEntity[] Fields;
        internal readonly int TableId;
        internal readonly bool Unique;

        /// <summary>
        ///     Ctor
        /// </summary>
        public Index(int id, string name, string description, BaseEntity[] fields, int tableId,
            bool unique,
            bool bitmap,
            bool active, bool baseline)
            : base(id, name, description, active, baseline)
        {
            Unique = unique;
            Fields = fields;
            Bitmap = bitmap;
            TableId = tableId;
        }
    }
}