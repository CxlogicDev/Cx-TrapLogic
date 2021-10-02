using CxUtility.Cx_Console.DisplayMethods;
using Microsoft.Extensions.Configuration;

namespace CxUtility.Cx_Console;


[CxConsoleInfo(nameof(CxConsoleProcess), typeof(CxConsoleProcess), CxRegisterTypes.Register, "The Main starting process that is ran and navigtaes to the calling process")]
public class CxConsoleProcess : ConsoleBaseProcess
{    
    IServiceProvider services { get; }
    CxConsoleHost _host { get; }

    public CxConsoleProcess(CxCommandService CxProcess, IServiceProvider Services, IConfiguration config) : base(CxProcess, config)
    {
        services = Services;
        _host = (services.GetService(typeof(CxConsoleHost)) as CxConsoleHost) ?? 
            throw new ArgumentException("No host service found.");
    }
    
    //public override Task ProcessAction(CancellationToken cancellationToken) => Info();

    public Task MainProcess_Async(CancellationToken cancellationToken, Dictionary<string, CxConsoleInfoAttribute> _RegisteredProcesses)
    {
        if (_CxCommandService.isValid() && _RegisteredProcesses.TryGetValue(_CxCommandService.Process, out CxConsoleInfoAttribute? registed) && registed != null)//_CxProcess.Process.hasCharacters()
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
                /*cmd._attribute.isActive &&*/ cmd._attribute.registerType != CxRegisterTypes.Skip)
                return service.ProcessAction(cancellationToken);

            return service.Info();
        }

        return Info();
    }

    /**/
    public override Task Info()
    {
        //Will write out the info 
        //OutputDisplay.write_ProcessActionHelpInfo(, config_ProcessActionHelpInfoOptions, config_TitleLineOptions);

        WriteOutput_Service.write_ProcessActionHelpInfo(_HelpInfoFormat());

        return Task.CompletedTask;
    }
    
    protected ProcessHelpInfo _HelpInfoFormat()
    {

        /*
            This is were I need to display Either Default something or an override to show the registered methods and Calls             
         */

        if (_host.hasRegistedProcesses)//
        {
            //Over and return 
            var InfoHelp = base.HelpInfoFormat();

            foreach (var item in _host._RegisteredProcesses)
            {
                InfoHelp.addProcessActionHelpInfo(new CommandHelpInfo(item.Value.processCallName, item.Value.description, item.Value.registerType));
            }

            return InfoHelp;
        }


        //Show a default of Something
        return base.HelpInfoFormat();
    }
}