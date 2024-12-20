using Ring.Schema.Enums;
using System.Runtime.CompilerServices;

namespace Ring.Util.Extensions;

internal static class DateTimeExtensions
{
    private readonly static Dictionary<byte, char[]> DateTimeTemplates = new()
    {
      { (byte)FieldType.ShortDateTime, new char[] { '0','0','0','0','-','0','0','-','0','0' } },
      { (byte)FieldType.DateTime, new char[] { '0','0','0','0','-','0','0','-','0','0','T','0','0',':',
          '0','0',':','0','0','.','0','0','0','0','0','0','Z' } },
      { (byte)FieldType.LongDateTime, new char[] {'0','0','0','0','-','0','0','-','0','0','T','0','0',':',
          '0','0',':','0','0','.','0','0','0','0','0','0','+','0','0',':','0','0' } }
    };
    private const int DecimalSys = 10;


    internal static string ToString(this DateTime value, FieldType fieldType, TimeSpan? offset)
    {
        // IS0-8601 ==> "YYYY-MM-DDTHH:MM:SS.mmmmmZ" eg. 2005-12-12T18:17:16.015+04:00; lenght max ==> 30
        var template = DateTimeTemplates[(byte)fieldType];
        var count = template.Length;
        var result = new char[count];
        var dateToConv = fieldType == FieldType.DateTime || (offset==null && fieldType != FieldType.ShortDateTime) ?
            value.ToUniversalTime() : value;
        Array.Copy(template, result, count);
        SetDateTime(result, 4, dateToConv.Year, 3);
        SetDateTime(result, 2, dateToConv.Month, 6);
        SetDateTime(result, 2, dateToConv.Day, 9);
        if (fieldType != FieldType.ShortDateTime)
        {
            SetDateTime(result, 2, dateToConv.Hour, 12);
            SetDateTime(result, 2, dateToConv.Minute, 15);
            SetDateTime(result, 2, dateToConv.Second, 18);
            SetDateTime(result, 3, dateToConv.Millisecond, 22);
            SetDateTime(result, 3, dateToConv.Microsecond, 25);

            if (fieldType == FieldType.LongDateTime)
            {
                var hours = offset?.Hours ?? 0;
                if (hours < 0)
                {
                    result[26] = '-';
                    hours *= -1;
                }
                SetDateTime(result, 2, hours, 27);
                SetDateTime(result, 2, offset?.Minutes ?? 0, 30);
            }
        }
        return new string(result);
    }

    #region private methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetDateTime(Span<char> input, int size, int value, int lastPosition)
    {
        // Code size: 42 (0x2a)
        var i =0;
        while (i<size)
        {
            input[lastPosition--] += (char)(value%DecimalSys);
            value/=DecimalSys;
            ++i;
        }
    }

    #endregion 
}
