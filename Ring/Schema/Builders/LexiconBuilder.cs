using Ring.Data;
using Ring.Data.Core;
using Ring.Data.Enums;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Models;
using Ring.Util.Core.Extensions;
using System;
using System.Text;

namespace Ring.Schema.Builders
{
    internal sealed class LexiconBuilder : EntityBuilder
    {

        private readonly LanguageBuilder _languageBuilder = new LanguageBuilder();

        /// <summary>
        /// Build Lexicon object from List of 
        /// </summary>
        public Lexicon GetInstances(Record lexicon, List lexiconItemList)
        {
            var languageList = Getlanguages(lexiconItemList);
            if (languageList == null || languageList.Length <= 0) return null;
            var result = GetInstance(lexicon, languageList);
            LoadTranslation(result, lexiconItemList);
            return result;
        }

        /// <summary>
        /// Get instance of lexicon from unique record from @lexicon table
        /// </summary>
        public static Lexicon GetInstance(Record lexicon, Language[] languages)
        {
            if (lexicon == null || lexicon.Table.Name != Constants.LexiconTableName) return null;
            int id;
            var translationCount = languages?.Length ?? 0;
            int schemaId;
            bool active;
            var isFieldSearchable = false;
            var isFieldUnique = false;
            int? tableId;
            int? toFieldId;
            int? fromFieldId;
            int? relationId;


            lexicon.GetField(Constants.MetaDataId, out id);
            lexicon.GetField(Constants.MetaDataSchemaId, out schemaId);
            lexicon.GetField(Constants.MetaDataActive, out active);
            lexicon.GetField(Constants.LexiconTableId, out tableId);
            lexicon.GetField(Constants.LexiconToFieldId, out toFieldId);
            lexicon.GetField(Constants.LexiconFromFieldId, out fromFieldId);
            lexicon.GetField(Constants.LexiconRelationId, out relationId);

            Guid guid;
            Guid.TryParse(lexicon.GetField(Constants.LexiconGuid), out guid);

            Table table = null;
            Field toField = null;
            Field fromField = null;
            Relation relation = null;

            if (schemaId >= 0)
            {
                var schema = Global.Databases.GetSchema(schemaId);
                if (schema != null && tableId != null)
                {
                    table = schema.GetTable((int)tableId);
                    if (table != null && toFieldId != null) toField = table.GetField((int)toFieldId);
                    if (table != null && fromFieldId != null) fromField = table.GetField((int)fromFieldId);
                    if (table != null && relationId != null) relation = table.GetRelation((int)relationId);
                }
            }
            if (fromField == null) fromField = toField;
            if (fromField != null)
            {
                isFieldSearchable = !fromField.CaseSensitive;
                isFieldUnique = fromField.IsUnique(table);
            }
            var result = new Lexicon(id, lexicon.GetField(Constants.MetaDataName), lexicon.GetField(Constants.MetaDataDescription), schemaId,
                table, toField, fromField, relation, languages, guid,
                new System.Collections.Generic.Dictionary<string, string>[translationCount], isFieldSearchable && isFieldUnique, true, active);
            return result;
        }

