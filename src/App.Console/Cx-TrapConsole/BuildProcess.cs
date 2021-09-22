using CxUtility.Cx_Console;
using CxUtility.Cx_Console.DisplayMethods;
using Microsoft.Extensions.Configuration;

namespace Cx_TrapConsole;

[CxConsoleInfo(nameof(BuildProcess), typeof(BuildProcess), CxRegisterTypes.Preview, "Helps Build the Project. ")]
internal class BuildProcess : ConsoleBaseProcess
{
    public BuildProcess(CxCommandService CxProcess, IConfiguration config) : base(CxProcess, config) { }

    public override Task ProcessAction(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    protected override void config_ProcessActionHelpInfoOptions(ProcessActionHelpInfoOptions options) => 
        this._config_ProcessActionHelpInfoOptions(options);
    

    protected override void config_TitleLineOptions(TitleLineOptions options) => 
        this._config_TitleLineOptions(options);
    
}

