using Ring.Data;
using Ring.Data.Core;
using Ring.Data.Mappers;
using Ring.Schema.Builders;
using Ring.Schema.Core.Rules;
using Ring.Schema.Core.Rules.Impl;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Database = Ring.Schema.Models.Schema;

namespace Ring.Schema.Core.Extensions
{
    internal static class SchemaExtension
    {

        /// <summary>
        /// Get table object by name (case sensitive) --> O(log n)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Sequence GetSequence(this Database schema, string name)
        {
            int indexerLeft = 0, indexerRigth = schema.Sequences.Length - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
                var indexerCompare = string.CompareOrdinal(name, schema.Sequences[indexerMiddle].Name); //indexerCompare = string.Compare(name, _tablesByName[indexerMiddle].Name, StringComparison.OrdinalIgnoreCase);
                if (indexerCompare == 0) return schema.Sequences[indexerMiddle];
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return null;
        }

        /// <summary>
        /// Generate a new Job Id using sequence "@job_id"
        /// </summary>
        public static long GetJobId(this Database schema) => GetSequence(schema, Constants.SequenceJobIdName).NextValue();

        /// <summary>
        /// Get find a tablespace from a table
        /// </summary>
        public static TableSpace GetTableSpace(this Database schema, Table table, EntityType entityType)
        {
            // avoiud exception here 
            if (schema == null || table == null) return null;
            for (var i = 0; i < schema.TableSpaces.Length; ++i)
            {
                // check readonly info first 
                if (table.Readonly && schema.TableSpaces[i].IsTable && schema.TableSpaces[i].IsReadonly && entityType == EntityType.Table)
                    return schema.TableSpaces[i];
                // default tablespaces
                if (schema.TableSpaces[i].IsTable && entityType == EntityType.Table) return schema.TableSpaces[i];
                if (schema.TableSpaces[i].IsIndex && entityType == EntityType.Index) return schema.TableSpaces[i];
            }
            return null;
        }

        /// <summary>
        /// Get find a tablespace from an index  
        /// </summary>
        public static TableSpace GetTableSpace(this Database schema, Index table)
        {
            return null;
        }

        /// <summary>
        /// Get table object by Id
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Table GetTable(this Database schema, int id)
        {
            int indexerLeft = 0, indexerRigth = schema.TablesById.Length - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
                var indexerCompare = id - schema.TablesById[indexerMiddle].Id;
                if (indexerCompare == 0L) return schema.TablesById[indexerMiddle];
                if (indexerCompare > 0L) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return null;
        }

        /// <summary>
        /// Get table object by name (case sensitive) --> O(log n)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Table GetTable(this Database schema, string name)
        {
            int indexerLeft = 0, indexerRigth = schema.TablesById.Length - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
                var indexerCompare = string.CompareOrdinal(name, schema.TablesByName[indexerMiddle].Name); //indexerCompare = string.Compare(name, _tablesByName[indexerMiddle].Name, StringComparison.OrdinalIgnoreCase);
                if (indexerCompare == 0) return schema.TablesByName[indexerMiddle];
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return null;
        }

        /// <summary>
        /// Get first table by type
        /// </summary>
        public static Table GetTable(this Database schema, TableType type)
        {
            if (schema != null)
                for (var i = 0; i < schema.TablesById.Length; ++i)
                    if (schema.TablesById[i].Type == type) return schema.TablesById[i];
            return null;
        }

        /// <summary>
        /// Get table object by name --> The Sequential Search --> O(n)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Table GetTable(this Database schema, string name, StringComparison comparisonType)
        {
            if (schema == null || name == null) return null;
            if (comparisonType == StringComparison.Ordinal) return GetTable(schema, name);
            for (var i = 0; i < schema.TablesById.Length; ++i)
                if (schema.TablesById[i].Name.Equals(name, comparisonType))
                    return schema.TablesById[i];
            return null;
        }

        /// <summary>
        /// Get MTM table object by name
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Table GetMtmTable(this Database schema, string name)
        {
            int indexerLeft = 0, indexerRigth = schema.MtmTables.Length - 1;
            while (indexerLeft <= indexerRigth)
            {
                var indexerMiddle = indexerLeft + indexerRigth;
                indexerMiddle >>= 1;   // indexerMiddle <-- indexerMiddle /2 
                var indexerCompare = string.CompareOrdinal(name, schema.MtmTables[indexerMiddle].Name); 
                if (indexerCompare == 0) return schema.MtmTables[indexerMiddle];
                if (indexerCompare > 0) indexerLeft = indexerMiddle + 1;
                else indexerRigth = indexerMiddle - 1;
            }
            return null;
        }

