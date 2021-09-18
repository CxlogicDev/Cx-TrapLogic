using Microsoft.Extensions.Configuration;

namespace CxUtility.Cx_Console;


[CxConsoleInfo(nameof(ConsoleProcess), typeof(ConsoleProcess), CxRegisterTypes.Register, "The Main starting process that is ran and navigtaes to the calling process")]
public class ConsoleProcess : ConsoleBaseProcess
{

    internal static string _ProcessHeadTitle { get; set; }
    internal static string _ProcessDescription { get; set; }

    IServiceProvider services { get; }

    public ConsoleProcess(CxCommandService CxProcess, IServiceProvider Services, IConfiguration config) : base(CxProcess, config)
    {
        services = Services;
    }

    //protected override string ProcessHeadTitle => _ProcessHeadTitle;

    //protected override string ProcessDescription => _ProcessDescription;

    public override Task MainProcess(CancellationToken cancellationToken)
    {
        if (_RegisteredProcesses.TryGetValue(_CxProcess.Process, out CxConsoleInfoAttribute registed))//_CxProcess.Process.hasCharacters()
        {
            //WriteLine($"Process: {_CxProcess.Process}; Action: {_CxProcess.Command ?? "None"}; Arguments: {(_CxProcess._Args.Count > 0 ? string.Join(" ", _CxProcess._Args.Keys) : "")}");

            //Will only gett her if the method has been registed

            var obj = services.GetService(registed.ProcessType);

            var service = obj as IConsoleProcessService;

            //Need to look for the Registed command
            if (!_CxProcess.isHelpCall() &&
                registed.ProcessActions.TryGetValue(_CxProcess.Command ?? "026c27a5-748b-4bee-b890-5178c0725eb2", out var cmd) &&
                cmd._attribute.isActive)
                return service.MainProcess(cancellationToken);


            return service.Info();
        }

        return Info();
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

    protected override ProcessHelpInfo HelpInfoFormat()
    {

        /*
            This is were I need to display Either Default something or an override to show the registered methods and Calls             
         */

        if (hasRegistedProcesses)//
        {
            //Over and return 
            var InfoHelp = base.HelpInfoFormat();

            foreach (var item in _RegisteredProcesses)
            {
                InfoHelp.addActionHelpInfo(new ProcessActionHelpInfo(item.Value.ProcessCallName, item.Value.Description));
            }


            return InfoHelp;
        }


        //Show a default of Something
        return base.HelpInfoFormat();
    }

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


    #region Register Service

    /// <summary>
    /// A Dictionary for displaying registered services. 
    /// </summary>
    private static Dictionary<string, CxConsoleInfoAttribute> _RegisteredProcesses { get; } = new Dictionary<string, CxConsoleInfoAttribute>();


    internal static void RegisterAssemblyType(params Type[] AssemplyTypes)
    {
        if (!(AssemplyTypes.Length > 0))
            return;

        var BaseTypes = AssemplyTypes.Where(w => w.BaseType == typeof(ConsoleBaseProcess)).ToArray();

        foreach (var item in BaseTypes)
        {
            try
            {
                var cxAttribute = item
                    .GetCustomAttributes(typeof(CxConsoleInfoAttribute), true)
                    .FirstOrDefault() as CxConsoleInfoAttribute;

                if (cxAttribute?.RegisterType != CxRegisterTypes.None)
                    _RegisteredProcesses.TryAdd(cxAttribute.ProcessCallName, cxAttribute);

            }
            catch (Exception)
            {
                continue;
            }
        }

        /*

        foreach (var item in AssemplyTypes)
        {
            if (_RegisteredProcesses.ContainsKey(item))
                continue;
        }


        RegisterAssembly(AssemplyTypes.Select(s => s.Assembly).Distinct().ToArray());

        */
    }

    internal static bool hasRegistedProcesses => _RegisteredProcesses.Count > 0;

    internal static void RegisterAssembly(System.Reflection.Assembly assemblies)
    {
        /*
            AssemplyTypes.Where(w => w.BaseType == typeof(ConsoleBaseProcess)).ToArray()
        */

        var assemblyTypes = assemblies
            .GetTypes().Where(w => w.BaseType == typeof(ConsoleBaseProcess)).ToArray();
        //.Select(s => s.GetTypes().Where(w => w.BaseType == typeof(ConsoleBaseProcess)).ToArray());

        //foreach (var items in assemblyTypes)
        //RegisterAssembly(items);
        RegisterAssemblyType(assemblyTypes);

        /*
            {
                //
                    if (_RegisteredProcesses.ContainsKey(item))
                        continue;
                    // var MyHostRegistered = typeof(CxProcessService).Assembly.GetTypes().Where(w => w.BaseType == typeof(BaseHostedService)).ToArray()

                    var Att = (CxConsoleAttribute)item.GetCustomAttributes(typeof(CxConsoleAttribute), false).FirstOrDefault();

                    if ((Att?.RegisterType ?? CxRegisterTypes.None) == CxRegisterTypes.None)
                    {
                        continue;
                    }

                    _RegisteredProcesses.Add(Att.ProcessType, Att);
            }
        */
    }

    #endregion

}