namespace Ring.Schema.Core.Extensions.models
{
    internal struct RelationTypeEnumsName
    {
        internal readonly string Name;
        internal readonly long Id;

        public RelationTypeEnumsName(string name, long id)
        {
            Id = id;
            Name = name;
        }

#if DEBUG
        public override string ToString() => Name + ", " + Id;

#endif
    }
}
