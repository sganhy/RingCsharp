namespace Ring.Schema.Enums;

internal enum DdlStatement : byte
{
    Create = 1,
    Drop = 2,
    Alter = 3,
    Truncate = 9,
    Undefined = 127
}
