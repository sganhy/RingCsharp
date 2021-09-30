using Ring.Data;
using Ring.Data.Core;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Models;
using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace Ring.Schema.Builders
{
    internal sealed class LanguageBuilder : EntityBuilder
    {
        // <languageName.ToUpper(),>
        private static readonly ConcurrentDictionary<string, Language> LanguageCache = new ConcurrentDictionary<string, Language>();

        public Language GetInstance(MetaData meta)
        {
            var cultureInfo = new CultureInfo(meta.Name);
            var result = new Language(int.Parse(meta.Id), meta.Name,
                string.IsNullOrWhiteSpace(meta.Description) ? cultureInfo.DisplayName : meta.Description,
                cultureInfo, meta.Name?.ToUpper(), meta.IsEnabled(), meta.IsBaselined());
            return result;
        }

        public Language GetInstance(Record record)
        {
            if (record == null || record.RecordType != Constants.LexiconItemTableName) return null;
            int id;
            record.GetField(Constants.MetaDataId, out id);
            var cultureName = record.GetField(Constants.MetaDataValue)?.ToUpper();
            if (cultureName == null) return null;
            if (!Constants.Cultures.ContainsKey(cultureName)) return null;
            if (LanguageCache.ContainsKey(cultureName)) return LanguageCache[cultureName];
            var culture = Constants.Cultures[cultureName];
            var result = new Language(id, culture.Name, string.Empty, culture, culture.Name.ToUpper(), true, true);
            LanguageCache.TryAdd(cultureName, result);
            return result;
        }

        public Language GetInstance(string languageName)
        {
            if (languageName == null) return null;
            languageName = languageName.Replace('_', '-').Trim();
            Language result = null;
            if (Constants.Cultures.ContainsKey(languageName.ToUpper()))
            {
                if (!LanguageCache.ContainsKey(languageName.ToUpper()))
                {
                    var culture = Constants.Cultures[languageName.ToUpper()];
                    var br = new BulkRetrieve { Schema = Global.Databases.MetaSchema };
                    br.SimpleQuery(0, Constants.LexiconItemTableName);
                    br.AppendFilter(0, Constants.LexiconItemLexiId, OperationType.Equal, null as string);
                    br.AppendFilter(0, Constants.MetaDataRefId, OperationType.Equal, null as string);
                    br.RetrieveRecords();
                    var lst = br.GetRecordList(0);
                    for (var i = 0; i < lst.Count; ++i)
                    {
                        var language = GetInstance(lst[i]);
                        if (string.Equals(languageName, language.NameUpper, StringComparison.OrdinalIgnoreCase)) result = language;
                    }
                    if (result != null) return result;
                    // create new language
                    var schema = Global.Databases.MetaSchema;
                    var bs = new BulkSave { Schema = schema };
                    var sequence = Global.SequenceLanguageId;
                    var table = schema.GetTable(Constants.LexiconItemTableName);
                    var rcd = new Record(table);
                    rcd.SetField(Constants.MetaDataId, (int)sequence.NextValue() & int.MaxValue);
                    rcd.SetField(Constants.MetaDataValue, culture.Name);
                    bs.InsertRecord(rcd);
                    bs.Save();
                    return GetInstance(rcd);
                }
                result = LanguageCache[languageName.ToUpper()];
            }
            return result;
        }

        public Language GetDefaultInstance()
        {
            var result = new Language(0, Constants.DefaultLanguage, Constants.DefaultCultureInfo.DisplayName, Constants.DefaultCultureInfo,
                Constants.DefaultLanguage.ToUpper(), true, true);
            return result;
        }

    }
}
