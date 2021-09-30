using Ring.Data;
using Ring.Data.Core;
using Ring.Data.Core.Extensions;
using Ring.Data.Mappers;
using Ring.Data.Models;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System;
using System.IO;

namespace Ring.Schema.Core.Extensions
{
    internal static class TableSpaceExtension
    {
        /// <summary>
        /// Check if physical and logical tablespace exists
        /// </summary>
        public static bool Exists(this TableSpace tableSpace)
        {
            var schema = Global.Databases.MetaSchema;
            var connection = schema.Connections.Get(); // not from new schema !!!
            bool resultPhysical;
            bool resultLogical;
            var log = new Logger(typeof(TableSpace));

            try
            {
                resultPhysical = Exists(connection, tableSpace, true);
                resultLogical = Exists(connection, tableSpace, false);
            }
            catch (Exception ex)
            {
                log.Error(tableSpace.SchemaId, ex);
                throw;
            }
            finally
            {
                schema.Connections.Put(connection);
            }
            return resultLogical && resultPhysical;
        }

        public static void Create(this TableSpace tableSpace)
        {
            if (tableSpace == null) return;

            var databaseCollection = Global.Databases;
            var schema = databaseCollection.MetaSchema;
            var connection = schema.Connections.Get(); // not from new schema !!!
            var log = new Logger(typeof(TableSpace));

            try
            {
                // create directory if necessary
                CreateDirectory(tableSpace.FileName);
                if (!Exists(connection, tableSpace, true)) Helpers.SqlHelper.CreateTableSpace(connection, tableSpace); // cannot crash 
                if (!Exists(connection, tableSpace, false))
                {
                    var bs = new BulkSave { Schema = Global.Databases.MetaSchema }; // !! not efficient !! commit each table !!
                    var metaDataBuilder = new MetaDataBuilder();
                    var metaData = metaDataBuilder.GetInstance(tableSpace);
                    metaData.Id = GetNewFieldId(connection, tableSpace.SchemaId).ToString();
                    bs.InsertRecord(RecordMapper.Map(tableSpace.SchemaId, metaData, true));
                    bs.Save(connection);
                }
            }
            catch (Exception ex)
            {
                log.Error(tableSpace.SchemaId, ex);
                throw;
            }
            finally
            {
                schema.Connections.Put(connection);
            }
        }

        #region private methods

        private static bool Exists(IDbConnection connection, TableSpace tableSpace, bool physical)
        {
            var br = new BulkRetrieve { Schema = Global.Databases.MetaSchema };
            var fieldBuilder = new FieldBuilder();
            List result;
            if (physical)
            {
                br.SimpleQuery(0, Global.Databases.MetaSchema.GetTable(TableType.TableSpaceDictionary).Name);
                br.AppendFilter(0,
                    fieldBuilder.GetFieldName(Global.Databases.MetaSchema.Driver, TableType.TableSpaceDictionary, FieldKey.Name),
                    OperationType.Equal, tableSpace.Name, false);
                br.RetrieveRecords(connection);
                result = br.GetRecordList(0);
            }
            else
            {
                var tempResult = MetaDataExtension.GetMetaDataList(connection, tableSpace.SchemaId, EntityType.TableSpace, 0, null);
                result = new List { ItemType = tempResult.ItemType };
                for (var i = 0; i < tempResult.Count; ++i)
                    if (string.Equals(tableSpace.Name, tempResult[i].GetField(Constants.MetaDataName),
                        StringComparison.OrdinalIgnoreCase))
                    {
                        result.AppendItem(tempResult[i]);
                        break;
                    }
            }
            br.Dispose();
            return result.Count > 0;
        }

        private static int GetNewFieldId(IDbConnection connection, int schemaId)
        {
            var lst = MetaDataExtension.GetMetaDataList(connection, schemaId, EntityType.TableSpace, 0, null);
            var result = 0;
            for (var i = 0; i < lst.Count; ++i)
            {
                var id = int.Parse(lst[i].GetField());
                if (id > result) result = id;
            }
            return ++result;
        }

        private static void CreateDirectory(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder)) return;
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        }

        #endregion
    }
}
