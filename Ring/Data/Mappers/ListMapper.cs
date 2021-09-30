using Ring.Data.Enums;
using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Data.Mappers
{
    internal static class ListMapper
    {

        public static List Map(MetaData[] metaData, bool enabled)
        {
            if (metaData == null) return new List();
            var result = new List(ItemType.Record, metaData.Length);
            var schemaObjectTypeId = GetSchemaId(metaData);

            // calcul 
            for (var i = 0; i < metaData.Length; ++i)
            {
                result.AppendItem(RecordMapper.Map(schemaObjectTypeId, metaData[i], enabled));
            }
            return result;
        }

        #region private methods

        private static int GetSchemaId(MetaData[] metaData)
        {
            if (metaData == null) return 0;
            const long schemaTypeId = (long)EntityType.Schema;
            var i = 0;
            while (i < metaData.Length)
            {
                if (metaData[i].ObjectType == schemaTypeId) return int.Parse(metaData[i].Id);
                ++i;
            }
            return 0;
        }

        #endregion 


    }
}
