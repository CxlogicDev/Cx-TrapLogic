using Microsoft.Extensions.Configuration;
using CxUtility.Cx_Console.DisplayMethods;


namespace CxUtility.Cx_Console;

public abstract class ConsoleBaseProcess : IConsoleProcessService
{
    public CxCommandService _CxProcess { get; }

    public IConfiguration _Config { get; }

    public ConsoleBaseProcess(CxCommandService CxProcess, IConfiguration config)
    {
        _CxProcess = CxProcess;
        _Config = config;
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
        OutputDisplay.write_ProcessActionHelpInfo(HelpInfoFormat(), config_ProcessActionHelpInfoOptions, config_TitleLineOptions);

        return Task.CompletedTask;
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
            if (!item.Value._attribute.isActive)
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
    /// This Method controls Dispaly Info Inside the Start and End Bars
    /// </summary>
    /// <param name="options">Allowed Options</param>
    protected abstract void config_ProcessActionHelpInfoOptions(ProcessActionHelpInfoOptions options);

    /// <summary>
    /// This Method Controls Display Info in the Tiltle Bar
    /// </summary>
    /// <param name="options">The alllowed Options</param>
    protected abstract void config_TitleLineOptions(TitleLineOptions options);

    internal Task runProcess(CancellationToken cancellationToken)
    {
        cancellationToken.Register(HandleCancellation);

        return ProcessAction(cancellationToken);
    }

    /// <summary>
    /// The Action that is ran for the Process
    /// </summary>
    /// <param name="cancellationToken">The Token to Cancle the program</param>
    public abstract Task ProcessAction(CancellationToken cancellationToken);

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
