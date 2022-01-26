using CxUtility.Cx_Console;
using CxUtility.Images;
using CxUtility.Images.Resizing;

namespace Cx_TrapTestConsole;

[CxConsoleInfo("image", typeof(Cx_Images_Test), CxRegisterTypes.Preview, "Live Inplace Testing Ground for Projects")]
internal class Cx_Images_Test : ConsoleBaseProcess
{

    HttpClient httpClient { get; }

    
    public Cx_Images_Test(CxCommandService CxProcess, IConfiguration config, HttpClient _httpClient) : base(CxProcess, config)
    {
        httpClient = _httpClient;
    }

    record CxImageResizeArgs(string saveDir, string imageUrl = "https://ashleyfurniture.scene7.com/is/image/AshleyFurniture/B138-31-36-46-87-84-86-91-SD?scl=1&printres=300")
    {
        internal const string CxImageName = "resize";
        internal const string CxImageDescription = "Pull Images and status in the system. ";
        internal const CxRegisterTypes CxImageType = CxRegisterTypes.Preview;

        internal const string arg_saveDir = "-saveDir";
        internal const string arg_saveDirName = "saveDir";
        internal const string arg_saveDirDescription = "";
        internal const CxConsoleActionArgAttribute.arg_Types arg_saveDirType = CxConsoleActionArgAttribute.arg_Types.Value;

        internal static CxImageResizeArgs get_args(CxCommandService cxserv)
        {
            _ = cxserv.getCommandArg(arg_saveDir, out string? saveDir);
            //_ = cxserv.getCommandArg($"-{IH_shipto}", out string? shipto);

            return new(saveDir.ErrorIfNull(new ArgumentNullException(arg_saveDirName)));            
        }
    }

    [CxConsoleAction(CxImageResizeArgs.CxImageName, CxImageResizeArgs.CxImageDescription, true, CxImageResizeArgs.CxImageType)]
    [CxConsoleActionArg(CxImageResizeArgs.arg_saveDirName, CxImageResizeArgs.arg_saveDirType, true, true, CxImageResizeArgs.CxImageDescription)]
    public async Task ImageResize(CancellationToken cancellationToken)
    {

        var args = CxImageResizeArgs.get_args(_CxCommandService);

        ImageResult? result = await httpClient.ImageUrlResult_ScaledByWidth(args.imageUrl);

        //IMG-{Guid.NewGuid()}.{result.ImageFormat.Image_ExtentionTypes().TrimStart('.')}
        string testFile = $"{result.FileName()}";
        await File.WriteAllBytesAsync(testFile, result.Data);



    }





}
