namespace Ring.Schema.Models;

internal sealed class Lexicon : BaseEntity
{
	internal readonly int SchemaId;
	internal readonly Guid Guid;              // unique by group of translation 
	internal readonly Language Language;
	internal readonly Table? Table;
	internal readonly Field? Field;           // source_field_id
	internal readonly Relation? Relation;
	internal readonly string? Relationvalue;
	internal readonly bool UpperCaseSearch;   // force to store only source value in upper case (not both as usual)

	/// <summary>
	/// 	Ctor
	/// </summary>
	internal Lexicon(int id, string name, string physicalName, string description, int schemaId,
		Table table, Field field, Relation? relation,string? relationValue,
		Language language, Guid guid, bool upperCaseSearch, bool active, bool baseline)
		: base(id, name, physicalName, description, baseline, active)
	{
		SchemaId = schemaId;
		Table = table;
		Relation = relation;
		Relationvalue = relationValue;
		Language = language;
		Guid = guid;
		Field = field;
		UpperCaseSearch = upperCaseSearch;
	}
}