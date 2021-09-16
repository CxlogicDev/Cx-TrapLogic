namespace System.Logic;
using static CxExtensions;

/// <summary>
/// Use for common re-used methods 
/// </summary>
public static class CxExtensions_BitLogic
{
    /// <summary>
    /// Changes A Collection of Active Bits to a integer value. Repeat Bit Valus are ignored
    /// </summary>
    /// <param name="ActiveBits">The Active Bits that will be converted into a integer value</param>        
    public static int IntFromActiveBits(this int[] ActiveBits) =>
        ActiveBits.Distinct().Sum(s => (int)Math.Pow(2, s - 1));

    /// <summary>
    /// Changes A collection of Active Bits to a long value. Repeat Bit Valus are ignored
    /// </summary>
    /// <param name="ActiveBits">The Active Bits that will be converted into a integer value</param>        
    public static long LongFromActiveBits(this int[] ActiveBits) =>
        ActiveBits.Distinct().Sum(s => (long)Math.Pow(2, s - 1));

    /// <summary>
    /// Will Get the Active Bits from a base Number.
    /// </summary>
    /// <param name="Value">The Base value to convert.</param>
    /// <param name="BaseValue">The Logic Base Type.</param>
    /// <returns>The Active Bits in the baseValue</returns>
    public static int[] ActiveBits(this string Value, LogicBaseValueTypes BaseValue) =>
        Value.Int64FromBaseString(BaseValue).ActiveBits();

    /// <summary>
    /// Will Get the Active Bits from a base 32 Number.
    /// </summary>
    /// <param name="Value">The Base value to convert.</param>
    /// <returns>The Active Bits in the baseValue</returns>
    public static int[] ActiveBits(this int Value)
    {
        var BinaryValue = Value.toBaseString(LogicBaseValueTypes.Binary);

        if (BinaryValue.Contains('1')) //We have a Bit array of values to return 
            return BinaryValue.Reverse().FindAllIndexof('1').Select(s => s + 1).ToArray();

        return new int[] { };
    }

    /// <summary>
    /// Will Get the Active Bits from a Base64 Number.
    /// </summary>
    /// <param name="Value">The Base value to convert. Default id Hex</param>
    /// <returns>The Active Bits in the baseValue</returns>
    public static int[] ActiveBits(this long Value)
    {
        var BinaryValue = Value.toBaseString(LogicBaseValueTypes.Binary);

        if (BinaryValue.Contains('1')) //We have a Bit array of values to return 
            return BinaryValue.Reverse().FindAllIndexof('1').Select(s => s + 1).ToArray(); ;

        return new int[] { };
    }

    /// <summary>
    /// Gets the Base type supplied for the active bitArray
    /// </summary>
    /// <param name="ActiveBits">Active Bit values</param>
    /// <param name="baseValue">The The base supplied.</param>
    public static string BaseTypeFromActiveBits(this int[] ActiveBits, LogicBaseValueTypes baseValue) =>
            ActiveBits?.LongFromActiveBits().toBaseString(baseValue) ?? "0";

}
