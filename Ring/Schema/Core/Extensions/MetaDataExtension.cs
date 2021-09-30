using Ring.Data;
using Ring.Data.Core;
using Ring.Schema.Core.Extensions.models;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System;

namespace Ring.Schema.Core.Extensions
{
    internal static class MetaDataExtension
    {
        // enum caches to avoid boxing operations
        private static readonly EntityTypeEnumsId[] EntityTypeEnumsId = GetEntityTypeId();
        private const EntityType DefaultEntityType = EntityType.NotDefined;
        private static readonly FieldTypeEnumsId[] FieldTypeEnumsId = GetFieldTypeId();
        private const FieldType DefaultFieldType = FieldType.NotDefined;
        private static readonly RelationTypeEnumsId[] RelationTypeEnumsId = GetRelationTypeId();
        private const RelationType DefaultRelationType = RelationType.NotDefined;
        private static readonly RelationTypeEnumsName[] RelationTypeEnumsName = GetRelationTypeName(); // <type.ToString().Trim().ToLower(), typeId>

        public static bool IsTable(this MetaData metaData) => metaData.ObjectType == Constants.TableId;
        public static bool IsSchema(this MetaData metaData) => metaData.ObjectType == Constants.SchemaId;
        public static bool IsField(this MetaData metaData) => metaData.ObjectType == Constants.FieldId;
        public static bool IsIndex(this MetaData metaData) => metaData.ObjectType == Constants.IndexId;
        public static bool IsLexicon(this MetaData metaData) => metaData.ObjectType == Constants.LexiconId;
        public static bool IsLanguage(this MetaData metaData) => metaData.ObjectType == Constants.LanguageId;
        public static bool IsRelation(this MetaData metaData) => metaData.ObjectType == Constants.RelationId;
        public static bool IsService(this MetaData metaData) => metaData.ObjectType == Constants.ServiceId;
        public static bool IsSequence(this MetaData metaData) => metaData.ObjectType == Constants.SequenceId;
        public static bool IsTablespace(this MetaData metaData) => metaData.ObjectType == Constants.TablespaceId;

        public static EntityType GetEntityType(this MetaData meta)
        {
            int indexerLeft = 0, indexerRigth = EntityTypeEnumsId.Length - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
                var indexerCompare = meta.ObjectType - EntityTypeEnumsId[indexerMiddle].Id;
                if (indexerCompare == 0) return EntityTypeEnumsId[indexerMiddle].EntityType;
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return DefaultEntityType;
        }
        public static bool IsEnabled(this MetaData meta) => ReadFlag(meta.Flags, Constants.EntityEnabledBitPosition);
        public static bool IsBaselined(this MetaData meta) => ReadFlag(meta.Flags, Constants.EntityBaselinedBitPosition);
        public static void SetBaseline(this MetaData meta, bool baselined) => WriteFlag(ref meta.Flags, Constants.EntityBaselinedBitPosition, baselined);
        public static void SetEnabled(this MetaData meta, bool enabled) => WriteFlag(ref meta.Flags, Constants.EntityEnabledBitPosition, enabled);

        #region sequence methods  
        public static void SetSequenceCacheId(this MetaData meta, bool cacheId) =>
            WriteFlag(ref meta.Flags, Constants.TableCacheIdBitPosition, cacheId);

        #endregion

        #region field methods  
        // read 
        public static int GetFieldSize(this MetaData meta) => (int)((meta.Flags >> (Constants.FieldFirstBitPositionSize - 1)) & int.MaxValue);
        public static FieldType GetFieldType(this MetaData meta)
        {
            int indexerLeft = 0, indexerRigth = FieldTypeEnumsId.Length - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2
                var indexerCompare = meta.DataType - FieldTypeEnumsId[indexerMiddle].Id;
                if (indexerCompare == 0) return FieldTypeEnumsId[indexerMiddle].FieldType;
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return DefaultFieldType;
        }
        public static bool IsFieldNotNull(this MetaData meta) => ReadFlag(meta.Flags, Constants.FieldNotNullBitPosition);
        public static bool IsFieldCaseSensitif(this MetaData meta) => ReadFlag(meta.Flags, Constants.FieldCaseSensitifBitPosition);
        public static bool IsFieldMultilingual(this MetaData meta) => ReadFlag(meta.Flags, Constants.FieldMultilingualBitPosition);

