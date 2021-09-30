using System.Collections.Generic;

namespace Ring.Util.Models
{
    internal sealed class ExcelColumn
    {
        public readonly Dictionary<int, string> Cells;
        public readonly int FirstRowNum;
        public readonly int LastRowNum;
        public readonly string Name;

        public ExcelColumn(string name, int firstRowNum, int lastRowNum, Dictionary<int, string> cellsDictionary)
        {
            Cells = cellsDictionary;
            Name = name;
            LastRowNum = lastRowNum;
            FirstRowNum = firstRowNum;
        }

#if DEBUG
        public override string ToString() => Name;

#endif

    }
}