using Microsoft.Extensions.Configuration;

namespace CxUtility.Cx_Console;


[CxConsoleInfo(nameof(CxConsoleProcess), typeof(CxConsoleProcess), CxRegisterTypes.Register, "The Main starting process that is ran and navigtaes to the calling process")]
public class CxConsoleProcess : ConsoleBaseProcess
{
    internal static string _ProcessHeadTitle { get; set; }
    internal static string _ProcessDescription { get; set; }

    IServiceProvider services { get; }

    public CxConsoleProcess(CxCommandService CxProcess, IServiceProvider Services, IConfiguration config) : base(CxProcess, config)
    {
        services = Services;
    }

    protected override void config_ProcessActionHelpInfoOptions(DisplayMethods.ProcessActionHelpInfoOptions options)
    {
        throw new NotImplementedException();
    }

    protected override void config_TitleLineOptions(DisplayMethods.TitleLineOptions options)
    {
        throw new NotImplementedException();
    }

    public override Task ProcessAction(CancellationToken cancellationToken) => Info();

    public Task MainProcess(CancellationToken cancellationToken, Dictionary<string, CxConsoleInfoAttribute> _RegisteredProcesses)
    {
        if (_CxCommandService.isValid() && _RegisteredProcesses.TryGetValue(_CxCommandService.Process, out CxConsoleInfoAttribute? registed))//_CxProcess.Process.hasCharacters()
        {
            //WriteLine($"Process: {_CxProcess.Process}; Action: {_CxProcess.Command ?? "None"}; Arguments: {(_CxProcess._Args.Count > 0 ? string.Join(" ", _CxProcess._Args.Keys) : "")}");

            //Will only gett her if the method has been registed

            var obj = services.GetService(registed.processType);

            var service = obj as IConsoleProcessService;

            if (service == null)
                throw new Exception("Could not process no Service found");

            //Need to look for the Registed command
            if (!_CxCommandService.isHelpCall() &&
                registed.ProcessActions.TryGetValue(_CxCommandService.Command ?? "026c27a5-748b-4bee-b890-5178c0725eb2", out var cmd) &&
                cmd._attribute.isActive)
                return service.ProcessAction(cancellationToken);

            return service.Info();
        }

        return ProcessAction(cancellationToken);
    }


    //public override Task Info()
    //{
    //    /*
    //    Need to list Info.  
    //    */

    //    CxConsoleInfoAttribute InfoAttribute = this.getInfoAttribute();

    //    if (InfoAttribute == null)//May need to do a default display
    //        return Task.CompletedTask;

    //    List<ProcessActionHelpInfo> procInfo = new List<ProcessActionHelpInfo>();

    //    foreach (var item in InfoAttribute.ProcessActions)
    //    {

    //        var _action = InfoAttribute.ProcessType.GetMethod(item.Key);

    //        if (item.Value.isActive)
    //        {
    //            //Define the Action
    //            ProcessActionHelpInfo actInfo = new ProcessActionHelpInfo(item.Value.name, item.Value.description);

    //            foreach (var i in _action.getInfoActionArgs().Where(w => w.arg_isActive))
    //            {
    //                actInfo.addArgsHelpInfo(new ActionArgsHelpInfo(i.arg_Key, i.arg_Description, i.arg_Type));
    //            }

    //            procInfo.Add(actInfo);
    //        }
    //    }

    //    ProcessHelpInfo p = new ProcessHelpInfo();
    //    p._ProcessActions.AddRange(procInfo);

    //    //Will write out the info 
    //    OutputDisplay.write_ProcessActionHelpInfo(p, config_ProcessActionHelpInfoOptions, config_TitleLineOptions);

    //    return Task.CompletedTask;
    //}

    /**/

    //protected override ProcessHelpInfo HelpInfoFormat()
    //{

    //    /*
    //        This is were I need to display Either Default something or an override to show the registered methods and Calls             
    //     */

    //    if (hasRegistedProcesses)//
    //    {
    //        //Over and return 
    //        var InfoHelp = base.HelpInfoFormat();

    //        foreach (var item in _RegisteredProcesses)
    //        {
    //            InfoHelp.addActionHelpInfo(new ProcessActionHelpInfo(item.Value.ProcessCallName, item.Value.Description));
    //        }


    //        return InfoHelp;
    //    }


    //    //Show a default of Something
    //    return base.HelpInfoFormat();
    //}

    /**/

    //protected override void config_ProcessActionHelpInfoOptions(ProcessActionHelpInfoOptions options)
    //{
    //    var opt = _Config.GetSection("ProcessActionHelpInfoOptions").Get<ProcessActionHelpInfoOptions>();


    //    options.display_SystemHelperArgs = true;
    //    options.display_ShowExamples = true;

    //    options.ProcessDescription = opt?.ProcessDescription ?? "Default Description";
    //    options.ExtendInfoLines = opt?.ExtendInfoLines ?? new string[] { };
    //}

    /**/

    //protected override void config_TitleLineOptions(TitleLineOptions options)
    //{

    //    var opt = _Config.GetSection(nameof(TitleLineOptions)).Get<TitleLineOptions>();

    //    options.isEndLine = false;
    //    options.Title = opt?.Title ?? "Default Console Title";
    //    options.ExtraLines = opt?.ExtraLines ?? new string[] { };
    //    options.BorderSize = opt?.BorderSize ?? 150;
    //    options.IndentSize = opt?.IndentSize > 0 ? opt.IndentSize : 5;

    //    var delim = _Config.GetSection(nameof(TitleLineOptions))[nameof(TitleLineOptions.BorderDelim)];
    //    if (delim?.Length == 1 && char.TryParse(delim, out char BorderDelim))
    //        options.BorderDelim = BorderDelim;


    //    //throw new NotImplementedException();
    //}


    #region Helper Methods 

    //internal void write_ProcessInfo(CxProcessAttribute[] processes)
    //{

    //    WriteLine();
    //    WriteLine();

    //    write_TitleLine(ProcessHeadTitle);
    //    WriteLine();
    //    WriteLine();
    //    WriteLine($"   Description: {ProcessDescription}");

    //    var Available_Processes = processes.Where(w => w._Register == CxRegisterTypes.Register).ToArray();
    //    if (Available_Processes.Length > 0)
    //    {
    //        WriteLine();
    //        WriteLine();
    //        WriteLine("   >> Available Processes <<");
    //        WriteLine();

    //        foreach (var proAct in Available_Processes)
    //            WriteLine($"     - {proAct._CxProcessType} > {proAct._description}");
    //    }

    //    var Preview_Processes = processes.Where(w => w._Register == CxRegisterTypes.Preview).ToArray();
    //    if (Preview_Processes.Length > 0)
    //    {
    //        WriteLine();
    //        WriteLine();
    //        WriteLine("   >> Preview Processes <<");
    //        WriteLine();

    //        foreach (var proAct in Preview_Processes)
    //            WriteLine($"     - [Preview] {proAct._CxProcessType} > {proAct._description}");
    //    }

    //    helper_args();

    //    WriteLine();
    //    WriteLine();

    //    WriteLine("   Examples: ");
    //    WriteLine("    - dotnet run -- [process] [action] [[-arg]...]");
    //    WriteLine("    - CxLogic.exe [process] [action] [[-arg]...]");

    //    WriteLine();
    //    WriteLine();
    //    write_TitleLine(ProcessHeadTitle, true);
    //}


    #endregion


    

}