        /// <summary>
        /// Get Lexicon instance without lexicon items
        /// </summary>
        public static Lexicon GetInstance(Table table, int schemaId, int? tableId, int? fromFieldId, int? toFieldId, int? relationId,
            string relationValue)
        {
            Record result;
            var br = new BulkRetrieve { Schema = Global.Databases.MetaSchema };
            br.SimpleQuery(0, table.Name);
            br.AppendFilter(0, Constants.MetaDataSchemaId, OperationType.Equal, schemaId);
            br.AppendFilter(0, Constants.LexiconTableId, OperationType.Equal, tableId?.ToString());
            br.AppendFilter(0, Constants.LexiconToFieldId, OperationType.Equal, toFieldId?.ToString());
            br.RetrieveRecords();
            var lst = br.GetRecordList(0);
            var bs = new BulkSave() { Schema = Global.Databases.MetaSchema };

            // is it new @lexicon?
            if (lst.Count > 0)
            {
                result = lst[0];
                result.SetField(Constants.MetaDataModifyStmp, DateTime.Now);
                result.SetField(Constants.LexiconFromFieldId, fromFieldId?.ToString());

                bs.UpdateRecord(result);
                bs.Save();
            }
            else
            {
                #region create new 
                result = new Record(table);
                result.SetField(Constants.MetaDataId, GetLexiconId());
                result.SetField(Constants.MetaDataSchemaId, schemaId);
                result.SetField(Constants.MetaDataName, GetLexiconName(schemaId, tableId, toFieldId, relationId, relationValue));
                result.SetField(Constants.MetaDataDescription, null);
                result.SetField(Constants.LexiconGuid, GetLexiconGuid());
                result.SetField(Constants.LexiconTableId, tableId?.ToString());
                result.SetField(Constants.LexiconFromFieldId, fromFieldId?.ToString());
                result.SetField(Constants.LexiconToFieldId, toFieldId?.ToString());
                result.SetField(Constants.MetaDataModifyStmp, DateTime.Now);
                result.SetField(Constants.MetaDataActive, true);
                result.SetField(Constants.LexiconRelationId, null); //TODO manage relationId
                result.SetField(Constants.LexiconRelationValue, null);

                bs.InsertRecord(result);
                bs.Save();
                #endregion
            }
            return GetInstance(result, null);
        }

        #region private methods

        /// <summary>
        /// Generate lexicon name from id information
        /// </summary>
        /// <param name="schemaId"></param>
        /// <param name="tableId"></param>
        /// <param name="fieldId"></param>
        /// <param name="relationId"></param>
        /// <param name="relationValue"></param>
        /// <returns></returns>
        private static string GetLexiconName(int schemaId, int? tableId, int? fieldId, int? relationId, string relationValue)
        {
            var result = new StringBuilder();
            // no schema info - usless
            //result.Append(schemaId.ToString());
            if (tableId != null)
            {
                //result.Append(Constants.LexiconNameSperator);
                var schema = Global.Databases.GetSchema(schemaId);
                var table = schema.GetTable((int)tableId);
                result.Append(table?.Name);
                if (fieldId != null)
                {
                    result.Append(Constants.LexiconNameSperator);
                    var field = table.GetField((int)fieldId);
                    result.Append(field?.Name);
                }
                //TODO manage name for relationId
                if (relationId != null && relationValue != null)
                {
                }

            }
            return result.ToString();
        }

        /// <summary>
        /// Generate lexicon id
        /// </summary>
        /// <returns></returns>
        private static int GetLexiconId() => (int)(Global.SequenceLexiconId.NextValue() & int.MaxValue);

        /// <summary>
        /// Generate lexicon name from id information
        /// </summary>
        private static string GetLexiconGuid()
        {
            var result = Guid.NewGuid().ToString();
            var guidExist = true;

            // max loop = 100
            for (var i = 0; i < 100 && guidExist; ++i)
            {
                var br = new BulkRetrieve { Schema = Global.Databases.MetaSchema };
                br.SimpleQuery(0, Constants.LexiconTableName);
                br.AppendFilter(0, Constants.LexiconGuid, OperationType.Equal, result);
                br.RetrieveRecords();

                if (br.GetRecordList(0).Count > 0) result = Guid.NewGuid().ToString();
                else guidExist = false;
            }
            return result;
        }

