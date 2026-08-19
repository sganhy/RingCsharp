namespace Ring.Data;

[Flags]
public enum ConnectionState
{
	None = 0,
	Closed = 1,
	Open = 2,
	Connecting = 4,
	Executing = 8,
	Fetching = 16,
	Broken = 32
}