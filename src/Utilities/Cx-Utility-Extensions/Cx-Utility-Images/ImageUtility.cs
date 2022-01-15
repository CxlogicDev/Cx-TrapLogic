﻿
namespace CxUtility.Images;

using static ImageUtility;

/// <summary>
/// The Image type that processing covers
/// </summary>
public enum ImageFormats { jpg, jpeg, png, gif }


public record ImageFormat
{
    public ImageFormats Format { get; }
    public int value => (int)Format;
    public string name => Format switch
    {
        ImageFormats.jpg => nameof(ImageFormats.jpg),
        ImageFormats.jpeg => nameof(ImageFormats.jpeg),
        ImageFormats.png => nameof(ImageFormats.png),
        ImageFormats.gif => nameof(ImageFormats.gif),
        _ => throw new InvalidOperationException()
    };

    private ImageFormat(ImageFormats _format)
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
    public enum AccptedImageFormats { jpg, jpeg, png, gif }//, bmp, tga

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

        var response = await httpClient.GetByteArrayAsync(ImagePath); //Ex: ("api/products/1");

        ImageResult? Result = null;

        if (response?.Length > 0)
            Result = new ImageResult(ImagePath, response, true);

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

    //public static string imageTo64BaseString(string ImagePath)
    //{
    //    using (Image image = Image.FromFile(ImagePath))
    //    {
    //        using (MemoryStream m = new MemoryStream())
    //        {
    //            image.Save(m, image.RawFormat);
    //            byte[] imageBytes = m.ToArray();

    //            // Convert byte[] to Base64 String
    //            string base64String = Convert.ToBase64String(imageBytes);
    //            return base64String;
    //        }
    //    }
    //}



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
            ImageFormats.jpg | ImageFormats.jpeg => "image/jpeg",
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
    public static string Image_Base64Data_ContextTypes(string PathExt)
    {
        switch (PathExt.ErrorIfNull(new ArgumentNullException()))
        {
            case string when PathExt.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                             PathExt.EndsWith("jpeg", StringComparison.OrdinalIgnoreCase):
                return "data:image/jpeg;base64, ";
            case string when PathExt.EndsWith("png", StringComparison.OrdinalIgnoreCase): return "data:image/png;base64, ";
            case string when PathExt.EndsWith("gif", StringComparison.OrdinalIgnoreCase): return "data:image/gif;base64, ";
            case string when PathExt.EndsWith("bmp", StringComparison.OrdinalIgnoreCase): return "data:image/bmp;base64, ";
            case string when PathExt.EndsWith("svg", StringComparison.OrdinalIgnoreCase): return "data:image/svg;base64, ";
            case string when PathExt.EndsWith("tiff", StringComparison.OrdinalIgnoreCase): return "data:image/tiff;base64, ";
            default:
                throw new InvalidOperationException();
        }
    }

    /// <summary>
    /// The Context Base64 Data Type from The Image format Types.
    /// </summary>
    /// <param name="_format"></param>
    /// <returns></returns>
    public static string Image_Base64Data_ContextType(this ImageFormats _format) =>
      _format switch
      {
          ImageFormats.jpg | ImageFormats.jpeg => "data:image/jpeg;base64, ",
          ImageFormats.png => "data:image/png;base64, ",
          ImageFormats.gif => "data:image/gif;base64, ",
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

    
    internal static string Image_ExtentionTypes(this ImageFormats _format) =>
        _format switch
        {
            ImageFormats.jpg => ".jpg",
            ImageFormats.jpeg => ".jpeg",
            ImageFormats.png => ".png",
            ImageFormats.gif => ".gif",
            _ => ""
        };

    /*
    /// <summary>
    /// Converts a Image Path to a Scaled down Version
    /// </summary>
    /// <param name="ImagePath">The Base Url Path to The Image:</param>
    /// <param name="maxWidth">The max With of the scaled Image</param>
    /// <param name="returnSrcString">The Base string that is retunrd</param>
    /// <returns></returns>
    public static async Task<string> ImageUrlScaledTo64BaseString(this HttpClient web, string ImagePath, int maxWidth = 250, bool returnSrcString = false)
    {
        string contents = null;

        //web.BaseAddress = new Uri(ImagePath); //Ex: ("http://localhost:9000/");
        web.DefaultRequestHeaders.Accept.Clear();

        using (var response = await web.GetStreamAsync(ImagePath)) //Ex: ("api/products/1");
        using (Image image = Image.FromStream(response))
        {
            Image thumb = image.ScaleImageByWidth(maxWidth);//image.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);

            using (MemoryStream m = new MemoryStream())
            {
                thumb.Save(m, image.RawFormat);

                byte[] imageBytes = m.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);

                contents = returnSrcString ? $"{ImageUtility.Acceptable_Image_ContextDataTypes(ImagePath)}{base64String}" : base64String;
            }
        }


        return contents;
    }
    //*/

}