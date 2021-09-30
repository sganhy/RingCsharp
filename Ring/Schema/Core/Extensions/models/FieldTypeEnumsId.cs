using Ring.Schema.Enums;

namespace Ring.Schema.Core.Extensions.models
{
    internal struct FieldTypeEnumsId
    {
        internal readonly int Id;
        internal readonly FieldType FieldType;

        public FieldTypeEnumsId(int id, FieldType fieldType)
        {
            Id = id;
            FieldType = fieldType;
        }

#if DEBUG
        public override string ToString() => Id + ", " + FieldType;

#endif

    }

}
