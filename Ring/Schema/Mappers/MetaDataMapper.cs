using Ring.Data;
using Ring.Schema.Models;

namespace Ring.Schema.Mappers
{
    /// <summary>
    /// Map an objects to MetaData object (see GetInstance() overload)
    /// </summary>
    internal static class MetaDataMapper
    {
        /// <summary>
        /// Get instance from record 
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public static MetaData Map(Record record)
        {
            long lFlags;
            sbyte objectType;
            int dataType;

            record.GetField(Constants.MetaDataObjectType, out objectType);
            record.GetField(Constants.MetaDataDataType, out dataType);
            record.GetField(Constants.MetaDataFlags, out lFlags);
            // IMPORTANT - for table RefId=Id
            return new MetaData
            {
                Id = record.GetField(Constants.MetaDataId),
                Name = record.GetField(Constants.MetaDataName),
                RefId = objectType == Constants.EntityTypeIdTable
                    ? record.GetField(Constants.MetaDataId)
                    : record.GetField(Constants.MetaDataRefId),
                ObjectType = objectType,
                DataType = dataType,
                Flags = lFlags,
                LineNumber = 0L,
                Value = record.GetField(Constants.MetaDataValue),
                Description = record.GetField(Constants.MetaDataDescription)
            };
        }


    }

}
