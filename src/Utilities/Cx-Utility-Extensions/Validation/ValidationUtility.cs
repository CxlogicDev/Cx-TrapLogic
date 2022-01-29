namespace CxUtility.Validation;

public static class ValidationUtility
{
    /// <summary>
    /// Email Address Format validateder
    /// </summary>
    /// <param name="email">Email Address to validate</param>
    public static bool IsValidEmailAddressFormat(string EmailAddress)
    {
        try
        {
            System.Net.Mail.MailAddress addr = new System.Net.Mail.MailAddress(EmailAddress);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Test to see if IPv4 Address is valid
    /// </summary>
    /// <param name="IPv4"></param>
    public static bool IsValidIPv4Format(string IPv4)
    {
        if (IPv4.hasCharacters())
        {
            var IP4_Address = IPv4.Split('.')
               .Select(s => byte.TryParse(s, out byte _))
               .ToArray();
            ///Validate the IP ADDRESS
            return IP4_Address.Length == 4 && IP4_Address.All(a => a);
        }

        return false;
    }
}

