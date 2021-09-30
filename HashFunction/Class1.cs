using System.ComponentModel;

namespace ConsoleApplication8
{
	[DefaultValue(NewEntity)]
	internal enum EntityComparison : byte
	{
		NewEntity = 1,
		NewTable = 2,
		DeletedEntity = 6,
		DeletedTable = 7
	}
}