        // write 
        public static void SetFieldSize(this MetaData meta, long size)
        {
            var temp = ~((long)int.MaxValue << (Constants.FieldFirstBitPositionSize - 1));
            temp = meta.Flags & temp; // reset size to 0
            meta.Flags = temp + (size << Constants.FieldFirstBitPositionSize - 1);
        }
        public static void SetFieldNotNull(this MetaData meta, bool notNull) => WriteFlag(ref meta.Flags, Constants.FieldNotNullBitPosition, notNull);
        public static void SetFieldCaseSensitif(this MetaData meta, bool caseSensitive) => WriteFlag(ref meta.Flags, Constants.FieldCaseSensitifBitPosition, caseSensitive);
        public static void SetFieldMultilingual(this MetaData meta, bool multilingual) => WriteFlag(ref meta.Flags, Constants.FieldMultilingualBitPosition, multilingual);

		#endregion

		#region relation methods  

	    public static bool IsRelationConstraint(this MetaData meta) => ReadFlag(meta.Flags, Constants.RelationConstraintBitPosition);
		public static bool IsRelationNotNull(this MetaData meta) => ReadFlag(meta.Flags, Constants.RelationNotNullBitPosition);
	    public static void SetRelationdNotNull(this MetaData meta, bool notNull) => WriteFlag(ref meta.Flags, Constants.RelationNotNullBitPosition, notNull);
	    public static void SetRelationConstraint(this MetaData meta, bool notNull) => WriteFlag(ref meta.Flags, Constants.RelationConstraintBitPosition, notNull);
		internal static RelationType GetRelationType(this MetaData meta)
        {
            var typeId = (meta.Flags >> Constants.RelationFirstBitPositionType) & byte.MaxValue;
            int indexerLeft = 0, indexerRigth = RelationTypeEnumsId.Length - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2
                var indexerCompare = typeId - RelationTypeEnumsId[indexerMiddle].Id;
                if (indexerCompare == 0) return RelationTypeEnumsId[indexerMiddle].RelationType;
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return DefaultRelationType;
        }
        public static void SetRelationType(this MetaData meta, string typeName)
        {
            if (typeName == null) return;
			var prevFlags = meta.Flags & int.MaxValue;
            var typeId = (long)DefaultRelationType;
            var type = typeName.ToLower().Trim();
            // search typeId
            int indexerLeft = 0, indexerRigth = RelationTypeEnumsName.Length - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2
                var indexerCompare = string.CompareOrdinal(type, RelationTypeEnumsName[indexerMiddle].Name);
                if (indexerCompare == 0)
                {
                    typeId = RelationTypeEnumsName[indexerMiddle].Id;
                    break;
                }
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            meta.Flags = prevFlags + (typeId <<Constants.RelationFirstBitPositionType);
        }
        internal static void SetRelationType(this MetaData meta, RelationType type)
        {
            var prevFlags = meta.Flags & int.MaxValue;
	        var typeId = (long) type;
			meta.Flags = prevFlags + (typeId << Constants.RelationFirstBitPositionType);
        }

        #endregion

        #region index methods  
        public static void SetIndexUnique(this MetaData meta, bool unique) => WriteFlag(ref meta.Flags, Constants.IndexUniqueBitPosition, unique);

        public static bool IsIndexUnique(this MetaData meta) => ReadFlag(meta.Flags, Constants.IndexUniqueBitPosition);

        public static bool IsIndexBitmap(this MetaData meta) => ReadFlag(meta.Flags, Constants.IndexBitmapBitPosition);

        #endregion

        #region tablespace methods  
        public static void SetTablespaceTable(this MetaData meta, bool readonlyValue) => WriteFlag(ref meta.Flags, Constants.TableSpaceTableBitPosition, readonlyValue);
        public static void SetTablespaceIndex(this MetaData meta, bool readonlyValue) => WriteFlag(ref meta.Flags, Constants.TableSpaceIndexBitPosition, readonlyValue);
        public static void SetTableSpaceReadonly(this MetaData meta, bool readonlyValue) => WriteFlag(ref meta.Flags, Constants.TableSpaceReadonlyBitPosition, readonlyValue);
        public static bool IsTablespaceTable(this MetaData meta) => ReadFlag(meta.Flags, Constants.TableSpaceTableBitPosition);
        public static bool IsTablespaceIndex(this MetaData meta) => ReadFlag(meta.Flags, Constants.TableSpaceIndexBitPosition);
        public static bool IsTableSpaceReadonly(this MetaData meta) => ReadFlag(meta.Flags, Constants.TableSpaceReadonlyBitPosition);

