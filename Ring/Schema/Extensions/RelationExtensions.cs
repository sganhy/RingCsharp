using Ring.Schema.Enums;
using Ring.Schema.Models;
using Ring.Util.Helpers;
using System.Globalization;
using System.Text;

namespace Ring.Schema.Extensions;

internal static class RelationExtensions
{
	private const char MtmSeparator = '_';
	private const char PaddingChar = '0';
	private const char HashCodeSeparator = (char)2222;

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
		var meta = relation.ToMeta(-1, relationType);
		return meta.ToRelation(relation.ToTable) ?? relation;
	}

	internal static long GetHashCode(this Relation relation)
	{
		HashHelper.Djb2X(relation.GetStringCode(), out long hash);
		return hash;
	}

	internal static string GetStringCode(this Relation relation)
	{
		/*
		 * Relation InverseRelation
		 * readonly bool HasConstraint
		 * readonly bool NotNull
		 * readonly Table ToTable
		 * readonly RelationType Type
		 * readonly FieldType FieldType
		 */
		var result = new StringBuilder();
		result.Append(relation.InverseRelation.Name);
		result.Append(relation.InverseRelation.Id);
		result.Append(HashCodeSeparator);
		result.Append(relation.InverseRelation.Type.ToString());
		result.Append(HashCodeSeparator);
		result.Append(relation.HasConstraint);
		result.Append(HashCodeSeparator);
		result.Append(relation.NotNull);
		result.Append(HashCodeSeparator);
		result.Append(relation.ToTable.Id);
		result.Append(relation.ToTable.Name);
		result.Append(HashCodeSeparator);
		result.Append(relation.Type.ToString());
		result.Append(HashCodeSeparator);
		result.Append(relation.FieldType.ToString());
		// BaseEntity
		result.Append(BaseEntityExtensions.GetStringCode(relation));
		return result.ToString();
	}

	internal static Relation SetRecordIndex(this Relation relation, int recordIndex)
	{
		var result = new Relation(relation.Id, relation.Name, relation.Description, relation.Type, relation.ToTable, recordIndex,
			relation.FieldType, relation.NotNull, relation.HasConstraint, relation.Baseline, relation.Active);
		// manage inverse relationship
		result.SetInverseRelation(relation.InverseRelation);
		relation.InverseRelation.SetInverseRelation(result);
		return result;
	}
}
