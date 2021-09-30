using System.Text;

namespace Ring.Util.Core.Extensions
{
    internal static class StringBuilderExtension
    {
        internal static StringBuilder Reverse(this StringBuilder text)
        {
            if (!(text?.Length > 1)) return text;
            var pivotPos = text.Length;
            pivotPos >>= 1;
            for (var i = 0; i < pivotPos; i++)
            {
                var iRight = text.Length - (i + 1);
                var rightChar = text[i];
                var leftChar = text[iRight];
                text[i] = leftChar;
                text[iRight] = rightChar;
            }
            return text;
        }

    }
}