        /// <summary>
        /// Generate dictinc list of lexico_itm.id where reference_id >= 0
        /// </summary>
        /// <param name="lexiconItemList"></param>
        /// <returns></returns>
        private Language[] Getlanguages(List lexiconItemList)
        {
            var subResult = new List(ItemType.Integer);
            var tempDico = new System.Collections.Generic.SortedDictionary<int, bool>();
            for (var i = 0; i < lexiconItemList.Count; ++i)
            {
                var rcd = lexiconItemList[i];
                if (rcd.IsEmpty(Constants.MetaDataRefId)) continue;
                int id;
                rcd.GetField(Constants.MetaDataId, out id);
                if (tempDico.ContainsKey(id)) continue;
                tempDico.Add(id, false);
                subResult.AppendItem(id);
            }
            var br = new BulkRetrieve { Schema = Global.Databases.MetaSchema };
            br.SimpleQuery(0, Constants.LexiconItemTableName);
            br.AppendFilter(0, Constants.EntityId, OperationType.In, subResult);
            br.RetrieveRecords();
            var result = new System.Collections.Generic.List<Language>();
            var lst = br.GetRecordList(0);
            for (var i = 0; i < lst.Count; ++i)
            {
                var language = _languageBuilder.GetInstance(lst[i]);
                if (language != null) result.Add(language);
            }
            // sort list of language
            result.Sort((x, y) => string.CompareOrdinal(x.NameUpper, y.NameUpper));
            return result.ToArray();
        }

        /// <summary>
        /// Load translation dictionnaries
        /// </summary>
        /// <param name="lexicon"></param>
        /// <param name="lexiconItemList"></param>
        private static void LoadTranslation(Lexicon lexicon, List lexiconItemList)
        {
            //TODO improve perf: high memory usage !!!
            if (lexiconItemList == null || lexicon?.Languages == null || lexicon.Languages.Length == 0) return;
            var sourceRefDico = new System.Collections.Generic.SortedDictionary<string, string>();    // <value, value>
            var translateRefDico = new System.Collections.Generic.SortedDictionary<string, string>(); // <value, value>
            var sourceRefRefIdDico = new System.Collections.Generic.Dictionary<string, string>();     // <sourceId, SourceValue>
            var translateList = new List(ItemType.Record, ((lexiconItemList.Count * 2) / 3) + 1);

            #region load references dico
            for (var i = 0; i < lexiconItemList.Count; ++i)
            {
                var rcd = lexiconItemList[i];
                var element = rcd.GetField(Constants.LexiconItemValue);
                // source element !!
                if (element == null) continue;
                if (rcd.IsEmpty(Constants.LexiconItemRefId))
                {
                    // using a key ?
                    var newUpperValue = element.ToUpper();
                    var souceId = rcd.GetField();
                    if (!sourceRefDico.ContainsKey(newUpperValue)) sourceRefDico.Add(newUpperValue, newUpperValue);
                    if ((!lexicon.UpperCaseSearch) && !sourceRefDico.ContainsKey(element))
                        sourceRefDico.Add(element, element);
                    if (!sourceRefRefIdDico.ContainsKey(souceId)) sourceRefRefIdDico.Add(souceId, element);
                }
                else
                {
                    if (!translateRefDico.ContainsKey(element)) translateRefDico.Add(element, element);
                    translateList.AppendItem(rcd);
                }
            }
            #endregion
            #region allow dico structure to lexicon.Translations
            var sourceRefArray = sourceRefDico.ToArray();
            for (var i = 0; i < lexicon.Translations.Length; ++i)
            {
                var translationDico = new System.Collections.Generic.Dictionary<string, string>(sourceRefDico.Count);
                lexicon.Translations[i] = translationDico;
                var languageId = lexicon.Languages[i].Id.ToString(); // get current language
                for (var j = 0; j < sourceRefArray.Length; ++j)
                    lexicon.Translations[i].Add(sourceRefArray[j].Value, null);
                for (var j = 0; j < translateList.Count; ++j)
                {
                    var rcd = translateList[j];
                    if (languageId == rcd.GetField())
                    {
                        var sourceId = rcd.GetField(Constants.LexiconItemRefId);
                        if (sourceRefRefIdDico.ContainsKey(sourceId))
                        {
                            var sourceValue = sourceRefRefIdDico[sourceId];
                            if (translationDico.ContainsKey(sourceValue))
                                translationDico[sourceValue] = translateRefDico[rcd.GetField(Constants.LexiconItemValue)];
                            if (translationDico.ContainsKey(sourceValue.ToUpper()))
                                translationDico[sourceValue.ToUpper()] = translateRefDico[rcd.GetField(Constants.LexiconItemValue)];
                        }
                    }
                }
            }
            #endregion
        }

        #endregion 

    }
}
