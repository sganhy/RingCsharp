using Ring.Schema.Models;
using System.Text;

namespace Ring.Schema.Extensions;

internal static class BaseEntityExtensions
{
	private const char HashCodeSeparator = (char)8888;

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