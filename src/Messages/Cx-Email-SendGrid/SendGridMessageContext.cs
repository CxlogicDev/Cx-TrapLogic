using CxUtility.Images;
using static CxMessage.Email.SendGrid.SendMessageWithImage;

namespace CxMessage.Email.SendGrid;

public static class SendGridMessageContext
{

    public static SendMessage get_SendMessage_Object(this SendGridAccess access, string to, string from, string subject, string? message = null, string? html_message = null) =>
        new(access, subject, new(from), new[] { new MessageAddress(to) }) {            
            message =  message,
            html_message = html_message
        };

    public static Task<Response> Send_QuickMessage(this SendMessage message, CancellationToken cancellationToken = default)
    {
        //var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        var client = new SendGridClient(message.Access.apiKey);
        var msg = MailHelper.CreateSingleEmail(message.FROM, message.TO.FirstOrDefault(), message.subject, message.message, message.html_message);

        return client.SendEmailAsync(msg, cancellationToken);//.ConfigureAwait(false);
    }

    public static SendMessageWithImage get_ImageMessage_Object(this SendGridAccess access, EmailAddress from, string subject, string? message = null, string? html_message = null) =>
        new SendMessageWithImage(access, subject, from)
        {
            message = message,
            html_message = html_message
        };

    /// <summary>
    /// Set the Image data to be attached
    /// </summary>
    /// <param name="imageMessage"></param>
    /// <param name="data_Object">The Image data object</param>
    /// <param name="dataType">The type supplied </param>
    /// <param name="httpClient">The HttpClient used to pull A url image. </param>
    /// <param name="ContentId">The contentId if different from default: Image</param>
    /// <param name="image_ext">Needed if using base64 or Data types. </param>
    /// <exception cref="InvalidOperationException">The data object is invalid</exception>
    /// <exception cref="InvalidCastException">The data type does not match the pull cast type</exception>
    public static async Task<SendMessageWithImage> set_image_Data(this SendMessageWithImage imageMessage, object data_Object, MailImagePullTypes dataType, string ContentId, HttpClient? httpClient = default, string? image_ext = null)
    {
        switch (dataType)
        {
            case MailImagePullTypes.File:
                imageMessage.image_Data_Path = (string)data_Object
                    .ErrorIfNull_Or_NotValid(f => f.GetType() == typeof(string), new InvalidCastException("The object type should be a string"));

                _ = imageMessage.image_Data_Path.Error_IfNotValid(f => File.Exists(f), new FileNotFoundException("No image file found."));

                if (File.Exists(imageMessage.image_Data_Path))
                {
                    imageMessage.image_Data = File.ReadAllBytes(imageMessage.image_Data_Path);

                    if (imageMessage.image_Data.Length > 0)
                        imageMessage.image_Data_base64 = Convert.ToBase64String(imageMessage.image_Data);

                    //get the content Type
                    imageMessage.image_ContentType = ImageUtility.Image_ContextTypes(imageMessage.image_Data_Path);

                    FileInfo fi = new FileInfo(imageMessage.image_Data_Path);
                    imageMessage.image_Data_Path = fi.Name;
                }

                break;

            case MailImagePullTypes.Base64:

                imageMessage.image_Data_base64 = (string)data_Object
                    .ErrorIfNull_Or_NotValid(f => f.GetType() == typeof(string), new InvalidCastException("The object type should be a string"));

                if (image_ext.hasNoCharacters())
                    throw new InvalidOperationException("The Content type has to be supplied for a base64 string");

                imageMessage.image_ContentType = ImageUtility.Image_ContextTypes(image_ext);

                imageMessage.image_Data_Path = $"{ContentId}.{image_ext.TrimStart('.')}";

                break;

            case MailImagePullTypes.Data:
                imageMessage.image_Data = (byte[])data_Object
                    .ErrorIfNull_Or_NotValid(f => f.GetType() == typeof(byte[]), new InvalidCastException("The object type should be a byte array"));

                _ = imageMessage.image_Data
                    .Error_IfNotValid(t => t.isNotNull() && t.Length > 0, new InvalidDataException("No data supplied for the image"));

                imageMessage.image_Data_base64 = Convert.ToBase64String(imageMessage.image_Data);

                if (image_ext.hasNoCharacters())
                    throw new InvalidOperationException("The Content type has to be supplied for Data");

                imageMessage.image_ContentType = ImageUtility.Image_ContextTypes(image_ext);

                imageMessage.image_Data_Path = $"{ContentId}.{image_ext.TrimStart('.')}";

                break;

            case MailImagePullTypes.HttpLink:

                imageMessage.image_Data_Path = (string)data_Object
                   .ErrorIfNull_Or_NotValid(f => f.GetType() == typeof(string), new InvalidCastException("The object type should be a string"));

                var img_result = await httpClient.ErrorIfNull(new ArgumentNullException(nameof(httpClient))).ImageUrlResult(imageMessage.image_Data_Path);

                imageMessage.image_Data_base64 = img_result?.base64String;

                //get the content Type
                imageMessage.image_ContentType = ImageUtility.Image_ContextTypes(imageMessage.image_Data_Path);

                imageMessage.image_Data_Path = imageMessage.image_Data_Path.Split('/').Last();

                break;
                            
            default:
                throw new InvalidCastException("not a valid datatype to convert the object too");
        }

        imageMessage.image_ContentId = ContentId;

        return imageMessage;
    }

    /// <summary>
    /// Process The Email with a Single Image attached
    /// </summary>
    /// <param name="emailMessage">The Message with a Image</param>
    /// <param name="cancellationToken">A canellation Token</param>
    public static Task<Response?> SendMailMessageAsync(this SendMessageWithImage emailMessage, CancellationToken cancellationToken = default)
    {
        if (!emailMessage.ErrorIfNull(new ArgumentNullException(nameof(emailMessage))).isValid)
            return Task.FromResult<Response?>(default);

        var client = new SendGridClient(emailMessage.Access.apiKey);

        var msg = new SendGridMessage()
        {
            From = emailMessage.FROM,
            Subject = emailMessage.subject,
            PlainTextContent = emailMessage.message,
            HtmlContent = emailMessage.html_message,
        };

        if (emailMessage.hasImage)
            msg.AddAttachment(new Attachment()
            {
                Content = emailMessage.image_Data_base64,
                Filename = emailMessage.image_Data_Path,
                Type = emailMessage.image_ContentType,
                ContentId = emailMessage.image_ContentId,
                Disposition = SGAttachmentDispostion.inline.value
            });

        //Attach the who to send mail to
        msg.AddTos(emailMessage.TO);//.attachMailTo(emailMessage.TO ?? new List<string>(), MailSendtoTypes.To);

        //Attach CC
        if (emailMessage.CC.Count > 0)
            msg.AddCcs(emailMessage.CC);//.attachMailTo(emailMessage.CC ?? new List<string>(), MailSendtoTypes.CC);

        //Attach BCC
        if (emailMessage.BCC.Count > 0)
            msg.AddBccs(emailMessage.BCC);//.attachMailTo(emailMessage.BCC ?? new List<string>(), MailSendtoTypes.BCC);

        //Need to add in the Email
        return client.SendEmailAsync(msg, cancellationToken);

    }

    /// <summary>
    /// Validate the Email
    /// </summary>
    /// <param name="message">The mail Message</param>
    internal static bool isValidEmail(this MessageAddress message)
    {
        try
        {
            _ = new System.Net.Mail.MailAddress(message.email);
        }
        catch
        {
            return false;
        }

        return true;
    }
}

