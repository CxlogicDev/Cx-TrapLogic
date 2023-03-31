using System.Text;
using System.Collections.Immutable;

namespace System;

public enum LogicBaseValueTypes
{
    Binary = 2,
    Integer = 10,
    Hex = 16
}

/// <summary>
/// The Intended use is for simple exchanges between Binary, Integer, and Hex.
/// this is not Optimized or complex situations 
/// </summary>
public static partial class CxExtensions //_LogicNumbers
{
    public const string BinaryBaseValues = "01";

    public const string IntegerBaseValues = "0123456789";    
    
    public const string HexBaseValues = "0123456789ABCDEF";

    // Only has Used internaly. ToDO: Check if is most Optimized 
    internal static ImmutableArray<char> allowed_BinaryBaseValues = ImmutableArray.Create(BinaryBaseValues.ToArray());
    internal static ImmutableArray<char> allowed_IntegerBaseValues = ImmutableArray.Create(IntegerBaseValues.ToArray());
    internal static ImmutableArray<char> allowed_HexBaseValues = ImmutableArray.Create(new[] { 'a','b','c','d','e','f','0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F' });
    
    /// <summary>
    /// The Chars allowed by the Logic base type
    /// </summary>
    /// <param name="BaseVal">BaseType to use</param>
    /// <returns>The Chars allowed.</returns>
    public static string LogicBaseValue(this LogicBaseValueTypes BaseVal) => BaseVal switch
    {
        LogicBaseValueTypes.Binary => BinaryBaseValues,
        LogicBaseValueTypes.Hex => HexBaseValues,
        LogicBaseValueTypes.Integer => IntegerBaseValues,
        _ => "",
    };

    /// <summary>
    /// Creates an unchangeable array
    /// </summary>
    /// <param name="BaseVal">The Enum of the base value </param>
    internal static ImmutableArray<char> Allowed_LogicBaseValue(this LogicBaseValueTypes BaseVal) => BaseVal switch
    {
        LogicBaseValueTypes.Binary => allowed_BinaryBaseValues,
        LogicBaseValueTypes.Integer => allowed_IntegerBaseValues,
        LogicBaseValueTypes.Hex => allowed_HexBaseValues,
        _ => throw new ArgumentOutOfRangeException("Not a valid Value"),
    };

    /// <summary>
    /// Check to see if the value is a value of type {BaseValueTypes} coverts all char collections including strings
    /// </summary>
    /// <param name="value">The Value to test</param>
    /// <param name="BaseVal">The value to test for</param>
    /// <returns>If the value is a number of BaseValueType.</returns>
    public static bool IsBaseNumber(this IEnumerable<char> values, LogicBaseValueTypes BaseVal) => 
        values?.ToArray().isBaseNumber(BaseVal) ?? false;

    /// <summary>
    /// Check to see if the value is a value of type {BaseValueTypes}
    /// </summary>
    /// <param name="value">The Value to test</param>
    /// <param name="BaseVal">The value to test for</param>
    /// <returns>If the value is in LogicBaseValueType.</returns>
    public static bool isBaseNumber(this char[] values, LogicBaseValueTypes BaseVal)
    {

        if (values is null || values.Length == 0)
            return false;

        int StartIdx = values.Length > 2 ? BaseVal switch
        {
            LogicBaseValueTypes.Binary => (new[] { 'b', 'B' }).Contains(values[1]) ? 2 : 0,
            LogicBaseValueTypes.Hex => (new[] { 'x', 'X' }).Contains(values[1]) ? 2 : 0,
            _ => 0
        } : 0;

        var charChecks = BaseVal.Allowed_LogicBaseValue();

        for (int i = StartIdx; i < values.Length; i++)
            if (!charChecks.Contains(values[i]))//Return false if not valid. 
                return false;

        return true;
    }

    /// <summary>
    /// Check to see if the value is a value of type {BaseValueTypes}
    /// </summary>
    /// <param name="value">The Value to test</param>
    /// <param name="BaseVal">The value to test for</param>
    /// <returns>If the value is a number of BaseValueType.</returns>
    public static bool isBaseNumber(this char value, LogicBaseValueTypes BaseVal) =>
        (new[] { value }).IsBaseNumber(BaseVal);//use the Method alredy Built

    /// <summary>
    /// Check to see if the value is a number.
    /// </summary>
    /// <param name="value">The value to test</param>
    /// <returns>If the value is a number</returns>
    public static bool isIntegerNumber(this char value) => 
        value.isBaseNumber(LogicBaseValueTypes.Integer);

    /// <summary>
    /// Check to see if the value is a number.
    /// </summary>
    /// <param name="value">The value to test</param>
    /// <returns>If the value is a number</returns>
    public static bool isInteger(this string value) => value.IsBaseNumber(LogicBaseValueTypes.Integer);

    /// <summary>
    /// Changes a string to a number 
    /// </summary>
    /// <param name="str">String value to change</param>
    /// <returns>a number </returns>
    public static int? toInt32(this string str) => int.TryParse(str, out int num) ? num : str.Int32FromLogicBaseTypeString(LogicBaseValueTypes.Hex);

