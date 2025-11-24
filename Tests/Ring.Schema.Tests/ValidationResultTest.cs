using Microsoft.Extensions.Logging;
using Ring.Schema.Enums;
using System.Globalization;
using Xunit;

namespace Ring.Schema.Tests;

public sealed class ValidationResultTest
{
	private readonly static long MaxFileSize = 500L;

	[Fact]
	public void AddItem_FileToolLarge_ReturnMessage()
	{
		// arrange 
		var validationResult = new ValidationResult();
		validationResult.AddItem(LogType.FileToolLarge, MaxFileSize.ToString(CultureInfo.InvariantCulture));

		// act 
		var lastItem = validationResult.Validations.Last();

		// assert 
		Assert.Equal(1, validationResult.CriticalCount);
		Assert.Equal("File too large", lastItem.Name);
		Assert.Equal(LogLevel.Critical, lastItem.Level);
		Assert.Equal("File size exceeds the maximum allowed size of 500 bytes.", lastItem.Description);
	}
}

