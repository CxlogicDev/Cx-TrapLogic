using Microsoft.Extensions.Configuration;
using CxUtility.Cx_Console.DisplayMethods;
using System.Reflection;

namespace CxUtility.Cx_Console;

public abstract class ConsoleBaseProcess : IConsoleProcessService
{
    public CxCommandService _CxCommandService { get; }

    public IConfiguration _Config { get; }

    /// <summary>
    /// Auto init uses internal methods to write to Screen 
    /// </summary>
    public ICxLogService WriteOutput_Service { get; } 

    public ConsoleBaseProcess(CxCommandService CxProcess, IConfiguration config)
    {
        _CxCommandService = CxProcess;
        _Config = config;
        WriteOutput_Service = new CxLogService(_Config, config_TitleLineOptions, config_ProcessActionHelpInfoOptions);
    }

    /// <summary>
    /// Format that displays out the Help Info using attribute Format.
    /// </summary>
    /// <param name="override_Default">if this value is not null it will process over the default configuration Attributes</param>
    /// <returns>The Help Display For the Process</returns>
    protected ProcessHelpInfo HelpInfoFormat(Func<ProcessHelpInfo>? override_Default = default)
    {

        if (override_Default != null)
            return override_Default();

        //Process the deafult help.

        CxConsoleInfoAttribute? InfoAttribute = this.getInfoAttribute();

        if (InfoAttribute == null)//May need to do a default display
            return new();

        List<CommandHelpInfo> procInfo = new List<CommandHelpInfo>();

        foreach (var item in InfoAttribute.ProcessActions)
        {
            if (item.Value._attribute.registerType == CxRegisterTypes.Skip)
                continue;

            //Define the Action
            CommandHelpInfo actInfo = new CommandHelpInfo(item.Value._attribute.name, item.Value._attribute.description, item.Value._attribute.registerType);

            var result = item.Value.method.getInfoActionArgs().Where(w => w.arg_isActive).Select(s => new CommandArgsHelpInfo(s.arg_Key, s.arg_Description, s.arg_Type));
            actInfo.addAction_ArgsHelpInfo(result.ToArray());

            procInfo.Add(actInfo);
        }

        return new ProcessHelpInfo(procInfo);
    }

    /// <summary>
    /// Use this Delegate to override the Help Action Options
    /// </summary>
    protected Action<ProcessActionHelpInfoOptions>? config_ProcessActionHelpInfoOptions { get; set; } = null;

    /// <summary>
    /// Use this Delegate to override the Title Display
    /// </summary>
    protected Action<TitleLineOptions>? config_TitleLineOptions { get; set; } = null;

    /*
     
     protected override void config_ProcessActionHelpInfoOptions(ProcessActionHelpInfoOptions options)
        {
            var opt = _Config.GetSection("ProcessActionHelpInfoOptions").Get<ProcessActionHelpInfoOptions>();


            options.display_SystemHelperArgs = true;
            options.display_ShowExamples = true;
           
            options.ProcessDescription = opt?.ProcessDescription ?? "Default Description";
            options.ExtendInfoLines = opt?.ExtendInfoLines ?? new string[] { };           
        }    

        protected override void config_TitleLineOptions(TitleLineOptions options)
        {

            var opt = _Config.GetSection(nameof(TitleLineOptions)).Get<TitleLineOptions>();

            options.isEndLine = false;
            options.Title = opt?.Title ?? "Default Console Title";
            options.ExtraLines = opt?.ExtraLines ?? new string[] { };
            options.BorderSize = opt?.BorderSize ?? 150;
            options.IndentSize = opt?.IndentSize > 0 ? opt.IndentSize : 5;

            var delim = _Config.GetSection(nameof(TitleLineOptions))[nameof(TitleLineOptions.BorderDelim)];
            if (delim?.Length == 1 && char.TryParse(delim, out char BorderDelim))
                options.BorderDelim = BorderDelim;


            //throw new NotImplementedException();
        }
     
     
     
     
     */

    /// <summary>
    /// The Action that is ran for the Process
    /// </summary>
    /// <param name="cancellationToken">The Token to Cancle the program</param>
    public virtual Task ProcessAction(CancellationToken cancellationToken)
    {
        cancellationToken.Register(HandleCancellation);

        WriteOutput_Service.write_Lines();

       // WriteOutput_Service.write_Lines(
       //    $"My Processing Type: {GetType().Name}", "\n\n\n\n"
       //);

        var actingTask = this.get_Command(_CxCommandService)?.Invoke(this, null) as Task;

        if (actingTask == null)
            return Info();
        
       return actingTask;

        //WriteLine($"Window-Width: {Console.WindowWidth}");
        //WriteLine($"IsWindows: {OperatingSystem.IsWindows()}");
        

        //WriteLine($"Window-Width: {Console.WindowWidth}");
        //WriteLine($"Largest-Window-Width: {Console.LargestWindowWidth}");
        //WriteLine($"");

        //Internal process
       

        //return Task.CompletedTask;
    }

    /// <summary>
    /// Displays Info for the Processes Actions to the console. 
    /// Overide this method to create your own Help Display process. 
    /// Keep Formatting by overloading the following methods
    /// -- HelpInfoFormat: Display out the Actions and Action args that can be used
    /// -- config_ProcessActionHelpInfoOptions: Help Display parts
    /// -- config_TitleLineOptions: to Display Help Title Info
    /// </summary>
    public virtual Task Info()
    {
        //Will write out the info 
        //OutputDisplay.write_ProcessActionHelpInfo(, config_ProcessActionHelpInfoOptions, config_TitleLineOptions);

        WriteOutput_Service.write_ProcessActionHelpInfo(HelpInfoFormat());

        return Task.CompletedTask;
    }

    /// <summary>
    /// Tell The main process if a Cancelation has accured
    /// </summary>
    protected bool isCanceled { get; set; }

    /// <summary>
    /// The under lining Cancel Method when the CancellationToken is activated.
    /// </summary>
    protected virtual void HandleCancellation()
    {
        isCanceled = true;

        var typeName = GetType().Name;

        WriteLine($"{typeName} has a Cancellation. ");
    }

}
