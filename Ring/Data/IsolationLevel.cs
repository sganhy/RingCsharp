namespace Ring.Data;

#pragma warning disable CA1028, CA1008 // Enum Storage should be Int32, Enums should have zero value
public enum IsolationLevel : byte
{
	ReadCommitted = 1,
	ReadUncommitted = 2,
	RepeatableRead = 3, 
	Serializable = 4
}
#pragma warning restore CA1008, CA1028
