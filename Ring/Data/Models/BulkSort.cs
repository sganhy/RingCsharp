using Ring.Schema.Models;

namespace Ring.Data.Models;

internal sealed class BulkSort
{
    internal readonly Field Field;
    internal readonly SortOrder Type;
    internal BulkSort? Next;

    public BulkSort(Field field, SortOrder type)
    {
        Field = field;
        Type = type;
        Next = null;
    }
}
