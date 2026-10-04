namespace Ring.PostgreSQL.Enums;

internal enum FrontendMessageCode : byte
{
	/// <summary>
	///     Identifies the message as a Describe command ('D') to request metadata for a prepared statement or portal.
	/// </summary>
	Describe = (byte)'D',

	/// <summary>
	///     Identifies the message as a Sync command ('S') to signal the end of an extended-query protocol batch.
	/// </summary>
	Sync = (byte)'S',

	/// <summary>
	///     Identifies the message as an Execute command ('E') to execute a portal.
	/// </summary>
	Execute = (byte)'E',

	/// <summary>
	///     Identifies the message as a Parse command ('P') to create a prepared statement.
	/// </summary>
	Parse = (byte)'P',

	/// <summary>
	///     Identifies the message as a Bind command ('B') to create a portal from a prepared statement and parameters.
	/// </summary>
	Bind = (byte)'B',

	/// <summary>
	///     Identifies the message as a Close command ('C') to close a prepared statement or portal.
	/// </summary>
	Close = (byte)'C',

	/// <summary>
	///     Identifies the message as a Simple Query command ('Q').
	/// </summary>
	Query = (byte)'Q',

	/// <summary>
	///     Identifies the message as COPY data ('d').
	/// </summary>
	CopyData = (byte)'d',

	/// <summary>
	///     Identifies the message as a COPY completion indicator ('c').
	/// </summary>
	CopyDone = (byte)'c',

	/// <summary>
	///     Identifies the message as a COPY failure notification ('f').
	/// </summary>
	CopyFail = (byte)'f',

	/// <summary>
	///     Identifies the message as a Terminate connection command ('X').
	/// </summary>
	Terminate = (byte)'X',

	/// <summary>
	///     Identifies the message as a Password or SASL authentication response ('p').
	/// </summary>
	Password = (byte)'p'
}