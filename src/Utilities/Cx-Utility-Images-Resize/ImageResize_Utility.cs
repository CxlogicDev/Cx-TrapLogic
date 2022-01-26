
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Advanced;

namespace CxUtility.Images.Resizing;

public static class ImageResize_Utility
{

    //*
    /// <summary>
    /// Converts a Image Path to a Scaled down Version
    /// </summary>
    /// <param name="ImagePath">The Base Url Path to The Image:</param>
    /// <param name="maxWidth">The max With of the scaled Image</param>
    /// <param name="returnSrcString">The Base string that is retunrd</param>
    /// <returns></returns>
    public static async Task<ImageResult> ImageUrlResult_ScaledByWidth(this HttpClient web, string ImagePath, int maxWidth = 250)
    {

        IImageFormat format;

        ImageResult _UrlResult = await web.ImageUrlResult(ImagePath);

        using Image image = Image.Load(_UrlResult.Data, out format);

        image.Mutate(c => c.Resize(maxWidth, 0));

        using MemoryStream _Stream = new MemoryStream();

        //image.ToBase64String

        await image.SaveAsync(_Stream, format);

        ImageResult _Result = new ImageResult(_UrlResult.FilePath, _UrlResult.MediaType, _Stream.ToArray(), true);
            
            //ImageResult.get_Result_PreserveByteData(_UrlResult, _Stream.ToArray());//     _UrlResult new ImageResult()





        return _Result;
    }
    //*/

    /*

    /// <summary>
    /// Will Take a Scaled down version of the orginal image 
    /// </summary>
    /// <param name="image">The Image to scale</param>
    /// <param name="maxHeight">The Scaled Down Max Height</param>
    public static Image ScaleImageByHeight(this Image image, int maxHeight)
    {
        var ratio = (double)maxHeight / image.Height;
        var newWidth = (int)(image.Width * ratio);
        var newHeight = (int)(image.Height * ratio);
        var newImage = new Bitmap(newWidth, newHeight);
        using (var g = Graphics.FromImage(newImage))
        {
            g.DrawImage(image, 0, 0, newWidth, newHeight);
        }
        return newImage;
    }


    //*/


}
