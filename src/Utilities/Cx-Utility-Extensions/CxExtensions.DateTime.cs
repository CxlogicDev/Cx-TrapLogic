namespace System;

public enum AbbreviatedDayOfWeek
{
    //
    // Summary:
    //     Indicates Sunday.
    Sun,
    //
    // Summary:
    //     Indicates Monday.
    Mon,
    //
    // Summary:
    //     Indicates Tuesday.
    Tues,
    //
    // Summary:
    //     Indicates Wednesday.
    Wed,
    //
    // Summary:
    //     Indicates Thursday.
    Thur,
    //
    // Summary:
    //     Indicates Friday.
    Fri,
    //
    // Summary:
    //     Indicates Saturday.
    Sat
}

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

    /// <summary>
    /// Get the Full Name of the week day
    /// </summary>
    public static string Name(this DayOfWeek dow) => dow switch
    {
        DayOfWeek.Sunday => nameof(DayOfWeek.Sunday),
        DayOfWeek.Monday => nameof(DayOfWeek.Monday),
        DayOfWeek.Tuesday => nameof(DayOfWeek.Tuesday),
        DayOfWeek.Wednesday => nameof(DayOfWeek.Wednesday),
        DayOfWeek.Thursday => nameof(DayOfWeek.Thursday),
        DayOfWeek.Friday => nameof(DayOfWeek.Friday),
        _ => nameof(DayOfWeek.Saturday)
    };

    /// <summary>
    /// Get the Full Name of the week day
    /// </summary>
    public static string Name(this AbbreviatedDayOfWeek dow) => dow switch
    {
        AbbreviatedDayOfWeek.Sun => nameof(DayOfWeek.Sunday),
        AbbreviatedDayOfWeek.Mon => nameof(DayOfWeek.Monday),
        AbbreviatedDayOfWeek.Tues => nameof(DayOfWeek.Tuesday),
        AbbreviatedDayOfWeek.Wed => nameof(DayOfWeek.Wednesday),
        AbbreviatedDayOfWeek.Thur => nameof(DayOfWeek.Thursday),
        AbbreviatedDayOfWeek.Fri => nameof(DayOfWeek.Friday),
        _ => nameof(DayOfWeek.Saturday)
    };

    /// <summary>
    /// Get the Abbreviation of the Week Day
    /// </summary>    
    public static string Abbreviation(this AbbreviatedDayOfWeek adow) => adow switch
    {
        AbbreviatedDayOfWeek.Sun => nameof(AbbreviatedDayOfWeek.Sun),
        AbbreviatedDayOfWeek.Mon => nameof(AbbreviatedDayOfWeek.Mon),
        AbbreviatedDayOfWeek.Tues => nameof(AbbreviatedDayOfWeek.Tues),
        AbbreviatedDayOfWeek.Wed => nameof(AbbreviatedDayOfWeek.Wed),
        AbbreviatedDayOfWeek.Thur => nameof(AbbreviatedDayOfWeek.Thur),
        AbbreviatedDayOfWeek.Fri => nameof(AbbreviatedDayOfWeek.Fri),
        _ => nameof(AbbreviatedDayOfWeek.Sat)
    };

    /// <summary>
    /// Get the Abbreviation of the Week Day
    /// </summary>
    public static string Abbreviation(this DayOfWeek adow) => adow switch
    {
        DayOfWeek.Sunday => nameof(AbbreviatedDayOfWeek.Sun),
        DayOfWeek.Monday => nameof(AbbreviatedDayOfWeek.Mon),
        DayOfWeek.Tuesday => nameof(AbbreviatedDayOfWeek.Tues),
        DayOfWeek.Wednesday => nameof(AbbreviatedDayOfWeek.Wed),
        DayOfWeek.Thursday => nameof(AbbreviatedDayOfWeek.Thur),
        DayOfWeek.Friday => nameof(AbbreviatedDayOfWeek.Fri),
        _ => nameof(AbbreviatedDayOfWeek.Sat)
    };

    /// <summary>
    /// cast to an AbbreviatedDayOfWeek
    /// </summary>    
    public static AbbreviatedDayOfWeek TO_Abbreviation(this DayOfWeek dow) => (AbbreviatedDayOfWeek)((int)dow);

    /// <summary>
    /// cast to a DayOfWeek
    /// </summary>   
    public static DayOfWeek TO_DayOfWeek(this AbbreviatedDayOfWeek dow) => (DayOfWeek)((int)dow);

}