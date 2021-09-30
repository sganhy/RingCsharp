using Ring.Util.Core.Extensions;
using System;
using System.Text;

namespace Ring.Util
{
    internal sealed class Obfuscator
    {
        /// <summary>
	    /// Encode the given number into a Base36 string. Used to encode entity id
	    /// TODO: manage negative sequence & reverse performance
	    /// </summary>
	    /// <param name="sequence"></param>
	    /// <returns></returns>
	    public static string Encode(long sequence)
        {
            var sb = new StringBuilder(LongToBase(sequence));
            // sb <-- abs(sequence), radix(28)
            var maxRand = Constants.FillInCharsList.Length;
            for (var i = sb.Length >= Constants.MaxExternalkeySize ? 1 : Constants.MaxExternalkeySize - sb.Length; i > 0; --i)
            {
                var j = Constants.Rand.Next(maxRand);
                var newCharacter = Constants.FillInCharsList[j];
                j = sb.Length <= 1 ? 0 : Constants.Rand.Next(sb.Length + 1);               // j <-- new character position in sb
                // newCharacter
                sb.Insert(j, newCharacter);
            }
            //}
            // reverse the string
            return sb.Reverse().ToString();
        }

        /// <summary>
	    /// Decode the Base36 Encoded string into a number
	    /// </summary>
	    /// <param name="input"></param>
	    /// <returns></returns>
	    public static long Decode(string input)
        {
            var sb = new StringBuilder();
            // remove character useless char + reverse
            for (var i = input.Length - 1; i >= 0; --i)
                if (Constants.CharValues.ContainsKey(input[i])) sb.Append(input[i]);

            //return Long.parseLong(sb.toString(), Radix);
            return BaseToLong(sb.ToString());
        }

        /// <summary>
		/// Convert a long to string into [Constants.Radix] Base
		/// TODO: odd radix not working (test it)
		/// TODO: value greater than int32.MaxValue not working 
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
        private static string LongToBase(long value)
        {
            // Determine exact number of characters to use.
            char[] buffer;
            if (value == long.MaxValue) buffer = new char[Math.Max((int)Math.Ceiling(Math.Log(value - 1, Constants.Radix)), 1)];
            else buffer = new char[Math.Max((int)Math.Ceiling(Math.Log(value + 1, Constants.Radix)), 1)];
            var i = (long)buffer.Length;
            do
            {
                buffer[--i] = Constants.BaseChars[value % Constants.Radix];
                value = value / Constants.Radix;
            }
            while (value > 0);
            return new string(buffer);
        }

        /// <summary>
		/// Convert a string [Constants.Radix] Base into long 
		/// TODO: odd radix not working (test it)
		/// </summary>
		/// <param name="number"></param>
		/// <returns></returns>
        private static long BaseToLong(string number)
        {
            var chrs = number.ToCharArray();
            var m = chrs.Length - 1;
            var n = Constants.BaseChars.Length;
            var result = 0m;
            for (var i = 0; i < chrs.Length; i++, --m)
            {
                var x = Constants.CharValues[chrs[i]];
                result += x * (decimal)Math.Pow(n, m);
            }
            return (long)result;
        }

    }
}
