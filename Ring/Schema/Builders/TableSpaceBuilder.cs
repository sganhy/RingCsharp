using Ring.Schema.Core.Extensions;
using Ring.Schema.Models;

namespace Ring.Schema.Builders
{
    internal sealed class TableSpaceBuilder : EntityBuilder
    {
        public TableSpace GetInstance(MetaData meta, int schemaId) =>
            new TableSpace(
                int.Parse(meta.Id),
                meta.Name,
                meta.Description,
                meta.IsTablespaceIndex(),
                meta.IsTablespaceTable(),
                null,
                meta.Value,
                schemaId,
                meta.IsTableSpaceReadonly(),
                true,
                true);
    }
}
