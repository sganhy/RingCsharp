using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Globalization;
using System.Text;

namespace Ring.Schema.Extensions;

internal static class RelationExtensions
{
    private const char MtmSeparator = '_';
    private const char PaddingChar = '0';

    internal static Meta ToMeta(this Relation relation, int fromTableId)
    {
        var meta = new Meta();
        meta.SetEntityType(EntityType.Relation);
        meta.SetEntityId(relation.Id);
        meta.SetEntityName(relation.Name);
        meta.SetEntityDescription(relation.Description);
        meta.SetEntityRefId(fromTableId);
        meta.SetEntityBaseline(relation.Baseline);
        meta.SetInverseRelation(relation.InverseRelation.Name);
        meta.SetEntityActive(relation.Active);
        meta.SetEntityBaseline(relation.Baseline);
        meta.SetRelationType(relation.Type);
        meta.SetRelationConstraint(relation.HasConstraint);
        meta.SetRelationToTable(relation.ToTable.Type==TableType.Mtm ? 
            (relation.ToTable.GetRelation(relation.Name)?? relation).ToTable.Id : relation.ToTable.Id);
        meta.SetRelationdNotNull(relation.NotNull);
        return meta;
    }

    internal static string GetMtmName(this Relation relation)
    {
        // mtm relation already computed - find previous table_id
        var toTableId = relation.ToTable.Type == TableType.Mtm? 
            (relation.ToTable.GetRelation(relation.Name) ?? relation).ToTable.Id :  relation.ToTable.Id;
        var fromTableId = relation.InverseRelation.ToTable.Id;
        var sfromTableId = fromTableId.ToString(CultureInfo.InvariantCulture)?.PadLeft(5, PaddingChar);
        var sToTableId = toTableId.ToString(CultureInfo.InvariantCulture)?.PadLeft(5, PaddingChar);
        var result = new StringBuilder();
        int relId;

        if (fromTableId < toTableId)
        {
            relId = relation.Id;
            _ = result.Append(sfromTableId)
                  .Append(MtmSeparator)
                  .Append(sToTableId);
        }
        else
        {
            relId = relation.InverseRelation.Id;
            _ = result.Append(sToTableId)
                  .Append(MtmSeparator)
                  .Append(sfromTableId);
        }
        _ = result.Append(MtmSeparator);

        if (fromTableId != toTableId) _ = result.Append(relId.ToString(CultureInfo.InvariantCulture).PadLeft(3, PaddingChar));
        else _ = result.Append(Math.Min(relation.Id, relation.InverseRelation.Id).ToString(CultureInfo.InvariantCulture)
            .PadLeft(3, PaddingChar));

        return result.ToString();
    }

    internal static bool Initialized(this Relation relation)
        => !ReferenceEquals(relation.InverseRelation, relation) && 
           (relation.Type != RelationType.Mtm || relation.ToTable.Type == TableType.Mtm);

    internal static Relation GetRelation(this Relation relation, RelationType relationType)
    {
        var meta = relation.ToMeta(-1);
        meta.SetRelationType(relationType);
        return meta.ToRelation(relation.ToTable) ?? relation;
    }

}
