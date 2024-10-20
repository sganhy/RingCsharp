namespace Ring.Data;

//Warnings: 
// Enums should have zero value
// Mark enums with FlagsAttribute
// Enum Storage should be Int32
// Identifiers should not match keywords
#pragma warning disable CA1716, CA1008, CA1027, CA1028
public enum Operator : byte
#pragma warning restore CA1028, CA1027, CA1008, CA1716
{
    Equal = 1,
    NotEqual = 2,
    Greater = 3,
    GreaterOrEqual = 4,
    Less = 5,
    LessOrEqual = 6,
    Like = 7,
    NotLike = 8,
    In = 10
}