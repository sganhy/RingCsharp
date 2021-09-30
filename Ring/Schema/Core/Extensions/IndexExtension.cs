using Ring.Data;
using Ring.Data.Core;
using Ring.Data.Core.Extensions;
using Ring.Data.Mappers;
using Ring.Data.Models;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System;
using System.Text;

namespace Ring.Schema.Core.Extensions
{
    internal static class IndexExtension
    {
	    /// <summary>
	    /// Generate physical name of index 
	    /// </summary>
	    public static string GetPhysicalName(this Index index, Table table)
	    {
		    string result;
		    switch (table.Type)
		    {
				case TableType.Business:
					result = Constants.IndexPrefixDefault + table.Id.ToString().PadLeft(4, Constants.PaddingIndexName) +
								Constants.Underscore + index.Id.ToString().PadLeft(4, Constants.PaddingIndexName);
					break;
				case TableType.Mtm:
					result = Constants.IndexPrefixDefault + table.Name.Substring(1,3).ToLower() + Constants.Underscore + index.Id.ToString().PadLeft(4, Constants.PaddingIndexName);
					break;
				default:
					result = Constants.IndexPrefixDefault + table.Name.Substring(1).ToLower() + Constants.Underscore +
					         index.Id.ToString().PadLeft(3, Constants.PaddingIndexName);
					break; 
		    }
		    return result;
		}

	    /// <summary>
        ///  Generate key from Index object (based on fields + relations) 
        /// </summary>
        public static string GetKey(this Index index)
        {
            var result = new StringBuilder();
            result.AppendLine(index.Unique.ToString());
            for (var i = 0; i < index.Fields.Length; ++i) result.AppendLine(index.Fields[i].Name);
            return result.ToString().ToUpper();
        }

	    /// <summary>
	    /// Find table from an index from meta schema & pending schema only !!!!
	    /// </summary>
	    public static Table GetCurrentTable(this Index index)
	    {
		    if (index == null) return null;
		    var schema = Global.Databases.MetaSchema;
		    //1> check meta schema 
		    var table = schema.GetTable(index.TableId);
		    if (table != null && IndexExists(table, index)) return table;

		    if (index.TableId > 0)
		    {
			    //2> check pending schema
			    schema = Global.Databases.PendingSchema;
			    table = schema.GetTable(index.TableId);
			    if (table != null && IndexExists(table, index)) return table;
			    //3> check previous schema upgrading
			    schema = Global.Databases.GetSchema(schema.Id);
			    table = schema.GetTable(index.TableId);
			    if (table != null && IndexExists(table, index)) return table;
		    }
		    else return GetMtmTable(index);
			return null;
	    }


	    public static Table GetMtmTable(this Index index)
	    {
			//2> check pending schema
			var schema = Global.Databases.PendingSchema;
		    if (schema.MtmTables==null) return null;
		    for (var i = 0; i < schema.MtmTables.Length; ++i)
			    if (IndexExists(schema.MtmTables[i], index))
					return schema.MtmTables[i];
		    return null;
	    }


		/// <summary>
		/// Create Index on table (DDL)
		/// </summary>
		public static void Create(this Index index)
        {
            var log = new Logger(typeof(IndexExtension));
            var databaseCollection = Global.Databases;
            var table = GetCurrentTable(index);
            var schema = databaseCollection.MetaSchema; // here always metaData schema
            var connection = schema.Connections.Get();
            var jobId = databaseCollection.UpgradeJobId > 0 ? databaseCollection.UpgradeJobId : Global.SequenceJobId.Value.CurrentId; // no value, get default value of JobId
	        var pendingSchema = Global.Databases.PendingSchema ?? Global.Databases.MetaSchema;
	        var tableSpace = pendingSchema.GetTableSpace(table, EntityType.Index);

			try
            {
                // TODO check if exists 
                if (table.Type == TableType.Business || table.Type == TableType.Mtm)
                {
                    var indexBuilder = new IndexBuilder();
                    var newIndexId = (int)(Global.SequenceIndexId.NextValue(connection) & int.MaxValue); // generate new index Id from sequence & re-use connection
                    var newIndex = indexBuilder.GetInstance(newIndexId, index);
                    
                    var metaDataBuilder = new MetaDataBuilder();

					// create physical index 
					Helpers.SqlHelper.CreateIndex(connection, table, newIndex, tableSpace);

	                // insert into @meta
	                if (table.Type == TableType.Business)
	                {
		                var bs = new BulkSave {Schema = databaseCollection.MetaSchema};
		                bs.InsertRecord(RecordMapper.Map(table.SchemaId, metaDataBuilder.GetInstance(newIndex), true));
		                bs.Save(connection);
	                }
                }
				else Helpers.SqlHelper.CreateIndex(connection, table, index, tableSpace);
            }
            catch (Exception ex)
            {
                log.Error(table.SchemaId, jobId, ex);
            }
            finally
            {
                schema.Connections.Put(connection);
            }
        }

		/// <summary>
		/// Drop Index on table (DDL)
		/// </summary>
		public static void Remove(this Index index)
        {
            var databaseCollection = Global.Databases;
            var table = GetCurrentTable(index);
            if (table.Type != TableType.Business) return; // do nothing 
            var schema = databaseCollection.MetaSchema;   // here always metaData schema
            var connection = schema.Connections.Get();
            var log = new Logger(typeof(IndexExtension));

            try
            {
                // drop physical index 
                Helpers.SqlHelper.DropIndex(connection, Global.Databases.GetSchema(table.SchemaId), table, index);
                var lst = MetaDataExtension.GetMetaDataList(connection, table.SchemaId, EntityType.Index, table.Id, index.Id);
                MetaDataExtension.DeleteMetaDataList(connection, lst);
            }
            catch (Exception ex)
            {
                log.Error(table.SchemaId, databaseCollection.UpgradeJobId, ex);
            }
            finally
            {
                schema.Connections.Put(connection);
            }

        }

        #region private methods

        /// <summary>
        /// Find into table compared to reference
        /// </summary>
        private static bool IndexExists(Table table, Index index)
        {
            if (table?.Indexes != null)
                for (var i = 0; i < table.Indexes.Length; ++i)
                    if (ReferenceEquals(table.Indexes[i], index))
                        return true;
            return false;
        }

        #endregion

    }
}
