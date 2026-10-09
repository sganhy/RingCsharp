using Ring.PostgreSQL.Enums;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ring.PostgreSQL.Extensions;

internal static class ByteExtensions
{
	// Store a pinned reference to the very first byte element at startup
	private static readonly BackendMessageCode[] Map = CreateMap();

	private static BackendMessageCode[] CreateMap()
	{
		var map = new BackendMessageCode[256];
		// Initializes all elements to 0 (BackendMessageCode.Unknown)
		Array.Fill(map, BackendMessageCode.Unknown);
		map['R'] = BackendMessageCode.AuthenticationRequest;
		map['K'] = BackendMessageCode.BackendKeyData;
		map['2'] = BackendMessageCode.BindComplete;
		map['3'] = BackendMessageCode.CloseComplete;
		map['C'] = BackendMessageCode.CommandComplete;
		map['d'] = BackendMessageCode.CopyData;
		map['c'] = BackendMessageCode.CopyDone;
		map['W'] = BackendMessageCode.CopyBothResponse;
		map['G'] = BackendMessageCode.CopyInResponse;
		map['H'] = BackendMessageCode.CopyOutResponse;
		map['D'] = BackendMessageCode.DataRow;
		map['I'] = BackendMessageCode.EmptyQueryResponse;
		map['E'] = BackendMessageCode.ErrorResponse;
		map['F'] = BackendMessageCode.FunctionCall;
		map['V'] = BackendMessageCode.FunctionCallResponse;
		map['n'] = BackendMessageCode.NoData;
		map['N'] = BackendMessageCode.NoticeResponse;
		map['A'] = BackendMessageCode.NotificationResponse;
		map['t'] = BackendMessageCode.ParameterDescription;
		map['S'] = BackendMessageCode.ParameterStatus;
		map['1'] = BackendMessageCode.ParseComplete;
		map[' '] = BackendMessageCode.PasswordPacket;
		map['s'] = BackendMessageCode.PortalSuspended;
		map['Z'] = BackendMessageCode.ReadyForQuery;
		map['T'] = BackendMessageCode.RowDescription;
		return map;
	}

	// Unsafe pointer arithmetic bypassing bounds check (0-255 guaranteed valid byte index)
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static BackendMessageCode ToBackendMessageCode(this byte code) => Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(Map), code); // Code size: 18 (0x12) - no virtual calls

}
