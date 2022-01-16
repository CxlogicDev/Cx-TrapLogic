//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CxUtility.Images;


//public static class ImageResizeUtility
//{

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


//    //public static string imageTo64BaseString(string ImagePath)
//    //{
//    //    using (Image image = Image.FromFile(ImagePath))
//    //    {
//    //        using (MemoryStream m = new MemoryStream())
//    //        {
//    //            image.Save(m, image.RawFormat);
//    //            byte[] imageBytes = m.ToArray();

//    //            // Convert byte[] to Base64 String
//    //            string base64String = Convert.ToBase64String(imageBytes);
//    //            return base64String;
//    //        }
//    //    }
//    //}


//    /// <summary>
//    /// Will Take a Scaled down version of the orginal image 
//    /// </summary>
//    /// <param name="image">The Image to scale</param>
//    /// <param name="maxHeight">The Scaled Down Max Height</param>
//    public static Image ScaleImageByHeight(this Image image, int maxHeight)
//    {
//        var ratio = (double)maxHeight / image.Height;
//        var newWidth = (int)(image.Width * ratio);
//        var newHeight = (int)(image.Height * ratio);
//        var newImage = new Bitmap(newWidth, newHeight);
//        using (var g = Graphics.FromImage(newImage))
//        {
//            g.DrawImage(image, 0, 0, newWidth, newHeight);
//        }
//        return newImage;
//    }

//    /// <summary>
//    /// Will Take a scaled down version of the orginal image
//    /// </summary>
//    /// <param name="image">The Image to Scale Down.</param>
//    /// <param name="maxWidth">The Scaled Down Max Width</param>
//    public static Image ScaleImageByWidth(this Image image, int maxWidth)
//    {
//        var ratio = (double)maxWidth / (double)image.Width;
//        var newWidth = (int)(image.Width * ratio);
//        var newHeight = (int)(image.Height * ratio);
//        var newImage = new Bitmap(newWidth, newHeight);
//        using (var g = Graphics.FromImage(newImage))
//        {
//            g.DrawImage(image, 0, 0, newWidth, newHeight);
//        }
//        return newImage;
//    }

//    /// <summary>
//    /// Process down a image 
//    /// </summary>
//    /// <param name="image"></param>
//    /// <param name="size"></param>
//    /// <param name="preserveAspectRatio"></param>
//    /// <returns></returns>
//    public static Image ResizeImage(this Image image, Size size, bool preserveAspectRatio = true)
//    {
//        int newWidth;
//        int newHeight;
//        if (preserveAspectRatio)
//        {
//            int originalWidth = image.Width;
//            int originalHeight = image.Height;
//            float percentWidth = (float)size.Width / (float)originalWidth;
//            float percentHeight = (float)size.Height / (float)originalHeight;
//            float percent = percentHeight < percentWidth ? percentHeight : percentWidth;
//            newWidth = (int)(originalWidth * percent);
//            newHeight = (int)(originalHeight * percent);
//        }
//        else
//        {
//            newWidth = size.Width;
//            newHeight = size.Height;
//        }
//        Image newImage = new Bitmap(newWidth, newHeight);
//        using (Graphics graphicsHandle = Graphics.FromImage(newImage))
//        {
//            graphicsHandle.InterpolationMode = InterpolationMode.HighQualityBicubic;
//            graphicsHandle.DrawImage(image, 0, 0, newWidth, newHeight);
//        }
//        return newImage;
//    }

//    #region Hide Comment Out Tiff File Code Not Needed Currently
///*
//public static string[] ConvertTiffToJpeg(string fileName)
//{
//    using (Image imageFile = Image.FromFile(fileName))
//    {
//        FrameDimension frameDimensions = new FrameDimension(
//            imageFile.FrameDimensionsList[0]);

//        // Gets the number of pages from the tiff image (if multipage) 
//        int frameNum = imageFile.GetFrameCount(frameDimensions);
//        string[] jpegPaths = new string[frameNum];

//        for (int frame = 0; frame < frameNum; frame++)
//        {
//            // Selects one frame at a time and save as jpeg. 
//            imageFile.SelectActiveFrame(frameDimensions, frame);
//            using (Bitmap bmp = new Bitmap(imageFile))
//            {
//                jpegPaths[frame] = String.Format("{0}\\{1}{2}.jpg",
//                    Path.GetDirectoryName(fileName),
//                    Path.GetFileNameWithoutExtension(fileName),
//                    frame);
//                bmp.Save(jpegPaths[frame], ImageFormat.Jpeg);
//            }
//        }

//        return jpegPaths;
//    }
//}
////*/

