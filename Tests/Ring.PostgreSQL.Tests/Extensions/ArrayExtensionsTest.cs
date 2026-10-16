using Ring.Data;
using Ring.PostgreSQL.Enums;
using Ring.PostgreSQL.Extensions;
using System.Buffers;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace Ring.PostgreSQL.Tests.Extensions;

public class ArrayExtensionsTest : BaseTest
{
	public ArrayExtensionsTest(ITestOutputHelper output) : base(output) => Expression.Empty();
	
	[Fact]
	public void ParseErrorFields_ValidBody_ReturnsParsedOperationalError()
	{
		// Arrange: Build PostgreSQL error fields payload (FieldCode + Null-terminated UTF8 string), terminated by 0 byte.
		// Constants: Severity = 'S', Code = 'C', Message = 'M', Detail = 'D', Hint = 'H', TableName = 'T', SchemaName = 'N'
		byte severityCode = (byte)ErrorTypeCode.Severity;
		byte errorCode = (byte)ErrorTypeCode.Code;
		byte errorMessage = (byte)ErrorTypeCode.Message;
		byte errorDetail = (byte)ErrorTypeCode.Detail;
		byte errorHint = (byte)ErrorTypeCode.Hint;
		byte errorTableName = (byte)ErrorTypeCode.TableName;
		byte errorSchemaName = (byte)ErrorTypeCode.SchemaName;

		var payload = new List<byte>();

		payload.Add(severityCode);
		payload.AddRange(Encoding.UTF8.GetBytes("ERROR"));
		payload.Add(0);

		payload.Add(errorCode);
		payload.AddRange(Encoding.UTF8.GetBytes("23505"));
		payload.Add(0);

		payload.Add(errorMessage);
		payload.AddRange(Encoding.UTF8.GetBytes("duplicate key value"));
		payload.Add(0);

		payload.Add(errorDetail);
		payload.AddRange(Encoding.UTF8.GetBytes("Key already exists."));
		payload.Add(0);

		payload.Add(errorHint);
		payload.AddRange(Encoding.UTF8.GetBytes("Check unique constraint."));
		payload.Add(0);

		payload.Add(errorTableName);
		payload.AddRange(Encoding.UTF8.GetBytes("users"));
		payload.Add(0);

		payload.Add(errorSchemaName);
		payload.AddRange(Encoding.UTF8.GetBytes("public"));
		payload.Add(0);

		payload.Add(0); // Termination byte

		Span<byte> span = CollectionsMarshal.AsSpan(payload); // or payload.ToArray()

		// Act
		var error = span.ParseErrorFields();

		// Assert
		Assert.Equal(OperationalErrorType.PgError, error.Type);
		Assert.Equal("ERROR", error.Severity);
		Assert.Equal("23505", error.SqlState);
		Assert.Equal("duplicate key value", error.Message);
		Assert.Equal("Key already exists.", error.Detail);
		Assert.Equal("Check unique constraint.", error.Hint);
		Assert.Equal("users", error.TableName);
		Assert.Equal("public", error.SchemaName);
	}

	[Fact]
	public void ParseErrorFieldsFromSequence_SingleSegment_ReturnsParsedError()
	{
		// Arrange
		byte errorCode = (byte)ErrorTypeCode.Code;
		var bytes = new byte[] { errorCode, (byte)'4', (byte)'2', (byte)'P', (byte)'0', (byte)'1', 0, 0 };
		var sequence = new ReadOnlySequence<byte>(bytes);

		// Act
		var error = ArrayExtensions.ParseErrorFieldsFromSequence(sequence);

		// Assert
		Assert.Equal("42P01", error.SqlState);
	}

	[Fact]
	public void ParseErrorFieldsFromSequence_MultiSegmentSmallAndLarge_ReturnsParsedError()
	{
		// Arrange
		// Test both small (< StackallocThreshold) and multi-segment sequences
		byte errorCode = (byte)ErrorTypeCode.Code;
		byte errorMessage = (byte)ErrorTypeCode.Message;

		var segment1 = new byte[] { errorCode, (byte)'0', (byte)'8', (byte)'0', (byte)'0', (byte)'6', 0 };
		var segment2 = new byte[] { errorMessage, (byte)'c', (byte)'o', (byte)'n', (byte)'n', (byte)'e', (byte)'c', (byte)'t', (byte)'i', (byte)'o', (byte)'n', (byte)' ', (byte)'f', (byte)'a', (byte)'i', (byte)'l', (byte)'e', (byte)'d', 0, 0 };

		// Create a multi-segment ReadOnlySequence using linked nodes
		var firstNode = new BufferSegment<byte>(segment1);
		var lastNode = firstNode.Append(segment2);

		var sequence = new ReadOnlySequence<byte>(firstNode, 0, lastNode, segment2.Length);

		// Act
		var error = ArrayExtensions.ParseErrorFieldsFromSequence(sequence);

		// Assert
		Assert.Equal("08006", error.SqlState);
		Assert.Equal("connection failed", error.Message);
	}

	[Fact]
	public void ParseByteaHexToBase64_ValidShortHex_ReturnsBase64String()
	{
		// Arrange: Postgres hex format '\x' + hex bytes for "Hello" ('H'=48, 'e'=65, 'l'=6c, 'l'=6c, 'o'=6f)
		// Hex: 48656c6c6f
		string hexStr = "\\x48656c6c6f";
		byte[] bytes = Encoding.ASCII.GetBytes(hexStr);

		// Act
		string base64 = bytes.AsSpan().ParseByteaHexToBase64(0, bytes.Length);

		// Assert
		string expectedBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes("Hello"));
		Assert.Equal(expectedBase64, base64);
	}

	[Fact]
	public void ParseByteaHexToBase64_ValidLongHex_ReturnsBase64String()
	{
		// Arrange: Long sequence (> StackallocThreshold of 128 bytes) to test ArrayPool path
		var rawData = new byte[150];
		for (int i = 0; i < rawData.Length; i++) rawData[i] = (byte)(i % 256);

		var sb = new StringBuilder("\\x");
		foreach (var b in rawData) sb.Append(b.ToString("x2"));

		byte[] bytes = Encoding.ASCII.GetBytes(sb.ToString());

		// Act
		string base64 = bytes.AsSpan().ParseByteaHexToBase64(0, bytes.Length);

		// Assert
		string expectedBase64 = Convert.ToBase64String(rawData);
		Assert.Equal(expectedBase64, base64);
	}

	[Theory]
	[InlineData("X4865")] // Missing backslash
	[InlineData("\\y4865")] // Missing 'x'
	[InlineData("\\x485")] // Odd number of hex digits
	[InlineData("\\")]    // Too short (length 1 < 2)
	[InlineData("")]      // Empty (length 0 < 2)
	public void ParseByteaHexToBase64_InvalidFormat_ThrowsFormatException(string invalidHex)
	{
		// Arrange
		byte[] bytes = Encoding.ASCII.GetBytes(invalidHex);

		// Act & Assert
		Assert.Throws<FormatException>(() => bytes.AsSpan().ParseByteaHexToBase64(0, bytes.Length));
	}

	private sealed class BufferSegment<T> : ReadOnlySequenceSegment<T>
	{
		public BufferSegment(T[] array)
		{
			Memory = array;
		}

		public BufferSegment<T> Append(T[] array)
		{
			var segment = new BufferSegment<T>(array)
			{
				RunningIndex = RunningIndex + Memory.Length
			};
			Next = segment;
			return segment;
		}
	}

}