        /// <summary>
        /// Check if newSchema objects exists in the database
        /// </summary>
        internal static bool Exists(this Database schema)
        {
            if (schema == null) return false;
            if (schema.IsMetaSchema)
            {
                // physical newSchema exists ?
                if (!Exists(schema.Driver, schema.SearchPath)) return false;
                // meta tables exists ?
                for (var i = 0; i < schema.TablesById.Length; ++i)
                    if (schema.TablesById[i].PhysicalType == PhysicalType.Table && !schema.TablesById[i].Exists())
                        return false;
            }
            else return Global.Databases.GetSchema(schema.Id) != null;
            return true;
        }

		/// <summary>
		/// Create a new newSchema (DDL) 
		/// </summary>
		internal static void Create(this Database newSchema)
        {
            if (!Exists(newSchema.Driver, newSchema.SearchPath)) CreatePhysicalSchema(newSchema.SearchPath);

            if (newSchema.IsMetaSchema)
            {
                // mta newSchema creation partial or not  
                for (var i = 0; i < newSchema.TablesById.Length; ++i)
                    if (newSchema.TablesById[i].PhysicalType == PhysicalType.Table && !newSchema.TablesById[i].Exists())
                        newSchema.TablesById[i].Create();
                return;
            }
            Global.Databases.SetPendingSchema(newSchema);
            var jobId = Global.Databases.UpgradeJobId;
            var databaseCollection = Global.Databases;
            
			// check duplicate key here !! to avoid crash 
			IValidationRule<Table> validator = new ValidationTableMetaKey();
            validator.Validate(newSchema.TablesById);

	        AddTableSpaces(newSchema);
	        AddTables(newSchema);
			AddSchema(newSchema);
			AddSequences(newSchema);
			AddForgenKeys(newSchema);
			AnalyzeMetaSchema();
			databaseCollection.LoadSchema(jobId, newSchema.Id);  // finally - return jobId
        }

        /// <summary>
        /// Alter physical newSchema
        /// </summary>
        internal static void Alter(this Database newSchema)
        {
            var prevSchema = Global.Databases.GetSchema(newSchema.Id);
            var databaseCollection = Global.Databases;
            var jobId = Global.Databases.UpgradeJobId;

			Global.Databases.SetPendingSchema(newSchema);

			var prevTableDico = GetTableDico(prevSchema);
	        var currentTableDico = GetTableDico(newSchema);

	        DropTables(prevSchema, currentTableDico);
			AddTables(newSchema, prevTableDico);
	        AlterTables(newSchema, prevSchema, prevTableDico);
	        AnalyzeMetaSchema();
			databaseCollection.LoadSchema(jobId, newSchema.Id);  // finally - return jobId
        }

        #region private methods

        /// <summary>
        /// Case insensitif method to detect if the physical newSchema exist in a database (very slow)
        /// </summary>
        private static bool Exists(DatabaseProvider provider, string schemaName)
        {
            var br = new BulkRetrieve { Schema = Global.Databases.MetaSchema };
            var fieldBuilder = new FieldBuilder();
            if (string.IsNullOrEmpty(schemaName)) return false;
            br.SimpleQuery(0, Global.Databases.MetaSchema.GetTable(TableType.SchemaDictionary).Name);
            br.AppendFilter(0, fieldBuilder.GetFieldName(provider, TableType.SchemaDictionary, FieldKey.SchemaName),
                OperationType.Equal, schemaName, false);
            br.RetrieveRecords();
            return br.GetRecordList(0).Count > 0;
        }

        /// <summary>
        /// Create database newSchema
        /// </summary>
        private static void CreatePhysicalSchema(string name)
        {
            var currentSchema = Global.Databases.MetaSchema;
            var conn = currentSchema.Connections.Get();
            try
            {
                Helpers.SqlHelper.CreateSchema(conn, name);
            }
            finally
            {
                currentSchema.Connections?.Put(conn);
            }
        }

		/// <summary>
		/// Insert schema information to @meta tables
		/// </summary>
		/// <param name="newSchema"></param>
	    private static void AddSchema(Database newSchema)
	    {
		    var databaseCollection = Global.Databases;
		    var bs = new BulkSave { Schema = databaseCollection.MetaSchema }; // update meta table 
		    var metaDataBuilder = new MetaDataBuilder();
			var metaData = metaDataBuilder.GetInstance(newSchema);
		    var metaDefaultLanguage = metaDataBuilder.GetInstance(newSchema.DefaultLanguage);

		    bs.Schema = databaseCollection.MetaSchema; // update meta table 
		    bs.InsertRecord(RecordMapper.Map(newSchema.Id, metaData, true));
		    bs.InsertRecord(RecordMapper.Map(newSchema.Id, metaDefaultLanguage, true));
		    bs.Save();
		}

