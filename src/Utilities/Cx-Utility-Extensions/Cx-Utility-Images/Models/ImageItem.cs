
namespace CxUtility.Images;

using static ImageUtility;

/// <summary>
/// The Image Request.
/// </summary>
/// <param name="FilePath">The Path to the Image File</param>
public record ImageItem(string FilePath)
{
    public bool isUrl => FilePath.isNull() ? false : FilePath.StartsWith("http", StringComparison.OrdinalIgnoreCase);

    public bool isValidFormat => FilePath.isNull() ? false : this.isValidFormat();
}

/// <summary>
/// 
/// </summary>
public record ImageResult : ImageItem
{

    /// <summary>
    /// The Data for the Image File
    /// </summary>
    [System.Diagnostics.CodeAnalysis.AllowNull]
    public byte[] Data { get; }

    /// <summary>
    /// The Base64 String for the Data
    /// </summary>
    public string base64String { get; }

    /// <summary>
    /// The Base64 string for the Data Formatted. Ex: [data:image/png;base64, DB645.....]
    /// </summary>
    public string Base64String_Formated => $"{Image_Base64Data_ContextTypes(FilePath)}{base64String}";

    /// <summary>
    /// Format of the image
    /// </summary>
    public ImageFormats ImageFormat => ImageFormat_From_FileExtention(FilePath);

    /// <summary>
    /// Process the ImageResult record
    /// </summary>
    /// <param name="_filePath">The File Path to the image</param>
    /// <param name="_data">The image Byte Data</param>
    /// <param name="preservByteData">Tells the Result to preserve the data</param>
    public ImageResult(string _filePath, byte[] _data, bool preserveByteData = false) : base(_filePath)
    {
        _ = _filePath.ErrorIfNull_Or_NotValid(v => isUrl || File.Exists(v), new InvalidOperationException());

        Data = preservByteData ? _data : null;

        base64String = Convert.ToBase64String(_data);
    }

    /// <summary>
    /// Gets the result record without out Preserving the byte data
    /// </summary>
    /// <param name="_item">The Request Item with the path</param>
    /// <param name="_data">The Data to create the result from </param>
    public static ImageResult get_Result(ImageItem _item, byte[] _data) => new ImageResult(_item.FilePath, _data, false);

    /// <summary>
    /// Gets the result record and Preserves the byte data
    /// </summary>
    /// <param name="_item">The Request Item with the path</param>
    /// <param name="_data">The Data to create the result from </param>
    public static ImageResult get_Result_PreserveByteData(ImageItem _item, byte[] _data) => new ImageResult(_item.FilePath, _data, true);
}