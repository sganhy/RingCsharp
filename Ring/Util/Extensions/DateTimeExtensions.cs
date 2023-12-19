using Ring.Schema.Enums;
using System.Runtime.CompilerServices;

namespace Ring.Util.Extensions;

internal static class DateTimeExtensions
{
    private readonly static Dictionary<byte, char[]> DateTimeTemplates = new()
    {
      { (byte)FieldType.ShortDateTime, new char[] {'0','0','0','0','-','0','0','-','0','0' } },
      { (byte)FieldType.DateTime, new char[] {'0','0','0','0','-','0','0','-','0','0','T','0','0',':','0','0',':','0','0','.','0','0','0','Z' } },
      { (byte)FieldType.LongDateTime, new char[] {'0','0','0','0','-','0','0','-','0','0','T','0','0',':',
          '0','0',':','0','0','.','0','0','0','0','0','0','0','+','0','0',':','0','0' } }
    };
    private readonly static int DecimalSys = 10;
    
    internal static string ToString(this DateTime value, FieldType fieldType, TimeSpan? offset)
    {
        // IS0-8601 ==> "YYYY-MM-DDTHH:MM:SS.mmmZ" eg. 2005-12-12T18:17:16.015+04:00; lenght max ==> 30
        var template = DateTimeTemplates[(byte)fieldType];
        var count = template.Length;
        var result = new char[count];
        var dateToConv = fieldType == FieldType.DateTime ? value.ToUniversalTime() : value;
        Array.Copy(template, result, count);
        SetDateTime(result, 4, dateToConv.Year, 3);
        SetDateTime(result, 2, dateToConv.Month, 6);
        SetDateTime(result, 2, dateToConv.Day, 9);
        if (fieldType != FieldType.ShortDateTime)
        {
            SetDateTime(result, 2, dateToConv.Hour, 12);
            SetDateTime(result, 2, dateToConv.Minute, 15);
            SetDateTime(result, 2, dateToConv.Second, 18);
            
            if (fieldType == FieldType.LongDateTime)
            {
                SetDateTime(result, 7, (int)(value.Ticks%10000000),30);
            }
            else SetDateTime(result, 3, dateToConv.Millisecond, 22);
            // manage offset
        }
        return new string(result);
    }

    #region private methods 

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetDateTime(char[] input, int size, int value, int lastPosition)
    {
        for (var i=0; i<size; ++i)
        {
            input[lastPosition--] += (char)(value % DecimalSys);
            value /= DecimalSys;
        }
    }

    #endregion 
}
