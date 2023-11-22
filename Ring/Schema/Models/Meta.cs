namespace Ring.Schema.Models;

// Cannot be a struct due to extensions
//              removed SchemaId field
internal sealed class Meta
{
    internal int Id;
    internal byte ObjectType;
    internal int ReferenceId;
    internal int DataType;
    internal long Flags;
    internal string Name; // name of entity
    internal string? Description;        // late loading 
    internal string? Value;
    internal bool Active = true;
    
    public Meta() => Name = string.Empty;
    public Meta(string name) => Name = name;

#if DEBUG
    public override string ToString() => Name ?? string.Empty;
#endif

}