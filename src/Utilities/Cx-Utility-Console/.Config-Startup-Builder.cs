global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
namespace CxUtility.Cx_Console;



public static class CxConsoleHost
{
#pragma warning disable CS8618 // Value will never be null when it is used  
    internal static CxConsoleHostBuilder _hostBuilder;
#pragma warning restore CS8618 

    /// <summary>
    /// Build the Cx Console Parts 
    /// </summary>
    /// <param name="registerAssemblies"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static ICxConsoleHostBuilder CxConsole_BuildHost(string[] args)
    {
        _hostBuilder = new CxConsoleHostBuilder(args);       
        return _hostBuilder;
    }

    /// <summary>
    /// Attach services and process that are needed by the process implenattaions.
    /// </summary>
    /// <param name="builder">The Created Builder that was supplied.</param>
    /// <param name="AddtionalServices">The services that need to be included</param>
    public static ICxConsoleHostBuilder CxConsole_RegisterServices(this ICxConsoleHostBuilder builder, Action<IServiceCollection> AddtionalServices)
    {
        _hostBuilder.ConfigureServices(AddtionalServices);

        return builder.HostBuilder_Check();
    }

    /// <summary>
    /// Will register Any Type in the supplied Assembly with a base type of ConsoleBaseProcess and Attribute of CxConsoleInfo
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="RegisterAssemblies"></param>
    /// <returns></returns>
    public static ICxConsoleHostBuilder CxConsole_RegisterProcessAssemblies(this ICxConsoleHostBuilder builder, params Assembly[] RegisterAssemblies)
    {
        if (RegisterAssemblies?.Length > 0)
            foreach (var RegAssembly in RegisterAssemblies)
                _hostBuilder.RegisterAssembly(RegAssembly);

        return builder.HostBuilder_Check();
    }

    public static async Task CxConsole_RunConsole(this ICxConsoleHostBuilder builder)
    {
        //Error if not the correct builder class
        builder.HostBuilder_Check();

        _hostBuilder.ConfigureServices(services =>
        {
            //Register the Cxservice 
            services.AddSingleton(obj => new CxCommandService(_hostBuilder._args));

            services.AddScoped<CxConsoleProcess>();

            services.AddHostedService<CxProcessHostedService>();

            if (_hostBuilder.hasRegistedProcesses)
                foreach (var item in _hostBuilder._RegisteredProcesses.Values)
                {
                    services.AddTransient(item.processType);
                }
        });

        using var appHost = _hostBuilder.iHost.Build();

        //Run the process 
        await appHost.StartAsync();

        //Stop the process 
        await appHost.StopAsync();

        return;
    }

    /// <summary>
    /// Check to let caller know there impemantation is not supported. 
    /// </summary>
    /// <exception cref="Exception">None Supported type get thrown</exception>
   static ICxConsoleHostBuilder HostBuilder_Check(this ICxConsoleHostBuilder builder)
    {
        if (builder.GetType() != typeof(CxConsoleHostBuilder))
            throw new Exception("The Build Inteface has a mismatch and the process cannot continue. The Interface used is intended for Building the CxConsoleHost only. Outside Impementations are not Supported.");

        return builder;
    }
}


internal class CxProcessHostedService : IHostedService
{

    CxConsoleProcess consoleProcess { get; }
    
    public CxProcessHostedService(CxConsoleProcess consoleService)
    {
        consoleProcess = consoleService;        
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

        await consoleProcess.MainProcess(cancellationToken, CxConsoleHost._hostBuilder._RegisteredProcesses);
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



/*
 
 var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureAppConfiguration(Configure_Project)
    .ConfigureAppConfiguration(cfgBld => cfgBld.AddInMemoryCollection(new[] { new KeyValuePair<string, string>("args", string.Join("|", args)) }));

builder.ConfigureServices(Configure_Services);

using var appHost = builder.Build();

await appHost.StartAsync();

await appHost.StopAsync();

Console.WriteLine("Hello, World!");

 
 
 */

//static class StartupUtility 
//{
//    public static void Configure_Project(IConfigurationBuilder _Config)
//    {
//        //Action<HostBuilderContext, IConfigurationBuilder> configureDelegate
//    }

//    public static void Configure_Services(HostBuilderContext _Host, IServiceCollection _Services)
//    {

//    }
//}