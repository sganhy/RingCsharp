using Ring.Data;
using Ring.Data.Core;
using Ring.Data.Core.Extensions;
using Ring.Data.Mappers;
using Ring.Data.Models;
using Ring.Schema.Builders;
using Ring.Schema.Enums;
using Ring.Schema.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Ring.Schema.Core.Extensions
{
    internal static class FieldExtension
    {
        private static readonly MetaDataBuilder MetaDataBuilder = new MetaDataBuilder();

        public static bool IsDateTime(this Field field) => field.Type == FieldType.DateTime ||
                                                           field.Type == FieldType.ShortDateTime ||
                                                           field.Type == FieldType.LongDateTime;

        public static bool IsNumeric(this Field field) => field.Type == FieldType.Long || field.Type == FieldType.Int ||
            field.Type == FieldType.Short || field.Type == FieldType.Byte || field.Type == FieldType.Float ||
            field.Type == FieldType.Double;

        public static bool IsPrimaryKey(this Field field) =>
            ReferenceEquals(field, Constants.DefaultPrimaryKey) || ReferenceEquals(field, Constants.DefaultClflyPrimaryKey) ||
			ReferenceEquals(field, Constants.DefaultShortPrimaryKey) || ReferenceEquals(field, Constants.DefaultIntPrimaryKey);

        /// <summary>
        /// Calculate searchable field value (remove diacritic characters and value.ToUpper())
        /// </summary>
        public static string GetSearchableValue(this Field field, string value)
        {
            if (value == null) return null;
            var result = new StringBuilder();
            var normalizedString = value.Normalize(NormalizationForm.FormD);
            var count = normalizedString.Length;

            for (var i = 0; i < count; ++i)
            {
                // CharUnicodeInfo.GetUnicodeCategory(c) <> UnicodeCategory.NonSpacingMark
                var c = normalizedString[i];
                if (char.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    result.Append(char.ToUpper(c));
            }
            return result.ToString();
        }

        /// <summary>
        /// Is current is unique for the current table ==> complexity O(n)
        /// </summary>
        public static bool IsUnique(this Field field, Table table)
        {
            if (field != null && table.Indexes != null)
            {
                if (table.PrimaryKeyIdIndex >= 0 && ReferenceEquals(table.PrimaryKey, field)) return true;
                for (var i = 0; i < table.Indexes.Length; ++i)
                {
                    var index = table.Indexes[i];
                    if (index.Fields != null && index.Fields.Length == 1 && ReferenceEquals(index.Fields[0], field)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Add field on table 
        /// </summary>
        public static void Add(this Field field, Table table)
        {
            if (field == null || table == null) return;
            var schema = Global.Databases.MetaSchema;
            var connection = schema.Connections.Get(); // not from new schema !!!
            var logger = new Logger(typeof(Field));
            var jobId = Global.Databases.UpgradeJobId;

            try
            {
                Helpers.SqlHelper.AlterTable(connection, table, field, DatabaseOperation.Create);
                //TODO check if it exist as inatif
                var meta = MetaDataBuilder.GetInstance(table.Id.ToString(), field);
                meta.Id = GetNewFieldId(table, field).ToString();
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
		/// Remove field on table 
		/// TODO no need table parameter
		/// </summary>
		public static void Remove(this Field field, Table table)
        {
            if (field == null || table == null) return;
            var schema = Global.Databases.MetaSchema;
            var connection = schema.Connections.Get(); // not from new schema !!!
            var logger = new Logger(typeof(FieldExtension));
            var jobId = Global.Databases.UpgradeJobId;

            try
            {
                Helpers.SqlHelper.AlterTable(connection, table, field, DatabaseOperation.Delete);
                var lst = MetaDataExtension.GetMetaDataList(connection, table.SchemaId, EntityType.Field, table.Id, field.Id);
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

        #region private methods

        private static int GetNewFieldId(Table table, Field field)
        {
            var prevTable = Global.Databases.GetSchema(table.SchemaId).GetTable(table.Id);
            var prevLastField = prevTable.FieldsById[prevTable.FieldsById.Length - 1];
            var prevMaxId = prevLastField.Id;
            // count number of new field before current one (position) 
            var newFieldCount = 0;
            var index = 0;
            // dico of previous fields 
            var hashSet = new HashSet<string>();
            for (var i = 0; i < prevTable.Fields.Length; ++i)
                if (!hashSet.Contains(prevTable.Fields[i].Name.ToUpper())) hashSet.Add(prevTable.Fields[i].Name.ToUpper());

            // find current position index of field
            for (var i = table.FieldsById.Length - 1; i >= 0; --i) if (table.FieldsById[i].Id == field.Id)
                {
                    index = i;
                    break;
                }
            while (index >= 0)
            {
                // is new field ? 
                if (!hashSet.Contains(table.FieldsById[index].Name.ToUpper())) ++newFieldCount;
                --index;
            }
            return prevMaxId + newFieldCount;
        }

        #endregion

    }
}