///*
///// <summary> 
///// Converts jpeg imagae(s) to tiff image(s). 
///// </summary> 
///// <param name="fileNames"> 
///// String array having full name to jpeg images. 
///// </param> 
///// <param name="isMultipage"> 
///// true to create single multipage tiff file otherwise, false. 
///// </param> 
///// <returns> 
///// String array having full name to tiff images. 
///// </returns> 
//public static string[] ConvertJpegToTiff(string[] fileNames, bool isMultipage)
//{
//    EncoderParameters encoderParams = new EncoderParameters(1);
//    ImageCodecInfo tiffCodecInfo = ImageCodecInfo.GetImageEncoders()
//        .First(ie => ie.MimeType == "image/tiff");

//    string[] tiffPaths = null;
//    if (isMultipage)
//    {
//        tiffPaths = new string[1];
//        Image tiffImg = null;
//        try
//        {
//            for (int i = 0; i < fileNames.Length; i++)
//            {
//                if (i == 0)
//                {
//                    tiffPaths[i] = String.Format("{0}\\{1}.tif",
//                        Path.GetDirectoryName(fileNames[i]),
//                        Path.GetFileNameWithoutExtension(fileNames[i]));

//                    // Initialize the first frame of multipage tiff. 
//                    tiffImg = Image.FromFile(fileNames[i]);
//                    encoderParams.Param[0] = new EncoderParameter(
//                        Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
//                    tiffImg.Save(tiffPaths[i], tiffCodecInfo, encoderParams);
//                }
//                else
//                {
//                    // Add additional frames. 
//                    encoderParams.Param[0] = new EncoderParameter(
//                        Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
//                    using (Image frame = Image.FromFile(fileNames[i]))
//                    {
//                        tiffImg.SaveAdd(frame, encoderParams);
//                    }
//                }

//                if (i == fileNames.Length - 1)
//                {
//                    // When it is the last frame, flush the resources and closing. 
//                    encoderParams.Param[0] = new EncoderParameter(
//                        Encoder.SaveFlag, (long)EncoderValue.Flush);
//                    tiffImg.SaveAdd(encoderParams);
//                }
//            }
//        }
//        finally
//        {
//            if (tiffImg != null)
//            {
//                tiffImg.Dispose();
//                tiffImg = null;
//            }
//        }
//    }
//    else
//    {
//        tiffPaths = new string[fileNames.Length];

//        for (int i = 0; i < fileNames.Length; i++)
//        {
//            tiffPaths[i] = String.Format("{0}\\{1}.tif",
//                Path.GetDirectoryName(fileNames[i]),
//                Path.GetFileNameWithoutExtension(fileNames[i]));

//            // Save as individual tiff files. 
//            using (Image tiffImg = Image.FromFile(fileNames[i]))
//            {
//                tiffImg.Save(tiffPaths[i], ImageFormat.Tiff);
//            }
//        }
//    }

//    return tiffPaths;
//} 
////*/
//#endregion

//    /// <summary>
//    /// The Stream of the Image that need to be scaled down 
//    /// </summary>
//    /// <param name="ImageStream">The Image stream that being scaled</param>
//    /// <param name="fileName">The Image name with extention to be scaled</param>
//    /// <param name="maxWidth"></param>
//    /// <param name="returnSrcString"></param>
//    /// <returns></returns>
//    public static string ImageStreamScaledTo64BaseString(this Stream ImageStream, int maxWidth = 250, bool returnSrcString = false)
//    {
//        if (ImageStream == null)
//            throw new ArgumentNullException(nameof(ImageStream), "The ImageStream Cannot be null!!");


//        string contents = null;

//        using (Image image = Image.FromStream(ImageStream))
//        {
//            Image thumb = image.ScaleImageByWidth(maxWidth);//image.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);

//            using (MemoryStream m = new MemoryStream())
//            {
//                thumb.Save(m, image.RawFormat);

//                byte[] imageBytes = m.ToArray();

//                string base64String = Convert.ToBase64String(imageBytes);

//                contents = returnSrcString ? $"{thumb.RawFormat.Image_ContextDataType()}{base64String}" : base64String;
//            }
//        }

//        return contents;
//    }

//    /// <summary>
//    /// The byte Array of the Image that need to be scaled down 
//    /// </summary>
//    /// <param name="ImageStream"></param>
//    /// <param name="maxWidth"></param>
//    /// <returns></returns>
//    public static byte[] ImageStreamScaledToByteArray(this Stream ImageStream, int maxWidth = 250)
//    {
//        if (ImageStream == null)
//            throw new ArgumentNullException(nameof(ImageStream), "The ImageStream Cannot be null!!");

//        byte[] imageBytes = null;

//        using (Image image = Image.FromStream(ImageStream))
//        {
//            Image thumb = image.ScaleImageByWidth(maxWidth);//image.GetThumbnailImage(120, 120, () => false, IntPtr.Zero);

//            using (MemoryStream m = new MemoryStream())
//            {
//                thumb.Save(m, image.RawFormat);

//                imageBytes = m.ToArray();
//            }
//        }

//        return imageBytes;
//    }
//}