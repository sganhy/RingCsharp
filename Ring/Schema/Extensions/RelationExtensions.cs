using Ring.Schema.Enums;
using Ring.Schema.Models;
using System.Globalization;
using System.Text;

namespace Ring.Schema.Extensions;

internal static class RelationExtensions
{
	private const char MtmSeparator = '_';
	private const char PaddingChar = '0';

	internal static Meta ToMeta(this Relation relation, int fromTableId, RelationType? newRelationType=null)
	{
		var flags = 0L;
		flags = Meta.SetEntityBaseline(flags, relation.Baseline);
		flags = Meta.SetRelationdNotNull(flags, relation.NotNull);
		flags = Meta.SetRelationConstraint(flags, relation.HasConstraint);
		flags = Meta.SetRelationType(flags, newRelationType ?? relation.Type);
		var meta = new Meta(relation.Id, (byte)EntityType.Relation, fromTableId, relation.ToTable.Type == TableType.Mtm ?
			(relation.ToTable.GetRelation(relation.Name) ?? relation).ToTable.Id : relation.ToTable.Id, flags, relation.Name, relation.Description, 
			 relation.InverseRelation.Name, relation.Active);
		return meta;
	}

	internal static string GetMtmName(this Relation relation)
	{
		// Code size: 302 (0x12e)
		// mtm relation already computed - find previous table_id
		var toTableId = relation.ToTable.Type == TableType.Mtm? (relation.ToTable.GetRelation(relation.Name) ?? relation).ToTable.Id : relation.ToTable.Id;
		var fromTableId = relation.InverseRelation.ToTable.Id;
		var sfromTableId = fromTableId.ToString(CultureInfo.InvariantCulture).PadLeft(5, PaddingChar);
		var sToTableId = toTableId.ToString(CultureInfo.InvariantCulture).PadLeft(5, PaddingChar);
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

	internal static bool Initialized(this Relation relation) => // Code size: 38 (0x26)
		!ReferenceEquals(relation.InverseRelation, relation) && (relation.Type != RelationType.Mtm || relation.ToTable.Type == TableType.Mtm);

	internal static Relation SetTypeAndId(this Relation relation, RelationType relationType, int id, bool notNull) => // Code size: 56 (0x38)
		new (id, relation.Name, relation.Description, relationType, relation.ToTable, relation.FieldType, notNull, relation.HasConstraint, relation.Baseline, relation.Active);

    internal static int Hash(this Relation relation)
    {
        return -1;
    }

    internal static bool IsEquivalentTo(this Relation relation, Relation? other)
    {
        // Code size: 105 (0x69)
        if (!relation.BaseEntityEquals(other)) return false;
        // other cannot be null here 
        return relation.HasConstraint == other!.HasConstraint && relation.NotNull == other.NotNull && relation.ToTable.SchemaId == other.ToTable.SchemaId;
    }

}
