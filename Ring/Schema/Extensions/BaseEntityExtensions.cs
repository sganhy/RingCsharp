using Ring.Schema.Models;
using System.Text;

namespace Ring.Schema.Extensions;

internal static class BaseEntityExtensions
{
	private const char HashCodeSeparator = (char)8998;

	// Code size: 198 (0xc6) - checked 2025-07-19
	internal static StringBuilder GetStringCode(this BaseEntity baseEntity) 
		=> new StringBuilder(25 + baseEntity.Description?.Length ?? 0 + baseEntity.Name.Length)
			.Append(baseEntity.Active)       // 5 chars max
			.Append(HashCodeSeparator)       // 1 char
			.Append(baseEntity.Baseline)     // 5 chars max
			.Append(HashCodeSeparator)       // 1 char
			.Append(baseEntity.Description)
			.Append(HashCodeSeparator)       // 1 char 
			.Append(baseEntity.Id)           // 11 chars max
			.Append(HashCodeSeparator)       // 1 char
			.Append(baseEntity.Name);

}