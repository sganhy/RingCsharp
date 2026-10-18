namespace Ring.Schema.Models;

internal abstract class BaseEntity
{
	// Fields: 2 references (16) + int (4) + 2 bools (2) = 22 bytes, padded to 24.
	internal readonly string Name;
	internal readonly string? Description;
	internal readonly int Id;
	internal readonly bool Baseline;
	internal readonly bool Active;

	/// <summary>
	/// 	Ctor
	/// </summary>
	/// <param name="id">BaseEntity id</param>
	/// <param name="name">BaseEntity logical name</param>
	/// <param name="description">BaseEntity description</param>
	/// <param name="active">Is entity enabled</param>
	/// <param name="baseline">Is entity baselined</param>
	protected BaseEntity(int id, string name, string? description, bool baseline, bool active)
	{
		Id = id;
		Name = name;
		Description = description;
		Baseline = baseline;
		Active = active;
	}

#if DEBUG
	public override string ToString() => $"{Id} - {Name}";
#endif

}