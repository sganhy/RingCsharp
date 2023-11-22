using Ring.Schema.Enums;
using Ring.Schema.Models;
using Index = Ring.Schema.Models.Index;

namespace Ring.Schema.Extensions;

internal static class IndexExtensions
{
    internal static Meta ToMeta(this Index index, int tableId)
    {
        var meta = new Meta();
        // first - define Object type
        meta.SetEntityType(EntityType.Index);
        meta.SetEntityId(index.Id);
        meta.SetEntityName(index.Name);
        meta.SetEntityDescription(index.Description);
        meta.SetEntityRefId(tableId);
        meta.SetEntityBaseline(index.Baseline);
        meta.SetEntityActive(index.Active);
        meta.SetIndexUnique(index.Unique);
        meta.SetIndexBitmap(index.Bitmap);
        meta.SetIndexedColumns(index.Columns);
        return meta;
    }
}
