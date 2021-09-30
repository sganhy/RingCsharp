namespace Ring.Schema.Models
{
    internal class MetaDataId
    {
        internal readonly int Id;
        internal readonly int ObjectType;
        internal readonly long Value;


        public MetaDataId(int id, int objectType, long value)
        {
            Id = id;
            ObjectType = objectType;
            Value = value;
        }
    }
}