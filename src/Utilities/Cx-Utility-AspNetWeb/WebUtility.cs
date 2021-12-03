using Microsoft.AspNetCore.Http;
using System.Web;

namespace CxUtility.AspNetWeb;

/// <summary>
/// Utility extention for asp.net web app
/// </summary>
public static class WebUtility
{
    //public static string baseUrl(this HttpContext http) => http.Request.baseUrl();
    //public static string baseUrl(this HttpRequest req)
    //{
    //    var BaseUrlIdx = (req.Url.LocalPath.Length == 1 && req.Url.LocalPath == "/") ?
    //                HttpUtility.UrlDecode(req.Url.AbsoluteUri).LastIndexOf(req.Url.LocalPath) :
    //                HttpUtility.UrlDecode(req.Url.AbsoluteUri).IndexOf(req.Url.LocalPath);
    //    var baseUri = req.Url.AbsoluteUri.Substring(0, BaseUrlIdx).TrimEnd('/');
    //    //
    //    return $"{baseUri.TrimEnd('/')}/";
    //}

    /// <summary>
    /// The Browser Types 
    /// </summary>
    public enum BrowserTypes { Unknown, IEBrowser, ChromeBrowser, EdgeBrowser, FireFoxBrowser, SafariBrowser, OperaBrowser }

    /// <summary>
    /// The Devive Type
    /// </summary>
    public enum DeviceTypes { Unknown, Browser, WindowsPhone, iOS, Android, ChromeBrowser, EdgeBrowser, FireFoxBrowser, SafariBrowser, iPad, iPhone, iPod }

    /// <summary>
    /// The Device used
    /// </summary>
    public enum Device { Unknown, Browser, WindowsPhone, iOS, Android }

    /// <summary>
    /// Pulls User Agent String from a request object
    /// </summary>
    public static string? get_UserAgentString(this HttpContext http) => 
        http.Request.get_UserAgentString();

    /// <summary>
    /// Pulls User Agent String from a request object
    /// </summary>
    public static string? get_UserAgentString(this HttpRequest request) => 
        !request.Headers.TryGetValue("User-Agent", out var _UserAgent)? 
            default : _UserAgent.ToString();

    /// <summary>
    /// Pull Browser Type from the request
    /// </summary>
    public static BrowserTypes get_BrowserType(this HttpContext httpContext) =>
        httpContext.Request.get_BrowserType();

    /// <summary>
    /// Pull Browser Type from the request
    /// </summary>
    public static BrowserTypes get_BrowserType(this HttpRequest request)
    {
        if (!request.Headers.TryGetValue("User-Agent", out var val))
            return BrowserTypes.Unknown;

        string UserAgent = val.ToString();

        if ("edge/".Split(':').Any(a => UserAgent.ToLower().Replace(" ", "").Contains(a.ToLower())))
            return BrowserTypes.OperaBrowser;
        else if ("windows;windowsnt;MSIE".Split(';').Any(a => UserAgent.ToLower().Replace(" ", "").Contains(a.ToLower())) && !UserAgent.ToLower().Contains("chrome/"))
            return BrowserTypes.IEBrowser;
        else if ("android:chrome/".Split(':').Any(a => UserAgent.ToLower().Replace(" ", "").Contains(a.ToLower())))
            return BrowserTypes.ChromeBrowser;
        else if ("Macintosh;:iPad;:iPhone;:Safari".Split(':').Any(a => UserAgent.ToLower().Replace(" ", "").Contains(a.ToLower())))
            //Apple detect
            return BrowserTypes.SafariBrowser;
        else if ("Opera".Split(':').Any(a => UserAgent.ToLower().Replace(" ", "").Contains(a.ToLower())))
            return BrowserTypes.OperaBrowser;
        else if ("firefox/".Split(':').Any(a => UserAgent.ToLower().Replace(" ", "").Contains(a.ToLower())))
            return BrowserTypes.FireFoxBrowser;

        return BrowserTypes.Unknown;
    }

    /// <summary>
    /// Gets the Device type 
    /// </summary>
    public static DeviceTypes get_DeviceType(this HttpContext http) => 
        http.Request.get_DeviceType();

    /// <summary>
    /// Gets the Device type 
    /// </summary>
    public static DeviceTypes get_DeviceType(this HttpRequest request)
    {
        //http://stackoverflow.com/questions/21741841/detecting-ios-android-operating-system
        string? userAgent = request.get_UserAgentString();
        //var Platform = navigator.platform;

        if (userAgent.hasNoCharacters())
            return DeviceTypes.Unknown;

        // Windows Phone must come first because its UA also contains "Android"
        if ("windows:phone".Split(':').All(a => userAgent.ToLower().Contains(a)))
        {
            return DeviceTypes.WindowsPhone; // "Windows Phone";
        }

        if ("android:mobile".Split(':').All(a => userAgent.ToLower().Contains(a)))
        {
            return DeviceTypes.Android; // "Android";
        }

        // iOS detection from: http://stackoverflow.com/a/9039885/177710
        //http://stackoverflow.com/questions/19877924/what-is-the-list-of-possible-values-for-navigator-platform-as-of-today
        //http://stackoverflow.com/questions/9038625/detect-if-device-is-ios/9039885#9039885
        if ((new[] { "ipad", "iphone", "ipod" }).Any(a => userAgent.ToLower().Contains(a)) && userAgent.ToLower().Contains("mobile/") && !userAgent.ToLower().Contains("win32"))
        {

            if ("ipad".Any(a => userAgent.ToLower().Contains(a))) { return DeviceTypes.iPad; }
            else if ("iphone".Any(a => userAgent.ToLower().Contains(a))) { return DeviceTypes.iPhone; }
            else if ("ipod".Any(a => userAgent.ToLower().Contains(a))) { return DeviceTypes.iPod; }
            else { return DeviceTypes.iOS; }// "iOS";
        }

        //if (/Opera/i.test(userAgent)) { return Device  }

        return DeviceTypes.Browser;
    }
}
/*
 
ViewDataDictionary<TModel> ViewData
 
 */