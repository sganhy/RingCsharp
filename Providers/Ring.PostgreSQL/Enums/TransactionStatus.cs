namespace Ring.PostgreSQL.Enums;

internal enum TransactionStatus : byte
{
	Idle = (byte)'I',
	InTransactionBlock = (byte)'T',
	InFailedTransactionBlock = (byte)'E',
	Pending = byte.MaxValue,
}
