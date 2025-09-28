using Ring.Schema.Models;
using System.Text;

namespace Ring.Schema.Extensions;

internal static class BaseEntityExtensions
{
	private const char HashCodeSeparator = (char)8998;

    //  Code size: 101 (0x65) - checked 2025-07-19
    internal static StringBuilder GetStringCode(this BaseEntity baseEntity) 
		=> new StringBuilder()
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