        #endregion

        #region table methods  

        public static void SetReadonly(this MetaData meta, bool readonlyValue) => WriteFlag(ref meta.Flags, Constants.TableReadonlyBitPosition, readonlyValue);
        public static void SetTableCached(this MetaData meta, bool cached) => WriteFlag(ref meta.Flags, Constants.TableChachedBitPosition, cached);

        public static void SetTableCacheId(this MetaData meta, bool cacheId) =>
                        WriteFlag(ref meta.Flags, Constants.TableCacheIdBitPosition, cacheId);
        public static void SetTableCacheId(this MetaData meta, CacheId cacheId) =>
                        WriteFlag(ref meta.Flags, Constants.TableCacheIdBitPosition, cacheId.ReservedRange != Constants.DisabledCacheId);

        public static bool IsTableReadonly(this MetaData meta) => ReadFlag(meta.Flags, Constants.TableReadonlyBitPosition);

        public static bool IsTableCached(this MetaData meta) => ReadFlag(meta.Flags, Constants.TableChachedBitPosition);

        public static CacheId GetCacheId(this MetaData meta) =>
            ReadFlag(meta.Flags, Constants.TableCacheIdBitPosition) ?
                new CacheId(new object(), 0L, 0L, Constants.MinimumCacheId) :
                new CacheId(new object(), 0L, 0L, Constants.DisabledCacheId);

        #endregion

        #region schema methods  
        public static void SetSchemaAsNative(this MetaData meta) => WriteFlag(ref meta.Flags, Constants.SchemaNativeBitPosition, true);
        public static void SetSchemaAsClfy(this MetaData meta) => WriteFlag(ref meta.Flags, Constants.SchemaClfyBitPosition, true);
        private static bool IsSchemaClfy(this MetaData meta) => ReadFlag(meta.Flags, Constants.SchemaClfyBitPosition);
        private static bool IsSchemaNative(this MetaData meta) => ReadFlag(meta.Flags, Constants.SchemaNativeBitPosition);
        public static SchemaSourceType GetSchemaType(this MetaData metaData)
        {
            if (metaData.IsSchemaClfy()) return SchemaSourceType.ClfyDataBase;
            if (metaData.IsSchemaNative()) return SchemaSourceType.NativeDataBase;
            return SchemaSourceType.NotDefined;
        }

        #endregion

        #region entity methods 
        public static string GetEntityDescription(this MetaData meta) => meta.Description ?? Constants.DefaultEntityDescription;
        public static string GetEntityName(this MetaData meta) => meta.Name ?? Constants.DefaultEntityName;

        /// <summary>
        ///     Get description from load type and IMetaData interface
        /// </summary>
        /// <param name="loadType">Type of load</param>
        /// <param name="meta">Meta data</param>
        /// <returns></returns>
        public static string GetEntityDescription(this MetaData meta, SchemaLoadType loadType) =>
            string.IsNullOrEmpty(meta.Description) || loadType != SchemaLoadType.Full
            ? Constants.DefaultEntityDescription
            : meta.Description;

        #endregion

        #region miscelanous

        /// <summary>
        /// Fecth data from @meta table
        /// </summary>
        internal static List GetMetaDataList(IDbConnection connection, int schemaId, EntityType? objectType, int? referenceId, int? id)
        {
            List result;
            using (var br = new BulkRetrieve { Schema = Global.Databases.MetaSchema })
            {
                br.SimpleQuery(0, Global.Databases.MetaSchema.Name);
                br.AppendFilter(0, Constants.MetaDataSchemaId, OperationType.Equal, schemaId);
                if (id != null) br.AppendFilter(0, Constants.MetaDataId, OperationType.Equal, (int)id);
                if (objectType != null) br.AppendFilter(0, Constants.MetaDataObjectType, OperationType.Equal, (int)objectType);
                if (referenceId != null) br.AppendFilter(0, Constants.MetaDataRefId, OperationType.Equal, (int)referenceId);
                br.RetrieveRecords(connection);
                result = br.GetRecordList(0);
            }
            return result;
        }

