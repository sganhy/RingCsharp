using System.Runtime.CompilerServices;

namespace Ring;

public static class Initialize
{
	[ModuleInitializer]
	internal static void Init()
	{
		// Forces Global to initialize BEFORE anything else
	}

	public static void Start(string connectionString)
    {
		RuntimeHelpers.RunClassConstructor(typeof(Global).TypeHandle);
		RuntimeHelpers.RunClassConstructor(typeof(Global).TypeHandle);
	}
}
