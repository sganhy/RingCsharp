using Ring.Schema.Models;

namespace Ring.Data.Models;

internal readonly struct RetrieveSort
{
    internal readonly Field Field;
    internal readonly SortOrder Type;

    public RetrieveSort(Field field, SortOrder type)
    {
        Field = field;
        Type = type;
    }
}
