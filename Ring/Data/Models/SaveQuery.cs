using Ring.Schema.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ring.Data.Models;

public struct SaveQuery
{
    internal readonly Table Table;
    internal readonly Table Type;
}
