using Ring.Schema.Enums;

namespace Ring.Schema.Mappers
{
    internal static class Constants
    {
        // metadata property name - should be in lower case
        internal static readonly string MetaDataId = Builders.Constants.MetaDataId;
        internal static readonly string MetaDataName = Builders.Constants.MetaDataName;
        internal static readonly string MetaDataDescription = Builders.Constants.MetaDataDescription;
        internal static readonly string MetaDataRefId = Builders.Constants.MetaDataRefId;
        internal static readonly string MetaDataObjectType = Builders.Constants.MetaDataObjectType;
        internal static readonly string MetaDataDataType = Builders.Constants.MetaDataDataType;
        internal static readonly string MetaDataFlags = Builders.Constants.MetaDataFlags;
        internal static readonly string MetaDataValue = Builders.Constants.MetaDataValue;

        // entity sourceType id
        internal const long EntityTypeIdTable = (long)EntityType.Table;
    }
}
