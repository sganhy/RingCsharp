using Ring.Schema.Core.Extensions;
using Ring.Schema.Models;

namespace Ring.Schema.Builders
{
    internal sealed class SequenceBuilder : EntityBuilder
    {
        public Sequence GetInstance(int schemaId, MetaData metaData)
        {
            var cacheId = metaData.GetCacheId();
            long maxValue;
            if (!long.TryParse(metaData.Value, out maxValue)) maxValue = long.MaxValue;
            var result = new Sequence(int.Parse(metaData.Id), metaData.Name, metaData.Description, schemaId,
                cacheId, maxValue, metaData.IsBaselined(), metaData.IsEnabled());
            return result;
        }

        public static Sequence GetJobIdInstance(int id, int schemaId)
        {
            var cacheId = new CacheId(new object(), Constants.DefaultJobIdValue, 0L, 0); // no cache for JobId
            var result = new Sequence(id, Constants.SequenceJobIdName, Constants.SequenceJobIdDesc, schemaId, cacheId, long.MaxValue, true, true);
            return result;
        }

        public static Sequence GetLexiconIdInstance(int id, int schemaId)
        {
            var cacheId = new CacheId(new object(), Constants.DefaultLexiconIdValue, 0L, 0); // no cache for LexiconId
            var result = new Sequence(id, Constants.SequenceLexiconIdName, Constants.SequenceLexiconIdDesc, schemaId, cacheId, int.MaxValue, true, true);
            return result;
        }

        public static Sequence GetEventIdInstance(int id, int schemaId)
        {
            var cacheId = new CacheId(new object(), Constants.DefaultEventIdValue, 0L, 0); // no cache for LexiconId
            var result = new Sequence(id, Constants.SequenceEventIdName, Constants.SequenceEventIdDesc, schemaId, cacheId, int.MaxValue, true, true);
            return result;
        }

        public static Sequence GetLanguageIdInstance(int id, int schemaId)
        {
            var cacheId = new CacheId(new object(), Constants.DefaultLanguageIdValue, 0L, 0); // no cache for LanguageId
            var result = new Sequence(id, Constants.SequenceLanguageIdName, Constants.SequenceLanguageIdDesc, schemaId, cacheId, short.MaxValue, true, true);
            return result;
        }

        public static Sequence GetIndexIdInstance(int id, int schemaId)
        {
            var cacheId = new CacheId(new object(), Constants.DefaultIndexIdValue, 0L, Constants.SequenceReservedRangeValue);
            var result = new Sequence(id, Constants.SequenceIndexIdName, Constants.SequenceIndexIdDesc, schemaId, cacheId, int.MaxValue, true, true);
            return result;
        }

        public static Sequence GetUserIdInstance(int id, int schemaId)
        {
            var cacheId = new CacheId(new object(), Constants.DefaultUserIdValue, 0L, Constants.SequenceReservedRangeValue);
            var result = new Sequence(id, Constants.SequenceUserIdName, Constants.SequenceUserIdDesc, schemaId, cacheId, int.MaxValue, true, true);
            return result;
        }

    }
}