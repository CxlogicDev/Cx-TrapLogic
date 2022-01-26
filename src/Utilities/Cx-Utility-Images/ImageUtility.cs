
namespace CxUtility.Images;

/// <summary>
/// The Image type that processing covers
/// </summary>
public enum ImageFormats { unknown, jpg, jpeg, png, gif }

public record ImageFormat
{
    /// <summary>
    /// The Format Type
    /// </summary>
    public ImageFormats Format { get; }

    /// <summary>
    /// The Format value of the format
    /// </summary>
    public int value => (int)Format;
    
    /// <summary>
    /// The file extention of the Format type
    /// </summary>
    public string name => Format switch
    {
        ImageFormats.jpg => nameof(ImageFormats.jpg),
        ImageFormats.jpeg => nameof(ImageFormats.jpeg),
        ImageFormats.png => nameof(ImageFormats.png),
        ImageFormats.gif => nameof(ImageFormats.gif),
        _ => nameof(ImageFormats.unknown)
    };

    /// <summary>
    /// The Image Context Type
    /// </summary>
    public string Image_ContextType => Format.Image_ContextType();

    /// <summary>
    /// The Base64 Context Type 
    /// </summary>
    public string Image_Base64Data_ContextType(string? base64String = default) => base64String.hasNoCharacters()? Format.Image_Base64Data_ContextType() :
        $"{Format.Image_Base64Data_ContextType()}{base64String}";

    /// <summary>
    /// The Image file Extention type
    /// </summary>
    public string Image_ExtentionTypes => Format.Image_ExtentionTypes();

    internal ImageFormat(ImageFormats _format)
    {
        Format = _format;
    }

    public static ImageFormat jpg = new(ImageFormats.jpg);
    public static ImageFormat jpeg = new(ImageFormats.jpeg);
    public static ImageFormat png = new(ImageFormats.png);
    public static ImageFormat gif = new(ImageFormats.gif);


}


public static class ImageUtility
{
    /// <summary>
    /// The Image Types accepted for processing
    /// </summary>
    public enum AccptedImageFormats { jpg = 1, jpeg, png, gif }//, bmp, tga

    internal static string[] Acceptable_Image_Types => new[] { ".jpg", ".jpeg", ".png", ".gif" };

