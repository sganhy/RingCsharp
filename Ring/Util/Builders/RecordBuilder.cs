using Ring.Data;
using Ring.Schema.Builders;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Models;
using Ring.Util.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ring.Util.Builders
{
    internal sealed class RecordBuilder
    {

        private readonly LanguageBuilder _languageBuilder = new LanguageBuilder();

        /// <summary>
        /// Build a record list from excel sheet 
        /// </summary>
        public Record[] GetInstances(int schemaId, ExcelSheet sheet)
        {
            if (sheet?.Columns == null || sheet.Columns.Length == 0) return new Record[0];
            var result = GetLexiconItemList(schemaId, sheet);
            return result.ToArray();
        }

        /// <summary>
        /// Get @lexicon_itm record instance 
        /// </summary>
        public static Record GetInstance(int? id, int lexiconId, int? referenceId, string value)
        {
            // id, lexicon_id, reference_id, value
            var result = new Record(Data.Core.Global.Databases.LexiconTable);
            result.SetField(Constants.MetaDataId, id?.ToString());
            result.SetField(Constants.LexiconItemLexiId, lexiconId);
            result.SetField(Constants.MetaDataRefId, referenceId?.ToString());
            result.SetField(Constants.MetaDataRefId, referenceId?.ToString());
            result.SetField(Constants.MetaDataValue, value);
            return result;
        }

        #region private methods 

        /// <summary>
        /// Get lexicon list from excel sheet
        /// </summary>
        /// <param name="schemaId"></param>
        /// <param name="sheet"></param>
        /// <returns></returns>
        private List<Record> GetLexiconItemList(int schemaId, ExcelSheet sheet)
        {
            var schema = Data.Core.Global.Databases.GetSchema(schemaId);
            var tableLexicon = Data.Core.Global.Databases.LexiconTable;
            var result = new List<Record>();

            // KEY for @lexicon     ==> schema_id, table_id, field_id, relation_id, relation_value
            //   use partial KEY    ==> table_id, field_id, relation_id, relation_value
            //   localKey()         ==> table_name+field_name. TODO manage key on schema & relation lexicon
            var dico = new SortedDictionary<string, Lexicon>();
            var tableColumn = GetColumn(sheet, Constants.SheetHeaderTableName);
            var toFieldColumn = GetColumn(sheet, Constants.SheetHeaderFieldName);
            var fromFieldColumn = GetColumn(sheet, Constants.SheetHeaderKeyFieldName);
            var valueColumn = GetColumn(sheet, Constants.SheetHeaderSourceName);
            var languageList = GetLanguageList(sheet);
            if (valueColumn == null || languageList.Length == 0) return new List<Record>();

            for (var i = sheet.FirstRowNum; i <= sheet.LastRowNum; ++i)
            {
                if (i == sheet.FirstRowNum) continue;
                // get id 
                var tableName = tableColumn.Cells.ContainsKey(i) ? tableColumn.Cells[i] : null;
                var tableId = tableName != null ? GetTableId(i, tableColumn, schema) : null;
                var toFieldId = tableId != null ? GetFielId(i, (int)tableId, toFieldColumn, schema) : null;
                var fromFieldId = tableId != null ? GetFielId(i, (int)tableId, fromFieldColumn, schema) : null;

                // generate key
                var key = GenerateKey(tableId, toFieldId, null, null);
                if (tableId == null) continue; //TODO : manage RelationId or schema translation
                // generate lexicon
                var currentLexicon = !dico.ContainsKey(key) ?
                            LexiconBuilder.GetInstance(tableLexicon, schemaId, tableId, fromFieldId, toFieldId, null, null) : dico[key];
                if (!dico.ContainsKey(key)) dico.Add(key, currentLexicon);
                // generate value 
                var value = valueColumn.Cells.ContainsKey(i) ? valueColumn.Cells[i] : null;
                // generate @lexicon_itm: id, lexicon_id, reference_id, value
                if (value != null)
                {
                    // Table table,int? id, int lexiconId, int? referenceId, string value
                    // source 
                    result.Add(GetInstance(i, currentLexicon.Id, null, value));
                    for (var j = 0; j < languageList.Length; ++j)
                    {
                        var languageInfo = languageList[j];
                        // manage destination
                        if (languageInfo.Item2.Cells.ContainsKey(i))
                            result.Add(GetInstance(languageInfo.Item1.Id,
                                currentLexicon.Id, i, languageInfo.Item2.Cells[i]));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Get column from header information 
        /// </summary>
        private static ExcelColumn GetColumn(ExcelSheet sheet, string headerName)
        {
            for (var i = 0; i < sheet.Columns.Length; ++i)
                if (sheet.Columns[i].Cells.ContainsKey(sheet.FirstRowNum) &&
                    string.Equals(sheet.Columns[i].Cells[sheet.FirstRowNum], headerName,
                        StringComparison.OrdinalIgnoreCase))
                    return sheet.Columns[i];
            return null;
        }

        /// <summary>
        /// Generate unique key for temporary lexicon dictionary
        /// </summary>
        /// <param name="tableId"></param>
        /// <param name="fieldId"></param>
        /// <param name="relationId"></param>
        /// <param name="relationValue"></param>
        /// <returns></returns>
        private static string GenerateKey(int? tableId, int? fieldId, int? relationId, string relationValue)
        {
            var result = new StringBuilder();
            result.Append(tableId);
            result.Append(Constants.KeySperator);
            result.Append(fieldId);
            result.Append(Constants.KeySperator);
            result.Append(relationId);
            result.Append(Constants.KeySperator);
            result.Append(relationValue);
            return result.ToString();
        }

        /// <summary>
        /// Get list of language from header
        /// </summary>
        private Tuple<Language, ExcelColumn>[] GetLanguageList(ExcelSheet sheet)
        {
            if (sheet?.Columns == null || sheet.Columns.Length == 0) return new Tuple<Language, ExcelColumn>[0];
            var result = new List<Tuple<Language, ExcelColumn>>();
            for (var i = 0; i < sheet.Columns.Length; ++i)
            {
                var col = sheet.Columns[i];
                var value = col.Cells[col.FirstRowNum];
                var language = _languageBuilder.GetInstance(value);
                if (language != null) result.Add(Tuple.Create(language, col));
            }
            return result.ToArray();
        }

        /// <summary>
        /// Get Table id from schema
        /// </summary>
        /// <param name="rowNum"></param>
        /// <param name="column"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        private static int? GetTableId(int rowNum, ExcelColumn column, Schema.Models.Schema schema) =>
            column.Cells.ContainsKey(rowNum) ? schema.GetTable(column.Cells[rowNum], StringComparison.OrdinalIgnoreCase)?.Id : null;

        /// <summary>
        /// Get Field id from schema
        /// </summary>
        /// <param name="rowNum"></param>
        /// <param name="tableId"></param>
        /// <param name="column"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        private static int? GetFielId(int rowNum, int tableId, ExcelColumn column, Schema.Models.Schema schema) =>
            column.Cells.ContainsKey(rowNum) ? schema.GetTable(tableId)?.GetField(column.Cells[rowNum], StringComparison.OrdinalIgnoreCase)?.Id : null;

        #endregion
    }
}