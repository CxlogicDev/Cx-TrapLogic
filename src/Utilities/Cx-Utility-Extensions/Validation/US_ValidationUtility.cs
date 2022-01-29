
namespace CxUtility.Validation.US;

public static class US_ValidationUtility
{
    /// <summary>
    /// This will remove any Phone number formatting for storage
    /// </summary>
    /// <param name="PhoneNumber">The Phone number input to clear the formatting.</param>
    /// <returns>The cleaned phone number</returns>
    public static string Clean_US_Phone(string PhoneNumber)
    {
        string phone = PhoneNumber.hasCharacters() ? new(PhoneNumber.Where(w => Char.IsDigit(w)).ToArray()) : string.Empty;
        return (phone.hasNoCharacters() || phone.Length < 10 || phone.Length > 11 || (phone.Length == 11 && phone[0] != '1')) ? string.Empty : phone;
    }

    /// <summary>
    /// Check to see if the Phone number is a valid US Number
    /// </summary>
    /// <param name="PhoneNumber">The phone number to check</param>
    /// <returns>The result of the check</returns>
    public static bool isValid_Format_US_Phone(string PhoneNumber) =>
        Clean_US_Phone(PhoneNumber).Length == 10 || (Clean_US_Phone(PhoneNumber).Length == 11 && Clean_US_Phone(PhoneNumber)[0] == '1');

    /// <summary>
    /// Will format a 7-10 digit number into US Formated number
    /// </summary>
    /// <param name="PhoneNumber">The number to format</param>
    /// <returns>The formatted number in the format {xxx-xxxx} or {(xxx) xxx-xxxx}</returns>
    public static string Format_US_Phone(string PhoneNumber)
    {
        try
        {
            var phone = Clean_US_Phone(PhoneNumber);

            if (phone.Length == 10)
            {
                //Real Phone Number
                var sb = $"({phone.Substring(0, 3)}) {phone.Substring(3, 3)}-{phone.Substring(6)}";
                return sb;
            }
            else if (phone.Length == 7)
            {
                //Real Phone Number
                var sb = $"{phone.Substring(0, 3)}-{phone.Substring(3)}";
                return sb;
            }

        }
        catch (Exception)
        {
        }

        return "";
    }
}

