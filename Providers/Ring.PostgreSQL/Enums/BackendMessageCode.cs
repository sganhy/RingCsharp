namespace Ring.PostgreSQL.Enums;

internal enum BackendMessageCode : byte
{
	/// <summary>
	///     Identifies the message as an authentication request ('R').
	/// </summary>
	AuthenticationRequest = (byte)'R',

	/// <summary>
	///     Identifies the message as cancellation key data ('K').
	/// </summary>
	BackendKeyData = (byte)'K',

	/// <summary>
	///     Identifies the message as a Bind Complete indicator ('2').
	/// </summary>
	BindComplete = (byte)'2',

	/// <summary>
	///     Identifies the message as a Close Complete indicator ('3').
	/// </summary>
	CloseComplete = (byte)'3',

	/// <summary>
	///     Identifies the message as a command completion status ('C').
	/// </summary>
	CommandComplete = (byte)'C',

	/// <summary>
	///     Identifies the message as COPY data ('d').
	/// </summary>
	CopyData = (byte)'d',

	/// <summary>
	///     Identifies the message as COPY completion indicator ('c').
	/// </summary>
	CopyDone = (byte)'c',

	/// <summary>
	///     Identifies the message as a CopyBoth response ('W').
	/// </summary>
	CopyBothResponse = (byte)'W',

	/// <summary>
	///     Identifies the message as a CopyIn response ('G').
	/// </summary>
	CopyInResponse = (byte)'G',

	/// <summary>
	///     Identifies the message as a CopyOut response ('H').
	/// </summary>
	CopyOutResponse = (byte)'H',

	/// <summary>
	///     Identifies the message as a data row ('D').
	/// </summary>
	DataRow = (byte)'D',

	/// <summary>
	///     Identifies the message as an empty query response ('I').
	/// </summary>
	EmptyQueryResponse = (byte)'I',

	/// <summary>
	///     Identifies the message as an error response ('E').
	/// </summary>
	ErrorResponse = (byte)'E',

	/// <summary>
	///     Identifies the message as a fastpath function call ('F').
	/// </summary>
	FunctionCall = (byte)'F',

	/// <summary>
	///     Identifies the message as a function call response ('V').
	/// </summary>
	FunctionCallResponse = (byte)'V',

	/// <summary>
	///     Identifies the message as a no-data indicator ('n').
	/// </summary>
	NoData = (byte)'n',

	/// <summary>
	///     Identifies the message as a notice/warning response ('N').
	/// </summary>
	NoticeResponse = (byte)'N',

	/// <summary>
	///     Identifies the message as an asynchronous notification ('A').
	/// </summary>
	NotificationResponse = (byte)'A',

	/// <summary>
	///     Identifies the message as parameter description data ('t').
	/// </summary>
	ParameterDescription = (byte)'t',

	/// <summary>
	///     Identifies the message as a runtime parameter status report ('S').
	/// </summary>
	ParameterStatus = (byte)'S',

	/// <summary>
	///     Identifies the message as a Parse Complete indicator ('1').
	/// </summary>
	ParseComplete = (byte)'1',

	/// <summary>
	///     Identifies the message as a password request packet (' ').
	/// </summary>
	PasswordPacket = (byte)' ',

	/// <summary>
	///     Identifies the message as a portal suspended indicator ('s').
	/// </summary>
	PortalSuspended = (byte)'s',

	/// <summary>
	///     Identifies the message as a Ready For Query backend status ('Z').
	/// </summary>
	ReadyForQuery = (byte)'Z',

	/// <summary>
	///     Identifies the message as a row description ('T').
	/// </summary>
	RowDescription = (byte)'T',
}