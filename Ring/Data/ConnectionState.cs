namespace Ring.Data;

public enum ConnectionState 
{
	Undefined = 0,
	Closed = 1,
	Open = 2,
	Connecting = 3,
	Executing = 4,
	Fetching = 5,
	Broken = 6
}