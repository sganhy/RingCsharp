using Ring.Data.Core;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Models;

namespace Ring.Data.Mappers
{
    internal static class RecordMapper
    {

        /// <summary>
        /// Generate record for @meta_id object
        /// </summary>
        public static Record Map(int id, int schemaId, sbyte objectType, long value)
        {
            var table = Global.Databases.MetaDataIdTable;
            var data = new string[table.Fields.Length];
            var result = new Record(table, data);
            // fast copy without validation 
            data[table.GetFieldIndex(Constants.MetaDataId)] = id.ToString();
            data[table.GetFieldIndex(Constants.MetaDataObjectType)] = objectType.ToString();
            data[table.GetFieldIndex(Constants.MetaDataSchemaId)] = schemaId.ToString();
            data[table.GetFieldIndex(Constants.MetaDataValue)] = value.ToString();
            return result;
        }

        /// <summary>
        /// Generate record for @meta object
        /// </summary>
        public static Record Map(int schemaId, MetaData metaData, bool active)
        {
            var table = Global.Databases.MetaDataTable;
            var data = new string[table.Fields.Length];
            var result = new Record(table, data);
            var dataType = metaData.DataType;

            // for table copy 
            data[table.GetFieldIndex(Constants.MetaDataId)] = metaData.Id;
            data[table.GetFieldIndex(Constants.MetaDataName)] = metaData.Name;
            data[table.GetFieldIndex(Constants.MetaDataSchemaId)] = schemaId.ToString();
            data[table.GetFieldIndex(Constants.MetaDataDescription)] = metaData.Description;
            data[table.GetFieldIndex(Constants.MetaDataRefId)] = metaData.RefId;
            data[table.GetFieldIndex(Constants.MetaDataObjectType)] = metaData.ObjectType.ToString();
            data[table.GetFieldIndex(Constants.MetaDataDataType)] = dataType.ToString();
            data[table.GetFieldIndex(Constants.MetaDataActive)] = active ? bool.TrueString : bool.FalseString;
            data[table.GetFieldIndex(Constants.MetaDataFlags)] = metaData.Flags.ToString();
            data[table.GetFieldIndex(Constants.MetaDataValue)] = metaData.Value;

            return result;
        }

    }
}
