namespace Ring.Schema.Models
{
    // Cannot be a struct due to extensions
    internal sealed class MetaData
    {
        internal int DataType;
        internal string Description; // late loading 
        internal long Flags;
        internal string Id;
        internal long LineNumber;
        internal string Name; // name of entity
        internal sbyte ObjectType;
        internal string RefId; // 
        internal string Value;


#if DEBUG
        
        public override string ToString() => Name ?? string.Empty;

#endif
    }
}