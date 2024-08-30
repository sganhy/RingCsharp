namespace Ring.Schema.Models;

internal abstract class BaseEntity
{
	internal readonly bool Active;
	internal readonly bool Baseline;
	internal readonly string? Description;
	internal readonly int Id;
	internal readonly string Name;

	/// <summary>
	///     Ctor
	/// </summary>
	/// <param name="id">BaseEntity id</param>
	/// <param name="name">BaseEntity name</param>
	/// <param name="description">BaseEntity description</param>
	/// <param name="active">Is entity enabled</param>
	/// <param name="baseline">Is entity baselined</param>
	protected BaseEntity(int id, string name, string? description, bool active, bool baseline)
	{
		Id = id;
		Name = name;
		Description = description;
		Active = active;
		Baseline = baseline;
	}

#if DEBUG
	public override string ToString() => $"{Id} - {Name}";
#endif

}