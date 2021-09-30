using Ring.Data;
using Ring.Data.Core;
using Ring.Data.Core.Extensions;
using Ring.Data.Helpers;
using Ring.Data.Mappers;
using Ring.Data.Models;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System;

namespace Ring.Schema.Core.Extensions
{
    internal static class SequenceExtension
    {

        /// <summary>
        /// Calculate next value of sequence (Hi/Lo algorithm) 
        /// </summary>
        public static long NextValue(this Sequence sequence, IDbConnection connection = null)
        {
            var result = 0L;
            if (sequence == null) return result;
            lock (sequence.Value.Sync)
            {
                if (sequence.Value.CurrentId >= sequence.MaxValue) throw new NotImplementedException();
                // reserve range of value into @meta_id id 
                if (sequence.Value.CurrentId < sequence.Value.MaxId) return ++sequence.Value.CurrentId;
                var currentSchema = Global.Databases.GetSchema(sequence.SchemaId);
                var conn = connection ?? currentSchema.Connections.Get(); // re-use connection if exist
                var range = sequence.Value.ReservedRange <= 0 ? 1 : sequence.Value.ReservedRange;

                try
                {
                    var sql = SqlHelper.GetUpdateMetaIdQuery(conn, sequence.Id, sequence.SchemaId, Constants.SequenceId, range);
                    sql.Connection = conn;
                    result = long.Parse(sql.ExecuteScalar().ToString());
                    sql.Connection = null; // don't keep a reference to the Db connection
                }
                finally
                {
                    // return connection immediatly
                    if (connection == null) currentSchema.Connections.Put(conn);
                }
                // ==== end update @meta_id
                sequence.Value.MaxId = result;
                result -= range;
                sequence.Value.CurrentId = ++result;
                sequence.Value.ReservedRange <<= 1; // double range !! 
            }
            return result;
        }

        /// <summary>
        /// Is sequence exists in @meta and @meta_id
        /// </summary>
        public static bool Exists(this Sequence sequence)
        {
            var schema = Global.Databases.MetaSchema;
            var connection = schema.Connections.Get(); // not from new schema !!!
            var log = new Logger(typeof(Sequence));
            var result = false;
            try
            {
                var resultMeta = MetaDataExtension.GetMetaDataList(connection, sequence.SchemaId, EntityType.Sequence, sequence.SchemaId, sequence.Id);
                var resultMetaId = MetaDataExtension.GetMetaDataIdList(connection, sequence.SchemaId, EntityType.Sequence, sequence.Id);

                result = resultMeta.Count > 0 && resultMetaId.Count > 0;
            }
            catch (Exception ex)
            {
                log.Error(sequence.SchemaId, ex);
            }
            finally
            {
                schema.Connections.Put(connection);
            }
            return result;
        }

        /// <summary>
        /// Add sequence into @meta and @meta_id
        /// </summary>
        public static void Create(this Sequence sequence)
        {
            var schema = Global.Databases.MetaSchema;
            var connection = schema.Connections.Get(); // not from new schema !!!
            var bs = new BulkSave { Schema = Global.Databases.MetaSchema };
            var metaDataBuilder = new MetaDataBuilder();
            var log = new Logger(typeof(Sequence));

            try
            {
                var resultMeta = MetaDataExtension.GetMetaDataList(connection, sequence.SchemaId, EntityType.Sequence, sequence.SchemaId, sequence.Id);
                if (resultMeta.Count <= 0)
                {
                    var meta = metaDataBuilder.GetInstance(sequence);
                    bs.InsertRecord(RecordMapper.Map(sequence.SchemaId, meta, true));
                }
                var resultMetaId = MetaDataExtension.GetMetaDataIdList(connection, sequence.SchemaId, EntityType.Sequence, sequence.Id);
                if (resultMetaId.Count <= 0)
                {
                    // int id, int schemaId,  sbyte objectType, long value
                    bs.InsertRecord(RecordMapper.Map(sequence.Id, sequence.SchemaId, (sbyte)EntityType.Sequence,
                        sequence.Value.CurrentId));
                }
                bs.Save(connection);
            }
            catch (Exception ex)
            {
                log.Error(sequence.SchemaId, ex);
            }
            finally
            {
                schema.Connections.Put(connection);
            }
        }

        /// <summary>
        /// Load current ID
        /// </summary>
        public static void Load(this Sequence sequence)
        {
            var databases = Global.Databases;
            using (var br = new BulkRetrieve { Schema = Global.Databases.MetaSchema })
            {
                // @meta_id 
                br.SimpleQuery(0, databases.MetaDataIdTable.Name);
                br.AppendFilter(0, Constants.MetaDataSchemaId, OperationType.Equal, sequence.SchemaId);
                br.AppendFilter(0, Constants.MetaDataObjectType, OperationType.Equal, (int)EntityType.Sequence);
                br.AppendFilter(0, Constants.MetaDataId, OperationType.Equal, sequence.Id);
                br.RetrieveRecords();

                var resultMetaId = br.GetRecordList(0);
                if (resultMetaId.Count > 0)
                {
                    var rcd = resultMetaId.ItemByIndex(0);
                    rcd.GetField(Constants.MetaDataValue, out sequence.Value.CurrentId);

                }
            }
        }

    }
}
