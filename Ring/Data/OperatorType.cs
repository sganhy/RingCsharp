namespace Ring.Data;

#pragma warning disable CA1008 // Enums should have zero value
#pragma warning disable CA1027 // Mark enums with FlagsAttribute
public enum OperatorType
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
#pragma warning restore CA1027
#pragma warning restore CA1008
