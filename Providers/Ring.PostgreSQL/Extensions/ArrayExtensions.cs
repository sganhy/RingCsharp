using Ring.Data;
using Ring.PostgreSQL.Enums;
using System.Text;

namespace Ring.PostgreSQL.Extensions;

internal static class ArrayExtensions
{
	private const byte ErrorSeverity = (byte)ErrorTypeCode.Severity;
	private const byte ErrorCode = (byte)ErrorTypeCode.Code;
	private const byte ErrorMessage = (byte)ErrorTypeCode.Message;
	private const byte ErrorDetail = (byte)ErrorTypeCode.Detail;
	private const byte ErrorHint = (byte)ErrorTypeCode.Hint;

	internal static ConnectionOperationalError ParseErrorResponse(this byte[] body, string? tableName)
	{
		string? severity = null, sqlState = null, message = null, detail = null, hint = null;
		var offset = 0;
		while (offset < body.Length && body[offset] != 0)
		{
			var field = body[offset++];
			var value = ReadCString(body, ref offset);
			switch (field)
			{
				case ErrorSeverity: severity = value; break;
				case ErrorCode: sqlState = value; break;
				case ErrorMessage: message = value; break;
				case ErrorDetail: detail = value; break;
				case ErrorHint: hint = value; break;
			}
		}
		return new ConnectionOperationalError(message ?? "An error was returned by the server.", sqlState ?? string.Empty, severity ?? string.Empty, detail, tableName);
	}

	#region private methods 

	private static string ReadCString(byte[] data, ref int offset)
	{
		// Code size: 47 (0x2f)
		var start = offset;
		while (offset < data.Length && data[offset] != 0) offset++;
		var value = Encoding.UTF8.GetString(data, start, offset - start);
		offset++; // skip null terminator
		return value;
	}

	#endregion 

}
