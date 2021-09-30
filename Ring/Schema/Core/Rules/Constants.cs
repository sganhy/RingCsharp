using Ring.Schema.Core.Rules.Impl;
using Ring.Schema.Models;

namespace Ring.Schema.Core.Rules
{
    internal static class Constants
    {
        // validators for metadata
        internal static readonly IValidationRule<MetaData>[] MetadaValidators =
        {
            new ValidationMetaName(),
            new ValidationMetaId(),
            new ValidationMetaDataType() ,
            new ValidationMetaRelation(),
            new ValidationMetaIndex()
        };

    }
}
