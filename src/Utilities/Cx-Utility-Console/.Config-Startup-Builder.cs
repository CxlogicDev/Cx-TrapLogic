global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Hosting;
using CxUtility.Cx_Console.DisplayMethods;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
namespace CxUtility.Cx_Console;

internal class CxProcessHostedService : IHostedService
{

    CxConsoleProcess _consoleProcess { get; }
    CxConsoleHost _host { get; }
    
    public CxProcessHostedService(CxConsoleProcess consoleService, CxConsoleHost host)
    {
        _consoleProcess = consoleService;    
        _host = host;    
    }

    /// <summary>
    /// Process The console
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    async Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        WriteLine();
        WriteLine();

        await _consoleProcess.MainProcess_Async(cancellationToken, _host._RegisteredProcesses);
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        WriteLine();
        WriteLine();
        //WriteLine($"Process {service?.Process ?? ""} Finished");

        //service.DisplayTimeReportItems();

        return Task.CompletedTask;
    }
}

