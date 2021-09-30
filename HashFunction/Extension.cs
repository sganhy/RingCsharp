using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication8
{
    public static class Extension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetField(this Table table, string name)
        {
            int indexerLeft = 0, indexerRigth = table.Fields.Length - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
                var indexerCompare = string.CompareOrdinal(name, table.Fields[indexerMiddle]);
                if (indexerCompare == 0) return table.Fields[indexerMiddle];
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return null;
        }

        public static string GetField2(this Table table, string name)
        {
            int indexerLeft = 0, indexerRigth = table.Fields.Length - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
                var indexerCompare = string.CompareOrdinal(name, table.Fields[indexerMiddle]);
                if (indexerCompare == 0) return table.Fields[indexerMiddle];
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return null;
        }


	    public static string GetField3(this Table table, string name)
	    {
		    int indexerLeft = 0, indexerRigth = table.Fields.Length - 1;
		    while (indexerLeft <= indexerRigth)
		    {
			    var indexerMiddle = indexerLeft + indexerRigth;
			    indexerMiddle /= 2;   // indexerMiddle <-- indexerMiddle /2 
			    var indexerCompare = string.CompareOrdinal(name, table.Fields[indexerMiddle]);
			    if (indexerCompare == 0) return table.Fields[indexerMiddle];
			    if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
			    else indexerRigth = indexerMiddle - 1;
		    }
		    return null;
	    }


	}
}
