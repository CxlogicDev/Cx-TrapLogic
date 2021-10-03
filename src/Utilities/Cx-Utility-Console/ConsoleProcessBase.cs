using Microsoft.Extensions.Configuration;
using CxUtility.Cx_Console.DisplayMethods;
using System.Reflection;

namespace CxUtility.Cx_Console;

public abstract class ConsoleBaseProcess : IConsoleProcessService
{
    public CxCommandService _CxCommandService { get; }

    public IConfiguration _Config { get; }

    /// <summary>
    /// The Info Attribute attched to the Process Class.
    /// </summary>
    protected CxConsoleInfoAttribute? _CxInfo => this.getInfoAttribute();

    protected CxConsoleActionAttribute? _CxCommandInfo => (_CxInfo?.ProcessActions.TryGetValue(_CxCommandService.Command, out var cmdInfo) ?? false )? cmdInfo._attribute : default;

    /// <summary>
    /// Auto init uses internal methods to write to Screen 
    /// </summary>
    public ICxLogService WriteOutput_Service { get; } 

    /// <summary>
    /// Set base of the process
    /// </summary>
    /// <param name="CxProcess">The Command service</param>
    /// <param name="config">The set up config</param>
    /// <param name="use_config_TitleLineOptions">Tells The Base to config the title to using the Default </param>
    /// <param name="use_config_ProcessActionHelpInfoOptions"></param>
    public ConsoleBaseProcess(CxCommandService CxProcess, IConfiguration config)
    {
        _CxCommandService = CxProcess;
        _Config = config;
        WriteOutput_Service = new CxLogService(_Config, CxLogService.default_override_Title_Options ?? config_TitleLineOptions, CxLogService.default_override_Help_Options ?? config_ProcessActionHelpInfoOptions);
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
    protected virtual void config_ProcessActionHelpInfoOptions(ProcessActionHelpInfoOptions options) {        
        if(_CxInfo != null)
        {
            options.ProcessDescription = _CxInfo.description;            
        }
    }

    /// <summary>
    /// Use this Delegate to override the Title Display
    /// </summary>
    protected virtual void config_TitleLineOptions(TitleLineOptions options) {
        if(_CxInfo != null)
        {
            options.Title = _CxInfo.processCallName.CaseNameDisplay();            
            options.Append_CallingInfo(
                _CxCommandService.Process, 
                _CxCommandService.Command.hasCharacters() ? $"[Invalid-Command] {_CxCommandService.Command}" : "", 
                _CxCommandService._Args.Select(s => $"{s.Key} {s.Value}").ToArray(),
                _CxInfo.registerType == CxRegisterTypes.Preview,
                _CxCommandService.Command.hasCharacters() && _CxCommandInfo != null && _CxCommandInfo.registerType == CxRegisterTypes.Preview
                );
        }
    }

    CxLogService? internal_log => WriteOutput_Service as CxLogService;

    /// <summary>
    /// The Action that is ran for the Process
    /// </summary>
    /// <param name="cancellationToken">The Token to Cancle the program</param>
    public virtual Task ProcessAction(CancellationToken cancellationToken)
    {
        cancellationToken.Register(HandleCancellation);

        //WriteOutput_Service.append_Something(_CxCommandService);
        //internal_log?._Title_Options.Append_CallingInfo(_CxCommandService.Process, _CxCommandService.Command, _CxCommandService._Args.Select(s => $"{s.Key} {s.Value}").ToArray());

        WriteOutput_Service.write_Lines();

       // WriteOutput_Service.write_Lines(
       //    $"My Processing Type: {GetType().Name}", "\n\n\n\n"
       //);

        var actingTask = this.get_Command(_CxCommandService)?.Invoke(this, null) as Task;

        if (actingTask == null)
            return Info();
        
       return actingTask;
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

        //WriteOutput_Service.write_Lines(HelpInfoFormat().ToJson());
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
