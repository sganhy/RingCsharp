namespace Ring.Data.Models;

internal sealed class PageInfo
{
    internal readonly int PageSize;
    internal readonly int PageNumber;
    internal int Count;

    internal PageInfo(int pageSize, int pageNumber)
    {
        PageSize = pageSize;
        PageNumber = pageNumber;
    }

}
