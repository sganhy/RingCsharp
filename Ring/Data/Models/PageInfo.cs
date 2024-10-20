using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
