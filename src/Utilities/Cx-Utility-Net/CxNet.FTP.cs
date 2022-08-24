using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CxUtility.Net;

public record FTPAccess(string Url, string User, string Password)
{
    //, int port = 22, bool isCorrectedFTPLink

    /// <summary>
    /// The Typical Ftp url style link
    /// </summary>
    public string FTPUrl => Url.StartsWith("ftp://", StringComparison.CurrentCultureIgnoreCase) ? Url : $"ftp://{Url.TrimEnd('/')}";

    /// <summary>
    /// The Timeout of the call to the FTP
    /// </summary>
    public TimeSpan TimeOut { get; private set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// List Of Read Directories and files in them
    /// </summary>
    public Dictionary<string, List<string>> CurrentDirectoryRead { get; } = new Dictionary<string, List<string>>();

    /// <summary>
    /// List Of all Read Directories
    /// </summary>
    public IEnumerable<string> ReadDirctories => CurrentDirectoryRead.Keys;
}

public static class FTPUtilityExtentions 
{


    //WebClient Client { get; }
    /*

    public static async Task<FTPAccess> ReadDirectoryAsync(this HttpClient client, FTPAccess _ftpObj)
    {
        List<string> data = new List<string>();

        try
        {

            //client.
            //HttpWebRequest

            FtpWebRequest req = 
                //new HttpRequestMessage//(HttpMethod.Get)

            client.SendAsync()


            FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(_ftpObj.Url);
            

            ftpWebRequest.Credentials = new NetworkCredential(_user, _password);
            ftpWebRequest.Proxy = null;
            ftpWebRequest.Timeout = (int)TimeOut.TotalMilliseconds;
            ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectory;
            ftpWebRequest.KeepAlive = true;
            //ftpWebRequest.AuthenticationLevel = System.Net.Security.AuthenticationLevel.
            //ftpWebRequest.UsePassive = true;


            using (FtpWebResponse response = (FtpWebResponse)ftpWebRequest.GetResponse())//(await ftpWebRequest.GetResponseAsync());
            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream);

                while (!reader.EndOfStream) { data.Add(await reader.ReadLineAsync()); }
            }
        }
        catch (Exception Ex)
        {
            throw;
        }

        return data ?? new List<string>();
    }


    //*/
    /*
    public async Task<string> DownLoadFile(string Diretory_FilePath, string DestinationSourcePath, string fileName = null, bool deleteDestinationSource = false)
    {

        if (!Directory.Exists(DestinationSourcePath))
        {
            Directory.CreateDirectory(DestinationSourcePath);
        }


        string destination = fileName.hasCharacters() ?
            Path.Combine(DestinationSourcePath, fileName) :
            Path.Combine(DestinationSourcePath, Diretory_FilePath.Split('/').Last());

        if (File.Exists(destination))
        {
            if (deleteDestinationSource)
            {
                File.Delete(destination);
            }
            else
                throw new Exception($"A File was found At the Destination: {destination}. Please change the Destiantion or delete the source File");
        }

        //string destination = Path.Combine(DestinationSourcePath);

        try
        {
            using (Stream responseStream = await DownLoadDataStream(Diretory_FilePath))
            {
                using (FileStream fs = File.Create(destination))
                {
                    //responseStream.Seek(0, SeekOrigin.Begin);
                    await responseStream.CopyToAsync(fs);
                }
            }
        }
        catch (Exception Ex)
        {
            throw;
        }

        return destination;
    }

    public async Task<string> DownLoadFileContentString(string Diretory_FilePath)
    {

        string data = null;

        var CallPath = $"{FTPBaseUrl.TrimEnd('/')}/{Diretory_FilePath}";

        FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri(CallPath));
        ftpWebRequest.Credentials = new NetworkCredential(_user, _password);
        ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;

        ftpWebRequest.Timeout = (int)TimeOut.TotalMilliseconds;

        try
        {
            FtpWebResponse response = (FtpWebResponse)(await ftpWebRequest.GetResponseAsync());

            using (Stream responseStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(responseStream);

                data = await reader.ReadToEndAsync();
            }
        }
        catch (Exception Ex)
        {
            throw;
        }

        return data;
    }

    public async Task<byte[]> DownLoadDataContent(string Diretory_FilePath)
    {

        byte[] data = null;

        try
        {

            using (var responseStream = await DownLoadDataStream(Diretory_FilePath))
            {
                using (var s = new MemoryStream())
                {
                    responseStream.CopyTo(s);
                    data = s.ToArray();
                }
            }
        }
        catch (Exception Ex)
        {
            throw;
        }

        return data;
    }

    /// <summary>
    /// The stream need to be disposed of once done
    /// </summary>
    /// <param name="Diretory_FilePath"></param>
    /// <returns></returns>
    public async Task<Stream> DownLoadDataStream(string Diretory_FilePath)
    {
        byte[] data = null;

        var CallPath = $"{FTPBaseUrl.TrimEnd('/')}/{Diretory_FilePath}";

        FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(new Uri(CallPath));
        ftpWebRequest.Credentials = new NetworkCredential(_user, _password);
        ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;

        ftpWebRequest.Timeout = (int)TimeOut.TotalMilliseconds;

        FtpWebResponse response = (FtpWebResponse)(await ftpWebRequest.GetResponseAsync());


        return response.GetResponseStream();

    }
    //*/
}