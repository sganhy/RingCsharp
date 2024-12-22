using Ring.Schema.Models;
using System.Text;

namespace Ring.Schema.Extensions;

internal static class BaseEntityExtensions
{
	private const char HashCodeSeparator = (char)8888;

	internal static string GetStringCode(this BaseEntity baseEntity)
	{
        // Code size: 108 (0x6c)
        var result = new StringBuilder();
		result.Append(baseEntity.Active)
			.Append(HashCodeSeparator)
			.Append(baseEntity.Baseline)
			.Append(HashCodeSeparator)
			.Append(baseEntity.Description)
			.Append(HashCodeSeparator)
			.Append(baseEntity.Id)
			.Append(HashCodeSeparator)
			.Append(baseEntity.Name);
		return result.ToString();
	}

}