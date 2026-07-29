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

	internal static OperationalError ParseErrorFields(this byte[] body)
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
		// string message, string state, string severity, string? detail, string hint, string? tableName
		var result = new OperationalError
		{
			Message = message ?? string.Empty,
			SqlState = sqlState ?? string.Empty,
			Severity = severity ?? string.Empty,
			Detail = detail,
			Hint = hint
		};
		return result;
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