    /// <summary>
    /// Changes a string to a long number
    /// </summary>
    /// <param name="str">The string value to change to a number</param>
    /// <returns>The number else a null becaues it was  not a number</returns>
    public static long? toInt64(this string str) => long.TryParse(str, out long num)? num : str.Int64FromBaseString(LogicBaseValueTypes.Hex);

    /// <summary>
    /// Changes a Int32 into a string value of BaseValueType
    /// </summary>
    /// <param name="value">The value tyo convert</param>
    /// <param name="BaseValueType">The type of Value to convert to</param>
    /// <param name="digits">The total number of digits that must be in. </param>
    public static string toBaseString(this int value, LogicBaseValueTypes BaseValueType, uint digits = 1)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(BaseValueType switch
        {
            LogicBaseValueTypes.Binary => Convert.ToString(value, 2),
            LogicBaseValueTypes.Integer => value.ToString(),
            LogicBaseValueTypes.Hex => value.ToString($"X{digits}"),
            _ => ""
        });

        if (BaseValueType != LogicBaseValueTypes.Integer && digits > 1)
            sb.Insert(0, (new string('0', digits > sb.Length ? (int)digits - sb.Length : 0)));

        return sb.ToString();
    }

    /// <summary>
    /// Changes a Int64 into a string value of BaseValueType
    /// </summary>
    /// <param name="value">The value tyo convert</param>
    /// <param name="BaseValueType">The type of Value to convert to</param>
    /// <param name="digits">The total number of digits that must be in. </param>
    public static string toBaseString(this long value, LogicBaseValueTypes BaseValueType, uint digits = 1)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(BaseValueType switch
        {
            LogicBaseValueTypes.Binary => Convert.ToString(value, 2),
            LogicBaseValueTypes.Integer => value.ToString(),
            LogicBaseValueTypes.Hex => value.ToString($"X{digits}"),
            _ => ""
        });

        if (BaseValueType != LogicBaseValueTypes.Integer && digits > 1)
            sb.Insert(0, (new string('0', digits > sb.Length ? (int)digits - sb.Length : 0)));

        return sb.ToString();
    }
    
    /// <summary>
    /// Change the base value to an int32
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <param name="BaseValueType">The Value base Type</param>
    public static int Int32FromLogicBaseTypeString(this string value, LogicBaseValueTypes BaseValueType) => value.IsBaseNumber(BaseValueType) ? BaseValueType switch
    {
        LogicBaseValueTypes.Binary => Convert.ToInt32(value, 2),
        LogicBaseValueTypes.Integer => int.TryParse(value, out int num) ? num : throw new ArgumentException($"The Value: {value ?? ""} Cound Not be converted to a Integer Value"),
        LogicBaseValueTypes.Hex => int.TryParse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.CurrentInfo, out var r) ? r :
                throw new ArgumentException($"The the value: {value} Could not be converted to BaseValueType: {BaseValueType}. "),
        _ => throw new ArgumentException($"The BaseValueType: {BaseValueType} has not been Built Out. ")
    } : (value is null? throw new ArgumentNullException($"Cannot convert a null value to a Int") : 
    throw new ArgumentException($"The Value: {value} could not be converted to base: {BaseValueType}"));

    /// <summary>
    /// Change the base value to an int64
    /// </summary>
    /// <param name="value">The value to convert</param>
    /// <param name="BaseValueType">The Value base Type</param>
    public static long Int64FromBaseString(this string value, LogicBaseValueTypes BaseValueType) => value.IsBaseNumber(BaseValueType) ? BaseValueType switch
    {
        LogicBaseValueTypes.Binary => Convert.ToInt64(value, 2),
        LogicBaseValueTypes.Integer => long.TryParse(value, out long num) ? num : throw new ArgumentException($"The Value: {value ?? ""} Cound Not be converted to a Integer Value"),
        LogicBaseValueTypes.Hex => long.TryParse(value, System.Globalization.NumberStyles.HexNumber, System.Globalization.NumberFormatInfo.CurrentInfo, out var r) ? r :
                throw new ArgumentException($"The the value: {value} Could not be converted to BaseValueType: {BaseValueType}. "),
        _ => throw new ArgumentException($"The BaseValueType: {BaseValueType} has not been Built Out. ")
    } : (value is null ? throw new ArgumentNullException($"Cannot convert a null value to a Int") :
    throw new ArgumentException($"The Value: {value} could not be converted to base: {BaseValueType}"));

    /// <summary>
    /// Rounds a decimal number to the nerest penny
    /// </summary>
    /// <param name="amount">The Amount to round</param>
    public static decimal Round2Penny(this decimal amount) => decimal.Ceiling(amount * 100.0m) / 100.0m;

    /// <summary>
    /// Will convert the decimal format value to a string value in format. Ex: 1123,12 = "1123.12".
    /// Note: This only deals with a decimal value or 2 digits
    /// </summary>
    /// <param name="value">The value to be converted</param>
    /// <returns>The decimal string</returns>
    public static string DecimalToString(this decimal value) =>
        value.ToString("F", Globalization.CultureInfo.CreateSpecificCulture("en-CA"));

}
