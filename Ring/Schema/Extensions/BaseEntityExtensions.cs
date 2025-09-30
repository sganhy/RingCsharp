using Ring.Schema.Models;
using System.Text;

namespace Ring.Schema.Extensions;

internal static class BaseEntityExtensions
{
	private const int BoolMaxLength = 5;			   // "False".Length
	private const char HashCodeSeparator = (char)8996;

    // Code size: 116 (0x74) - checked 2025-07-19
    internal static StringBuilder GetStringCode(this BaseEntity baseEntity) 
		=> new StringBuilder(baseEntity.GetStringCodeLength()) // compute capacity! 
			.Append(baseEntity.Active)
			.Append(HashCodeSeparator)
			.Append(baseEntity.Baseline)
			.Append(HashCodeSeparator)
			.Append(baseEntity.Description ?? string.Empty)
			.Append(HashCodeSeparator)
			.Append(baseEntity.Id)
			.Append(HashCodeSeparator)
			.Append(baseEntity.Name); // name is an mandatory field!

	internal static int GetStringCodeLength(this BaseEntity baseEntity)  // Code size: 48 (0x30) - checked 2025-09-30
		=> BoolMaxLength +							 // Active: "True" or "False"
			BoolMaxLength +							 // Baseline: "True" or "False"
			(baseEntity.Description?.Length ?? 0) +	 // Description (nullable)
			baseEntity.Id.GetInt32Length() +		 // Id (worst case: -2147483648)
			baseEntity.Name.Length +				 // Name (nullable)
			5;										 // 4 separators + 1 char	

}