    /// <summary>
    /// Gets the Image Format of the image file
    /// </summary>
    /// <param name="filePath">The Url or Diretory Path to the file. </param>
    /// <exception cref="ArgumentException">The filePath is not a accepted File Type</exception>
    /// <exception cref="ArgumentNullException">The filePath is null.</exception>
    public static ImageFormats ImageFormat_From_FileExtention(string filePath)
    {
        if (filePath.ErrorIfNull(new ArgumentNullException(nameof(filePath))).StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return get_ext(filePath.ToLower().Split('/').Last());

        return get_ext(filePath.ToLower().Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Last());

        ImageFormats get_ext(string file)
        {
            switch (file.ErrorIfNull(new ArgumentNullException("The file type is not accpedt. Only { .jpg, .jpeg, .png, .gif } are allowed")))
            {
                case string when file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase):
                    return ImageFormats.jpg;

                case string when file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase):
                    return ImageFormats.jpeg;

                case string when file.EndsWith(".png", StringComparison.OrdinalIgnoreCase):
                    return ImageFormats.png;

                case string when file.EndsWith(".gif", StringComparison.OrdinalIgnoreCase):
                    return ImageFormats.gif;

                default:
                    throw new ArgumentException("The file type is not accpedt. Only {.jpg, .jpeg, .png, .gif} are allowed");
            }
        }
    }

    public static ImageFormats ImageFormat_From_UrlExtention(string UrlPath)
    {
        if (UrlPath.hasNoCharacters())
            return ImageFormats.unknown;

        var partPart = UrlPath.Split('?').First().Split('/').Last();

        if (!partPart.Contains("."))
            return ImageFormats.unknown;

        return partPart.ToLower().Split('.').Last() switch
        {
            "jpg" => ImageFormats.jpg,
            "jpeg" => ImageFormats.jpeg,
            "png" => ImageFormats.png,
            "gif" => ImageFormats.gif,
            _ => ImageFormats.unknown
        };
    }  
       
    public static ImageFormats ImageFormat_From_MediaType(string mediaType)
    {
        if (mediaType.hasNoCharacters())
            return ImageFormats.unknown;

        return mediaType.Trim().ToLower() switch
        {
            "image/jpg" => ImageFormats.jpg,
            "image/jpeg" => ImageFormats.jpg,
            "image/png" => ImageFormats.png,
            "image/gif" => ImageFormats.gif,
            _ => ImageFormats.unknown
        };
    }

    /// <summary>
    /// Downloads an Image from a url and convert to a image object.
    /// </summary>
    /// <param name="httpClient">The httpClient to use</param>
    /// <param name="ImagePath"></param>
    /// <param name="Include_DataSrcString"></param>
    /// <returns></returns>
    public static async Task<ImageResult?> ImageUrlResult(this HttpClient httpClient, string ImagePath)
    {
        _ = httpClient.ErrorIfNull(new ArgumentNullException(nameof(httpClient)));

        httpClient.DefaultRequestHeaders.Accept.Clear();

        using var responseMessage = await httpClient.GetAsync(ImagePath);

        var response = await responseMessage.Content.ReadAsByteArrayAsync();

        string MediaType = responseMessage.Content.Headers.ContentType?.MediaType ?? "";
        //if (ImageFormat_From_UrlExtention(_ImagePath) == ImageFormats.unknown)
        //{
        //    //Rename the file with a valid extention
           


        //}

        //ImagePath


        ImageResult? Result = null;

        if (response?.Length > 0)
            Result = new ImageResult(ImagePath, MediaType, response, true);

        return Result;
    }

    public static async Task<ImageResult?> ImageResults(this ImageItem ImageRequest, HttpClient? httpClient = default)
    {
        
        if (ImageRequest.ErrorIfNull(new ArgumentNullException(nameof(ImageRequest))).isUrl)
            return await httpClient
                    .ErrorIfNull(new ArgumentNullException(nameof(httpClient)))
                    .ImageUrlResult(ImageRequest.FilePath);


        var data =
            await File
                .ReadAllBytesAsync(ImageRequest.FilePath.ErrorIfNull_Or_NotValid(v => File.Exists(v), new FileNotFoundException("File was not found.", ImageRequest.FilePath)));

        return ImageResult.get_Result_PreserveByteData(ImageRequest, data);

        
        


    }

    public static bool isValidFormat(this ImageItem ImageRequest)
    {
        if(ImageRequest.isNull())
            return false;

        var PathExt = ImageRequest.FilePath;

        switch (PathExt.ErrorIfNull(new ArgumentNullException()))
        {
            case string when PathExt.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) || 
                             PathExt.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase):
            case string when PathExt.EndsWith("png", StringComparison.OrdinalIgnoreCase):
            case string when PathExt.EndsWith("gif", StringComparison.OrdinalIgnoreCase):
            case string when PathExt.EndsWith("bmp", StringComparison.OrdinalIgnoreCase):
            case string when PathExt.EndsWith("svg", StringComparison.OrdinalIgnoreCase):
            case string when PathExt.EndsWith("tiff", StringComparison.OrdinalIgnoreCase): return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// The context type based from the path extention.
    /// </summary>
    /// <param name="PathExt">The Image Path Extention</param>
    /// <exception cref="InvalidOperationException">The Extention does not match Invalid Operation Exceptions</exception>
    /// <exception cref="ArgumentNullException">The input argument is null</exception>
    public static string Image_ContextTypes(string PathExt)
    {
        switch (PathExt.ErrorIfNull(new ArgumentNullException()))
        {
            case string when PathExt.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                             PathExt.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase):
                return "image/jpeg";
            case string when PathExt.EndsWith("png", StringComparison.OrdinalIgnoreCase): return "image/png";
            case string when PathExt.EndsWith("gif", StringComparison.OrdinalIgnoreCase): return "image/gif";
            case string when PathExt.EndsWith("bmp", StringComparison.OrdinalIgnoreCase): return "image/bmp";
            case string when PathExt.EndsWith("svg", StringComparison.OrdinalIgnoreCase): return "image/svg";
            case string when PathExt.EndsWith("tiff", StringComparison.OrdinalIgnoreCase): return "image/tiff";
            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// The context Type Based from ImageFormat Types 
    /// </summary>
    /// <param name="_format">The format type</param>
    public static string Image_ContextType(this ImageFormats _format) =>
        _format switch
        {
            ImageFormats.jpg => "image/jpeg",
            ImageFormats.jpeg => "image/jpeg",
            ImageFormats.png => "image/png",
            ImageFormats.gif => "image/gif",            
            _ => ""
        };

    /// <summary>
    /// The Context Base64 Data Type for the Path Extention.
    /// </summary>
    /// <param name="PathExt">The Image Path Extention</param>
    /// <exception cref="InvalidOperationException">The Extention does not match Invalid Operation Exceptions</exception>
    /// <exception cref="ArgumentNullException">The input argument is null</exception>
    public static string Image_Base64Data_ContextTypes(string PathExt, string? base64String = null)
    {
        switch (PathExt.ErrorIfNull(new ArgumentNullException()))
        {
            case string when PathExt.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                             PathExt.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase):
                return $"data:image/jpeg;base64, {base64String??""}";
            case string when PathExt.EndsWith("png", StringComparison.OrdinalIgnoreCase): return $"data:image/png;base64, {base64String ?? ""}";
            case string when PathExt.EndsWith("gif", StringComparison.OrdinalIgnoreCase): return $"data:image/gif;base64, {base64String ?? ""}";
            case string when PathExt.EndsWith("bmp", StringComparison.OrdinalIgnoreCase): return $"data:image/bmp;base64, {base64String ?? ""}";
            case string when PathExt.EndsWith("svg", StringComparison.OrdinalIgnoreCase): return $"data:image/svg;base64, {base64String ?? ""}";
            case string when PathExt.EndsWith("tiff", StringComparison.OrdinalIgnoreCase): return $"data:image/tiff;base64, {base64String ?? ""}";
            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// The Context Base64 Data Type from The Image format Types.
    /// </summary>
    /// <param name="_format"></param>
    /// <returns></returns>
    public static string Image_Base64Data_ContextType(this ImageFormats _format, string? base64String = null) =>
      _format switch
      {
          ImageFormats.jpg => $"data:image/jpeg;base64, {base64String ?? ""}",
          ImageFormats.jpeg => $"data:image/jpeg;base64, {base64String ?? ""}",
          ImageFormats.png => $"data:image/png;base64, {base64String ?? ""}",
          ImageFormats.gif => $"data:image/gif;base64, {base64String ?? ""}",
          _ => ""
      };

    internal static string Image_ExtentionTypes(string PathExt)
    {
        switch (PathExt.ErrorIfNull(new ArgumentNullException()))
        {
            case string when PathExt.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                             PathExt.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase):
                return ".jpg";
            case string when PathExt.EndsWith("png", StringComparison.OrdinalIgnoreCase): return ".png";
            case string when PathExt.EndsWith("gif", StringComparison.OrdinalIgnoreCase): return ".gif";
            case string when PathExt.EndsWith("bmp", StringComparison.OrdinalIgnoreCase): return ".bmp";
            case string when PathExt.EndsWith("svg", StringComparison.OrdinalIgnoreCase): return ".svg";
            case string when PathExt.EndsWith("tiff", StringComparison.OrdinalIgnoreCase): return ".tiff";
            default:
                throw new InvalidOperationException();
        }
    }

    public static string Image_ExtentionTypes(this ImageFormats _format) =>
        _format switch
        {
            ImageFormats.jpg => ".jpg",
            ImageFormats.jpeg => ".jpeg",
            ImageFormats.png => ".png",
            ImageFormats.gif => ".gif",
            _ => ""
        };
}