using Ring.Schema.Models;

namespace Ring.Data.Models;

internal sealed class RetrieveSort
{
    internal readonly Field Field;
    internal readonly SortOrder Type;
    internal RetrieveSort? Next;

    public RetrieveSort(Field field, SortOrder type)
    {
        Field = field;
        Type = type;
        Next = null;
    }
}
