namespace Ring.Util.Models;

internal sealed class SqlEntry
{
	readonly internal string? Text;
	readonly internal byte[] Encoded;

	/// <summary>
	///		Ctor
	/// </summary>
	internal SqlEntry(string? text, byte[] encoded)
	{
		Text = text;
		Encoded = encoded;
	}
}	
