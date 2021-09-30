namespace Ring.Util.Models
{
    internal sealed class ExcelSheet
    {
        public readonly string FileName;
        public readonly int FirstRowNum;
        public readonly string Id;
        public readonly int LastRowNum;
        public readonly string Name;
        public readonly int Priority;
        public ExcelColumn[] Columns;

        public ExcelSheet(string id, string name, string fileName, ExcelColumn[] columns, int firstRowNum,
            int lastRowNum, int priority)
        {
            Id = id;
            Name = name;
            Columns = columns;
            FirstRowNum = firstRowNum;
            LastRowNum = lastRowNum;
            FileName = fileName;
            Priority = priority;
        }


#if DEBUG
        public override string ToString()
        {
            return Id + "; " + Name;
        }
#endif
    }
}