		/// <summary>
		/// Aanalyze meta schema after schema upgrade
		/// </summary>
		private static void AddSequences(Database newSchema)
	    {
			if (newSchema.Sequences != null)
			{
				var databaseCollection = Global.Databases;
				var bs = new BulkSave { Schema = databaseCollection.MetaSchema }; // update meta table 
				var metaDataBuilder =  new MetaDataBuilder();
				for (var i = 0; i < newSchema.Sequences.Length; ++i)
				{
					var sequence = newSchema.Sequences[i]; // re-use same connection & transaction !
					// insert into @meta
					bs.InsertRecord(RecordMapper.Map(newSchema.Id, metaDataBuilder.GetInstance(sequence), true));
					// int id, int schemaId,  sbyte objectType, long value
					bs.InsertRecord(RecordMapper.Map(sequence.Id, newSchema.Id, (sbyte) EntityType.Sequence, 1L));
				}
				bs.Save();
			}
	    }

		/// <summary>
		/// Aanalyze meta schema after schema upgrade
		/// </summary>
		private static void AddForgenKeys(Database newSchema)
	    {
		    AddTableForgenKeys(newSchema);
		    AddMtmForgenKeys(newSchema);
		}
	    private static void AddTableForgenKeys(Database newSchema)
	    {
		    for (var i = 0; i < newSchema.TablesById.Length; ++i)
			    if (newSchema.TablesById[i].Relations != null)
				    for (var j = 0; j < newSchema.TablesById[i].Relations.Length; ++j)
					    if (newSchema.TablesById[i].Relations[j].Constraint)
						    newSchema.TablesById[i].Relations[j].AddConstrainst();
	    }
	    private static void AddMtmForgenKeys(Database newSchema)
	    {
		    if (newSchema.MtmTables == null) return;
			for (var i = 0; i < newSchema.MtmTables.Length; ++i)
				if (newSchema.MtmTables[i].Relations != null)
					for (var j = 0; j < newSchema.MtmTables[i].Relations.Length; ++j)
						if (newSchema.MtmTables[i].Relations[j].Constraint)
							newSchema.MtmTables[i].Relations[j].AddConstrainst();
	    }

		/// <summary>
		/// Aanalyze meta schema after schema upgrade
		/// </summary>
		private static void AnalyzeMetaSchema()
	    {
		    Global.Databases.MetaDataTable.Vacuum();   // vacuum on @meta table
		    Global.Databases.MetaDataTable.Analyze();  // analyze on @meta table
		    Global.Databases.MetaDataIdTable.Analyze();  // analyze on @meta_id table
		}
	    private static void AddTableSpaces(Database newSchema)
	    {
		    for (var i = 0; i < newSchema.TableSpaces.Length; ++i) if (!newSchema.TableSpaces[i].Exists()) newSchema.TableSpaces[i].Create();
		}
	    private static void AddTables(Database schema, HashSet<string> prevTableSet =null)
	    {
			// create tables by id !!!!! important for db 
		    for (var i = 0; i < schema.TablesById.Length; ++i)
				if (prevTableSet==null || !prevTableSet.Contains(schema.TablesById[i].Name.ToUpper()))
					schema.TablesById[i].Create();
		    for (var i = 0; i < schema.MtmTables.Length; ++i)
			    if (prevTableSet == null || !prevTableSet.Contains(schema.MtmTables[i].Name.ToUpper()))
				    schema.MtmTables[i].Create();
		}
	    private static HashSet<string> GetTableDico(Database schema)
	    {
		    var result = new HashSet<string>();
		    for (var i = 0; i < schema.TablesById.Length; ++i) if (!result.Contains(schema.TablesById[i].Name.ToUpper())) result.Add(schema.TablesById[i].Name.ToUpper());
		    for (var i = 0; i < schema.MtmTables.Length; ++i) if (!result.Contains(schema.MtmTables[i].Name.ToUpper())) result.Add(schema.MtmTables[i].Name.ToUpper());
			return result;
	    }
		private static void DropTables(Database schema, HashSet<string> newTableSet)
	    {
			for (var i = 0; i < schema.TablesById.Length; ++i)
				if (!newTableSet.Contains(schema.TablesById[i].Name.ToUpper()))
					schema.TablesById[i].Drop();
		}
	    private static void AlterTables(Database newSchema, Database prevSchema, HashSet<string> prevTableSet)
	    {
		    for (var i = 0; i < newSchema.TablesById.Length; ++i)
		    {
			    var table = newSchema.TablesById[i];
			    // exist in the previous schema ? 
			    if (!prevTableSet.Contains(table.Name.ToUpper())) continue;
			    // search by id first - can be complexicity --> O(n²) 
			    var tableToCompare = prevSchema.GetTable(table.Name) ?? prevSchema.GetTable(table.Name, StringComparison.OrdinalIgnoreCase);
			    if (tableToCompare != null && !TableExtension.Equals(table, tableToCompare)) table.Alter();
		    }
		}

		#endregion

	}
}
