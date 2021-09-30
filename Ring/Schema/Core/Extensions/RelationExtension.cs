using System;
using Ring.Data;
using Ring.Data.Core;
using Ring.Data.Core.Extensions;
using Ring.Data.Mappers;
using Ring.Data.Models;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Models;

namespace Ring.Schema.Core.Extensions
{
    internal static class RelationExtension
    {
        private static readonly MetaDataBuilder MetaDataBuilder = new MetaDataBuilder();

        public static Relation GetInverseRelation(this Relation relation) => relation?.To.GetRelation(relation.InverseRelationName);

	    public static Table GetCurrentTable(this Relation relation) => relation?.GetInverseRelation().To;

		/// <summary>
		/// Add relation on table (DDL) 
		/// </summary>
		public static void Add(this Relation relation)
        {
            if (relation == null) return;
			var table = GetCurrentTable(relation);
            var schema = Global.Databases.MetaSchema; 
            var connection = schema.Connections.Get(); // get connection from @meta schema
			var logger = new Logger(typeof(Relation));
            var jobId = Global.Databases.UpgradeJobId;

            try
            {
                if (relation.Type == RelationType.Mto || relation.Type == RelationType.Otop)
                    Helpers.SqlHelper.AlterTable(connection, table, relation, DatabaseOperation.Create);
                if (relation.Type == RelationType.Mtm)
                {
                    // create MTM table 
                    var pendingSchema = Global.Databases.PendingSchema;
                    var mtmTable = pendingSchema.GetMtmTable(relation.MtmTable);
                    mtmTable.Create();
                }
                //TODO check if it exist as inatif
                var meta = MetaDataBuilder.GetInstance(table.Id.ToString(), relation);
                var bs = new BulkSave { Schema = schema };
                bs.InsertRecord(RecordMapper.Map(table.SchemaId, meta, true));
                bs.Save(connection);
            }
            catch (Exception ex)
            {
                logger.Error(table.SchemaId, jobId, ex);
            }
            finally
            {
                schema.Connections.Put(connection);
            }
        }

        /// <summary>
        /// Remove relation on table (DDL) 
        /// </summary>
        public static void Remove(this Relation relation)
        {
            if (relation == null) return;
	        var table = relation.GetInverseRelation().To;
			var schema = Global.Databases.MetaSchema; 
            var connection = schema.Connections.Get();  // get connection from @meta schema
            var logger = new Logger(typeof(RelationExtension));
            var jobId = Global.Databases.UpgradeJobId;

            try
            {
                if (relation.Type == RelationType.Mto || relation.Type == RelationType.Otop)
                    Helpers.SqlHelper.AlterTable(connection, table, relation, DatabaseOperation.Delete);
                if (relation.Type == RelationType.Mtm)
                {
                    var prevSchema = Global.Databases.GetSchema(table.SchemaId);
                    var mtmTable = prevSchema.GetMtmTable(relation.MtmTable);
                    mtmTable.Drop();
                }

                // if MTM create table !!!
                //TODO check if it exist as inatif
                var lst = MetaDataExtension.GetMetaDataList(connection, table.SchemaId, EntityType.Relation, table.Id, relation.Id);
                MetaDataExtension.DeleteMetaDataList(connection, lst);
            }
            catch (Exception ex)
            {
                logger.Error(table.SchemaId, jobId, ex);
            }
            finally
            {
                schema.Connections.Put(connection);
            }
        }

	    /// <summary>
	    /// Add relation constraint Foreign key (DDL) 
	    /// </summary>
	    public static void AddConstrainst(this Relation relation)
	    {
		    var currentTable = GetCurrentTable(relation);
		    var schema = Global.Databases.MetaSchema;  // get connection from meta schema 
		    var connection = schema.Connections.Get();
		    var logger = new Logger(typeof(RelationExtension));
		    var jobId = Global.Databases.UpgradeJobId;

		    try
		    {

			    Helpers.SqlHelper.CreateForeignKey(connection, relation);
		    }
		    catch (Exception ex)
		    {
			    logger.Error(currentTable.SchemaId, jobId, ex);
		    }
		    finally
		    {
			    schema.Connections.Put(connection);
		    }
	    }

		/// <summary>
		/// Get table object behind MTM relationship
		/// </summary>
		public static Table GetMtmTable(this Relation relation)
	    {
		    if (relation == null || relation.Type != RelationType.Mtm) return null;
			
			// find first into pending schema 
		    var schema = Global.Databases.PendingSchema; // which schema ?
			var result = schema.GetMtmTable(relation.MtmTable);
		    if (result == null || result.Relations.Length<2) return null;
		    if (ReferenceEquals(result.Relations[0], relation) || ReferenceEquals(result.Relations[1], relation)) return result;
				//TODO mtm table not in PendingSchema
			return null;
	    }

		

    }
}
