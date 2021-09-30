using Ring.Schema.Enums;

namespace Ring.Schema.Core.Extensions.models
{
    internal struct EntityTypeEnumsId
    {
        internal readonly sbyte Id;
        internal readonly EntityType EntityType;

        public EntityTypeEnumsId(sbyte id, EntityType entityType)
        {
            Id = id;
            EntityType = entityType;
        }

#if DEBUG
        public override string ToString() => Id + ", " + EntityType;

#endif

    }
}
