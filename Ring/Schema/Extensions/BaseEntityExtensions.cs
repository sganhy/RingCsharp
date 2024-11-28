using Ring.Schema.Models;
using Ring.Util.Helpers;
using System.Text;

namespace Ring.Schema.Extensions;

internal static class BaseEntityExtensions
{
	private const char HashCodeSeparator = (char)555;

	internal static long GetHashCode(this BaseEntity baseEntity)
	{
		HashHelper.Djb2X(GetStringCode(baseEntity), out long hash);
		return hash;
	}

	internal static string GetStringCode(this BaseEntity baseEntity)
	{
		var result = new StringBuilder();
		result.Append(baseEntity.Active);
		result.Append(HashCodeSeparator);
		result.Append(baseEntity.Baseline);
		result.Append(HashCodeSeparator);
		result.Append(baseEntity.Description);
		result.Append(HashCodeSeparator);
		result.Append(baseEntity.Id);
		result.Append(HashCodeSeparator);
		result.Append(baseEntity.Name);
		return result.ToString();
	}

}