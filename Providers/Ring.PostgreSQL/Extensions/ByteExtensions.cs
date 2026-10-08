using Ring.PostgreSQL.Enums;
using System.Runtime.CompilerServices;

namespace Ring.PostgreSQL.Extensions;

internal static class ByteExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static BackendMessageCode ToBackendMessageCode(this byte code)
	{
		// Code size: 273 (0x111)
		switch (code)
		{
			case (byte)'R': return BackendMessageCode.AuthenticationRequest;
			case (byte)'K': return BackendMessageCode.BackendKeyData;
			case (byte)'2': return BackendMessageCode.BindComplete;
			case (byte)'3': return BackendMessageCode.CloseComplete;
			case (byte)'C':	return BackendMessageCode.CommandComplete;
			case (byte)'d':	return BackendMessageCode.CopyData;
			case (byte)'c':	return BackendMessageCode.CopyDone;
			case (byte)'W':	return BackendMessageCode.CopyBothResponse;
			case (byte)'G':	return BackendMessageCode.CopyInResponse;
			case (byte)'H':	return BackendMessageCode.CopyOutResponse;
			case (byte)'D':	return BackendMessageCode.DataRow;
			case (byte)'I':	return BackendMessageCode.EmptyQueryResponse;
			case (byte)'E':	return BackendMessageCode.ErrorResponse;
			case (byte)'F':	return BackendMessageCode.FunctionCall;
			case (byte)'V':	return BackendMessageCode.FunctionCallResponse;
			case (byte)'n':	return BackendMessageCode.NoData;
			case (byte)'N':	return BackendMessageCode.NoticeResponse;
			case (byte)'A':	return BackendMessageCode.NotificationResponse;
			case (byte)'t': return BackendMessageCode.ParameterDescription;
			case (byte)'S':	return BackendMessageCode.ParameterStatus;
			case (byte)'1':	return BackendMessageCode.ParseComplete;
			case (byte)' ':	return BackendMessageCode.PasswordPacket;
			case (byte)'s':	return BackendMessageCode.PortalSuspended;
			case (byte)'Z':	return BackendMessageCode.ReadyForQuery;
			case (byte)'T': return BackendMessageCode.RowDescription;
			default: return BackendMessageCode.Unknown;
		}
	}
	
}
