using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Common.ExtentionMethode;
public static class DateTimeExtention
{
    public static int GetPersianWeekDayNumber2(this DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Saturday => 0,
            DayOfWeek.Sunday => 1,
            DayOfWeek.Monday => 2,
            DayOfWeek.Tuesday => 3,
            DayOfWeek.Wednesday => 4,
            DayOfWeek.Thursday => 5,
            DayOfWeek.Friday => 6,
            _ => throw new ArgumentOutOfRangeException(nameof(dayOfWeek), "روز وارد شده معتبر نیست."),
        };
    }
    public static string ToEnglishNumber(this string input)
    {
        if (input == null)
            return input;
        StringBuilder englishNumbers = new StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            if (Char.IsDigit(input[i]))
            {
                englishNumbers.Append(char.GetNumericValue(input, i));
            }
            else
            {
                englishNumbers.Append(input[i]);
            }
        }
        return englishNumbers.ToString();
    }
}
