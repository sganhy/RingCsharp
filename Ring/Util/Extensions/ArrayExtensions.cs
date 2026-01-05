namespace Ring.Util.Extensions;

static internal class ArrayExtensions
{
	internal static bool ArraysEqual<T>(this T[] left, T[] right) where T : IEquatable<T>
	{
		// Code size: 81 (0x51)
		if (left.Length != right.Length) return false;
		for (int i = 0; i < left.Length; i++)
			if (!left[i].Equals(right[i])) return false;
		return true;
	}

}
