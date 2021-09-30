using Ring.Data.Enums;
using Ring.Schema.Models;

namespace Ring.Data.Models
{
    internal sealed class BulkSort
    {
        internal readonly Field Field;
        internal readonly SortOrderType Type;

        /// <summary>
        /// Ctor
        /// </summary>
        public BulkSort(Field field, SortOrderType orderType)
        {
            Field = field;
            Type = orderType;
        }

    }
}
