namespace Ring.PostgreSQL.Enums;

internal enum FrontendMessageCode : byte
{
	Describe = (byte)'D',
	Sync = (byte)'S',
	Execute = (byte)'E',
	Parse = (byte)'P',
	Bind = (byte)'B',
	Close = (byte)'C',
	Query = (byte)'Q',		
	CopyData = (byte)'d',
	CopyDone = (byte)'c',
	CopyFail = (byte)'f',
	Terminate = (byte)'X',
	Password = (byte)'p'
}
