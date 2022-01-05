namespace System;

public static partial class CxExtensions
{
    /// <summary>
    /// Gets the current days past Jan 1 Ex: Jan 5, 2000 = Day 5 | Dec 31, 2000 = Day 366(note: Year 2000 is a Leap Year) 
    /// </summary>
    /// <param name="_date">The Date to test agaist</param>
    /// <returns>The date count of the day from Jan 1, [Year Supplied]</returns>
    public static int DayPastedInYear(this DateTime _date)
    {
        int days = 0;

        for (int i = 1; i <= (_date.Month - 1); i++)
            days += DateTime.DaysInMonth(_date.Year, i);

        days += _date.Day;
        return days;
    }

}