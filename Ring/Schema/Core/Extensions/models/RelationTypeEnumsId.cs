using Ring.Schema.Enums;

namespace Ring.Schema.Core.Extensions.models
{
    internal struct RelationTypeEnumsId
    {
        internal readonly long Id;
        internal readonly RelationType RelationType;

        public RelationTypeEnumsId(long id, RelationType relationType)
        {
            Id = id;
            RelationType = relationType;
        }

#if DEBUG
        public override string ToString() => Id + ", " + RelationType;

#endif
    }
}
