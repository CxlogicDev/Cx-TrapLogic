using CxUtility.Images;

namespace CxMessage.Email.SendGrid;

public record MessageAddress(string email, string? name = null)
{
    public bool isValidEmail => this.isValidEmail();

    public EmailAddress SendGridAddress => new EmailAddress(email, name);
}

public record SendMailMessage(SendGridAccess Access, string subject, MessageAddress FROM, MessageAddress TO, string message = "", string? html_message = null)
{
    public bool isValid => true;

    public List<string> error_Messages { get; } = new();
}

public record SendMessage
{
    public SendGridAccess Access { get; }

    public string subject { get; set; }

    public EmailAddress FROM { get; }
    //string message = "", string? html_message = null
    //public SendMessage(SendGridAccess _Access, string _subject, string From_Email, string? From_Email_Label = null, MessageAddress[]? to = null, MessageAddress[]? cc = null, MessageAddress[]? bcc = null)
    //{
    //    Access = _Access;
    //    subject = _subject;

    //    FROM = new EmailAddress(From_Email, From_Email_Label);

    //    if (to.isNotNull())
    //        foreach (var t in to)
    //            Add_TO_Address(t.ErrorIfNull_Or_NotValid(v => v.isValidEmail));

    //    if (cc.isNotNull())
    //        foreach (var t in cc)
    //            Add_CC_Address(t.ErrorIfNull_Or_NotValid(v => v.isValidEmail));

    //    if (bcc.isNotNull())
    //        foreach (var t in bcc)
    //            Add_BCC_Address(t.ErrorIfNull_Or_NotValid(v => v.isValidEmail));
    //}

    public SendMessage(SendGridAccess _Access, string _subject, EmailAddress From_Email, MessageAddress[]? to = null, MessageAddress[]? cc = null, MessageAddress[]? bcc = null)
    {
        Access = _Access;
        subject = _subject;

        FROM = From_Email;

        if (to.isNotNull())
            foreach (var t in to)
                Add_TO_Address(t.ErrorIfNull_Or_NotValid(v => v.isValidEmail));

        if (cc.isNotNull())
            foreach (var t in cc)
                Add_CC_Address(t.ErrorIfNull_Or_NotValid(v => v.isValidEmail));

        if (bcc.isNotNull())
            foreach (var t in bcc)
                Add_BCC_Address(t.ErrorIfNull_Or_NotValid(v => v.isValidEmail));
    }

    public List<EmailAddress> TO => _TO.Select(s => s.SendGridAddress).ToList();
    List<MessageAddress> _TO = new();

    public List<EmailAddress> CC => _CC.Select(s => s.SendGridAddress).ToList();
    List<MessageAddress> _CC = new();

    public List<EmailAddress> BCC => _BCC.Select(s => s.SendGridAddress).ToList();
    List<MessageAddress> _BCC = new();

    public string? message { get; init; } = string.Empty;
    public string? html_message { get; init; } = null;

    public bool isValid => true;
    public List<string> error_Messages { get; } = new();

    //Adds Address in the TO List
    public SendMessage Add_TO_Address(MessageAddress _address) { if (_address.isNotNull() && !_TO.Contains(_address)) _TO.Add(_address); return this; }

    //Adds Address in the CC List
    public SendMessage Add_CC_Address(MessageAddress _address) { if (_address.isNotNull() && !_CC.Contains(_address)) _CC.Add(_address); return this; }

    //Adds Address in the BCC List
    public SendMessage Add_BCC_Address(MessageAddress _address) { if (_address.isNotNull() && !_BCC.Contains(_address)) _BCC.Add(_address); return this; }

}

public record SendMessageWithImage : SendMessage
{
    public enum MailImagePullTypes { _ = 1, File = 2, Base64 = 4, HttpLink = 8, Data = 0x10 }

    //public SendMessageWithLogo(SendGridAccess _Access, string _subject, string From_Email, string? From_Email_Label = null, MessageAddress[]? to = null, MessageAddress[]? cc = null, MessageAddress[]? bcc = null) : 
    //    base(_Access, _subject, From_Email, From_Email_Label, to, cc, bcc) { }

    public SendMessageWithImage(SendGridAccess _Access, string _subject, EmailAddress From_Email, MessageAddress[]? to = null, MessageAddress[]? cc = null, MessageAddress[]? bcc = null) :
        base(_Access, _subject, From_Email, to, cc, bcc) { }
        
    public string? image_ContentId { get; set; }

    public string? image_ContentType { get; set; }

    public string? image_Data_base64 { get; internal set; }

    public byte[]? image_Data { get; internal set; }
    
    public string? image_Data_Path { get; internal set; }

    public bool hasImage => image_Data_base64.hasCharacters();

}

internal class SGAttachmentDispostion
{
    public static SGAttachmentDispostion inline = new SGAttachmentDispostion("inline");

    public static SGAttachmentDispostion attachment = new SGAttachmentDispostion("attachment");

    private SGAttachmentDispostion(string Value)
    {
        value = Value;
    }

    public string value { get; }
}