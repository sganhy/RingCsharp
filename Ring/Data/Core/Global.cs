using Ring.Schema.Builders;
using Ring.Schema.Core.Extensions;
using Ring.Schema.Models;

namespace Ring.Data.Core
{
    internal static class Global
    {

        // !!!! schema current schema collection !!!!!!
        internal static readonly DatabaseCollection Databases = LoadSchemaCollection();

        // int id,int schemaId, string refenceId
        internal static readonly Sequence SequenceJobId = SequenceBuilder.GetJobIdInstance(0, Constants.DefaultMetaSchemaId);
        internal static readonly Sequence SequenceLexiconId = SequenceBuilder.GetLexiconIdInstance(1, Constants.DefaultMetaSchemaId);
        internal static readonly Sequence SequenceLanguageId = SequenceBuilder.GetLanguageIdInstance(2, Constants.DefaultMetaSchemaId);
        internal static readonly Sequence SequenceUserId = SequenceBuilder.GetUserIdInstance(3, Constants.DefaultMetaSchemaId);
        internal static readonly Sequence SequenceIndexId = SequenceBuilder.GetIndexIdInstance(4, Constants.DefaultMetaSchemaId);
        internal static readonly Sequence SequenceEventId = SequenceBuilder.GetEventIdInstance(5, Constants.DefaultMetaSchemaId);

        /// <summary>
        /// load Global sequences eg. @lexicon_id, @job_id
        /// </summary>
        internal static void LoadGlobalSequences()
        {
            if (!SequenceJobId.Exists()) SequenceJobId.Create();
            if (!SequenceLexiconId.Exists()) SequenceLexiconId.Create();
            if (!SequenceLanguageId.Exists()) SequenceLanguageId.Create();
            if (!SequenceUserId.Exists()) SequenceUserId.Create();
            if (!SequenceIndexId.Exists()) SequenceIndexId.Create();
            if (!SequenceEventId.Exists()) SequenceEventId.Create();

            SequenceJobId.Load();
            SequenceLexiconId.Load();
            SequenceLanguageId.Load();
            SequenceUserId.Load();
            SequenceIndexId.Load();
            SequenceEventId.Load();
        }

        #region private methods

        private static DatabaseCollection LoadSchemaCollection() => new DatabaseCollection();

        #endregion

    }
}
