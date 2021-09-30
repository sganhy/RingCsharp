using System.Runtime.CompilerServices;
using System.Text;

namespace Ring.Util.Helpers
{
    public static class HashHelper
    {

        // Murmur3 Constants for 32 bit variant
        private const int C132 = -862048943;
        private const int C232 = 0x1b873593;
        private const int R132 = 15;
        private const int R232 = 13;
        private const int M32 = 5;
        private const int N32 = -430675100;
        private const char MinLowerChar = 'a';


        /// <summary>
        /// Hash code method: djb2 (sum version)
        /// </summary>
        /// <param name="input">cannot be null</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Djb2(string input)
        {
            var hash = 5381;
            for (var i = 0; i < input.Length; ++i) hash += (hash << 5) + input[i];
            return hash;
        }

        /// <summary>
        /// Hash code method: djb2 (xor version)
        /// </summary>
        /// <param name="input">cannot be null</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Djb2X(string input)
        {
            var hash = 5381;
            for (var i = 0; i < input.Length; ++i) hash ^= hash << 5 ^ input[i];
            return hash;
        }

        /// <summary>
        /// Hash code method: djb2 xor version & from string.ToLower()
        /// </summary>
        /// <param name="input">cannot be null</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int LDjb2X(string input)
        {
            var hash = 5381;
            for (var i = 0; i < input.Length; ++i) if (input[i] < MinLowerChar) hash ^= hash << 5 ^ (input[i] + 32);
                else hash ^= hash << 5 ^ input[i];
            return hash;
        }

        /// <summary>
        /// Hash method: java
        /// </summary>
        /// <param name="input">cannot be null</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Java(string input)
        {
            var hash = 0;
            for (var i = 0; i < input.Length; ++i) hash = 31 * hash + input[i];
            return hash;
        }

        /// <summary>
        /// sdbm little-endian
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Sdbm(string input)
        {
            var hash = 0;
            for (var i = 0; i < input.Length; ++i) hash = (byte)input[i] + (hash << 6) + (hash << 16) - hash;
            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int Murmur3(string input, int seed) => Murmur3(Encoding.ASCII.GetBytes(input), seed);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Murmur3(byte[] data, int seed)
        {
            var hash = seed;
            var nblocks = data.Length >> 2;

            // body
            for (var i = 0; i < nblocks; ++i)
            {
                var i4 = i << 2;
                var k = (data[i4] & 0xff) | ((data[i4 + 1] & 0xff) << 8) | ((data[i4 + 2] & 0xff) << 16) | ((data[i4 + 3] & 0xff) << 24);

                // mix functions
                k *= C132;
                k = RotateLeft(k, R132);
                k *= C232;
                hash ^= k;
                hash = RotateLeft(hash, R232) * M32 + N32;
            }

            // tail
            var idx = nblocks << 2;
            var k1 = 0;
            switch (data.Length - idx)
            {
                case 3:
                    k1 ^= data[idx + 2] << 16;
                    goto case 2;
                case 2:
                    k1 ^= data[idx + 1] << 8;
                    goto case 1;
                case 1:
                    k1 ^= data[idx];
                    // mix functions
                    k1 *= C132;
                    k1 = RotateLeft(k1, R132);
                    k1 *= C232;
                    hash ^= k1;
                    break;
            }

            // finalization
            hash ^= data.Length;
            hash ^= (int)((uint)hash >> 16);
            hash *= -2048144789;
            hash ^= (int)((uint)hash >> 13);
            hash *= -1028477387;
            hash ^= (int)((uint)hash >> 16);

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int HalfAvalanche(int input)
        {
            var result = (uint)input;
            result = result + 0x479ab41d + (result << 8);
            result = (result ^ 0xe4aa10ce) ^ (result >> 5);
            result = (result + 0x9942f0a6) - (result << 14);
            result = result ^ 0x5aedd67d ^ (result >> 3);
            result = result + 0x17bea992 + (result << 7);
            return (int)result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int FullAvalanche(int input)
        {
            var result = (uint)input;
            result = result + 0x7ed55d16 + (result << 12);
            result = result ^ 0xc761c23c ^ (result >> 19);
            result = result + 0x165667b1 + (result << 5);
            result = (result + 0xd3a2646c) ^ (result << 9);
            result = result + 0xfd7046c5 + (result << 3);
            result = result ^ 0xb55a4f09 ^ (result >> 16);
            return (int)result;
        }

        /// <summary>
        /// Function to left rotate n by d bits
        /// </summary>
        /// <param name="x"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int RotateLeft(int x, int distance) => (x << distance) | (int)((uint)x >> 32 - distance);

        /*
		/// <summary>
		/// Function to right rotate n by d bits
		/// </summary>
		/// <param name="i"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int RotateRight(int i, int distance) => (i >> distance) | (i << (32 - distance));

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static int ToUpper(char c) => c >= MinLowerChar ? c - 32 : c;

        */
    }
}
