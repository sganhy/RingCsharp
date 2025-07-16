using Ring.Schema.Models;
using System.Text;

namespace Ring.Schema.Extensions;

internal static class BaseEntityExtensions
{
	private const char HashCodeSeparator = (char)8998;

	// Code size: 106 (0x6a)
	internal static string GetStringCode(this BaseEntity baseEntity) 
		=> new StringBuilder()
			.Append(baseEntity.Active)
			.Append(HashCodeSeparator)
			.Append(baseEntity.Baseline)
			.Append(HashCodeSeparator)
			.Append(baseEntity.Description)
			.Append(HashCodeSeparator)
			.Append(baseEntity.Id)
			.Append(HashCodeSeparator)
			.Append(baseEntity.Name)
			.ToString();

}