        /// <summary>
        /// Fecth data from @meta_id table
        /// </summary>
        internal static List GetMetaDataIdList(IDbConnection connection, int schemaId, EntityType objectType, int id)
        {
            List result;
            using (var br = new BulkRetrieve { Schema = Global.Databases.MetaSchema })
            {
                br.SimpleQuery(0, Global.Databases.MetaDataIdTable.Name);
                br.AppendFilter(0, Constants.MetaDataSchemaId, OperationType.Equal, schemaId);
                br.AppendFilter(0, Constants.MetaDataId, OperationType.Equal, id);
                br.AppendFilter(0, Constants.MetaDataObjectType, OperationType.Equal, (int)objectType);
                br.RetrieveRecords(connection);
                result = br.GetRecordList(0);
            }
            return result;
        }

        /// <summary>
        /// Delete @meta record list 
        /// </summary>
        internal static void DeleteMetaDataList(IDbConnection connection, List list)
        {
            if (list.Count <= 0) return;
	        var bs = new BulkSave {Schema = Global.Databases.MetaSchema};
            for (var i = 0; i < list.Count; ++i) bs.DeleteRecord(list[i]);
            bs.Save(connection);
        }

        #endregion

        #region private methods 

        /// <summary>
        /// Read bit value from binary variable
        /// </summary>
        /// <param name="flags">binary zone to read</param>
        /// <param name="bit">bit position</param>
        /// <returns></returns>
        private static bool ReadFlag(long flags, int bit) => ((flags >> (bit - 1)) & 1L) > 0L;

        /// <summary>
        /// Write a bit to a binary variable
        /// </summary>
        /// <param name="flags">binary element where to write</param>
        /// <param name="bit">bit position</param>
        /// <param name="value">set 0 or 1</param>
        private static void WriteFlag(ref long flags, int bit, bool value)
        {
            var mask = bit < 64 && bit > 0 ? 1L << (bit - 1) : 0L;
            if (value) flags = flags | mask;
            else flags = flags & ~mask;
        }

        /// <summary>
        /// Load EntityType enum cache  
        /// </summary>
        /// <returns></returns>
        private static EntityTypeEnumsId[] GetEntityTypeId()
        {
            var entityTypeEnums = (EntityType[])Enum.GetValues(typeof(EntityType));
            var result = new EntityTypeEnumsId[entityTypeEnums.Length];

            for (var i = 0; i < entityTypeEnums.Length; ++i)
                result[i] = new EntityTypeEnumsId((sbyte)entityTypeEnums[i], entityTypeEnums[i]);

            Array.Sort(result, (x, y) => x.Id - y.Id);
            return result;
        }

        /// <summary>
		/// Load FieldType enum cache  
        /// </summary>
        /// <returns></returns>
        private static FieldTypeEnumsId[] GetFieldTypeId()
        {
            var fieldTypeEnums = (FieldType[])Enum.GetValues(typeof(FieldType));
            var result = new FieldTypeEnumsId[fieldTypeEnums.Length];

            for (var i = 0; i < fieldTypeEnums.Length; ++i)
                result[i] = new FieldTypeEnumsId((int)fieldTypeEnums[i], fieldTypeEnums[i]);

            Array.Sort(result, (x, y) => x.Id - y.Id);
            return result;
        }

        /// <summary>
		/// Load RelationType enum cache  
        /// </summary>
        /// <returns></returns>
        private static RelationTypeEnumsId[] GetRelationTypeId()
        {
            var relationTypeEnums = (RelationType[])Enum.GetValues(typeof(RelationType));
            var result = new RelationTypeEnumsId[relationTypeEnums.Length];

            for (var i = 0; i < relationTypeEnums.Length; ++i)
                result[i] = new RelationTypeEnumsId((long)relationTypeEnums[i], relationTypeEnums[i]);

            Array.Sort(result, (x, y) => x.Id.CompareTo(y.Id));
            return result;
        }

        private static RelationTypeEnumsName[] GetRelationTypeName()
        {
            var relationTypeEnums = (RelationType[])Enum.GetValues(typeof(RelationType));
            var result = new RelationTypeEnumsName[relationTypeEnums.Length];

            for (var i = 0; i < relationTypeEnums.Length; ++i)
                result[i] = new RelationTypeEnumsName(relationTypeEnums[i].ToString().ToLower().Trim(), (long)relationTypeEnums[i]);

            Array.Sort(result, (x, y) => string.CompareOrdinal(x.Name, y.Name));
            return result;
        }

        #endregion

    }
}
