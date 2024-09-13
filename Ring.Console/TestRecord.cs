using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ring.Console
{
    public record TestRecord
    {
        internal bool NotNull;
        internal int Id;

        public TestRecord(int id, bool notNull)
        {
            Id = id;
            NotNull = notNull;
        }

    }
}
