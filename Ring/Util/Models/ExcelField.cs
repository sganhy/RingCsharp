using Ring.Schema.Models;

namespace Ring.Util.Models
{
    /// <summary>
    ///     Matching between Excel column and field object
    /// </summary>
    internal sealed class ExcelField
    {
        public readonly ExcelColumn Column;
        public readonly Field Field;
        public readonly int HeaderRowId;
        public readonly bool Key;
        public readonly Relation Relation;

        public ExcelField(Field field, Relation relation, ExcelColumn column, int headerRowId, bool key)
        {
            Field = field;
            Column = column;
            Relation = relation;
            HeaderRowId = headerRowId;
            Key = key;
        }

#if DEBUG
        public override string ToString() => Field?.Name + "; " + Column?.Name + (Key? "; [KEY]" :string.Empty);
#endif
    }
}