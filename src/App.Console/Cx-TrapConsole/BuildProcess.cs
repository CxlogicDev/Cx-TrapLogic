using CxUtility.Cx_Console;
using CxUtility.Cx_Console.DisplayMethods;
using Microsoft.Extensions.Configuration;

namespace Cx_TrapConsole;

[CxConsoleInfo(nameof(BuildProcess), typeof(BuildProcess), CxRegisterTypes.Preview, "Helps Build the Project. ")]
internal class BuildProcess : ConsoleBaseProcess
{
    public BuildProcess(CxCommandService CxProcess, IConfiguration config) : base(CxProcess, config) {

        //Override the default 
        this.config_ProcessActionHelpInfoOptions = this._config_ProcessActionHelpInfoOptions;

        //Override the default
        this.config_TitleLineOptions = this._config_TitleLineOptions;
    
    }

    [CxConsoleAction("build_tree", "Will discover the build tree and output it to the base directory or the solution", true, CxRegisterTypes.Preview)]
    public Task DiscoverSolutionTree()
    {


        WriteOutput_Service.write_Lines("Test the new Code output in the build process");


        return Task.CompletedTask;
    }


    /*
    public override Task ProcessAction(CancellationToken cancellationToken)
    {
        Console.WriteLine($"WindowBuffer-Width: {Console.BufferWidth}; WindowBuffer-Height: {Console.BufferHeight}");

        return Task.CompletedTask;
    }
    //*/
    




}

