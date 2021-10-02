global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Hosting;
using CxUtility.Cx_Console.DisplayMethods;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
namespace CxUtility.Cx_Console;



//public static class CxConsoleHost
//{
//#pragma warning disable CS8618 // Value will never be null when it is used  
//    internal static CxConsoleHostBuilder _hostBuilder;
//#pragma warning restore CS8618 

//    /// <summary>
//    /// Build the Cx Console Host and retrurns a builder. 
//    /// </summary>
//    /// <param name="args">The start up args</param>
//    /// <param name="_ConfigBuilder">The Config Builder to build any services</param>
//    public static ICxConsoleHostBuilder CxConsole_BuildHost(string[] args, Action<IConfigurationBuilder>? _ConfigBuilder = default)
//    {
//        _hostBuilder = new CxConsoleHostBuilder(args);   
//        if(_ConfigBuilder is not null)
//            _hostBuilder.ConfigBuilder(_ConfigBuilder);

//        //if (OperatingSystem.IsWindows())
//        //    WindowWidth = LargestWindowWidth;

//        return _hostBuilder;
//    }

//    /// <summary>
//    /// This is for the Title and border lines displayed in the out put 
//    /// </summary>
//    /// <param name="builder">The Host Builder</param>
//    /// <param name="Title_Options">The Title Options To send across All Processes. Note: Any Set localy will override this</param>
//    /// <param name="Help_Options">The Help Options to send across All Processes. Note: Any Set localy will override this</param>
//    public static ICxConsoleHostBuilder CxConsole_RegisterOptions(this ICxConsoleHostBuilder builder, Action<TitleLineOptions>? Title_Options, Action<ProcessActionHelpInfoOptions>? Help_Options)
//    {
//        if (Help_Options != null)
//            CxLogService.default_override_Help_Options = Help_Options;

//        if (Title_Options != null)
//            CxLogService.default_override_Title_Options = Title_Options;

//        return builder;
//    }

//    /// <summary>
//    /// Attach services and process that are needed by the process implenattaions.
//    /// </summary>
//    /// <param name="builder">The Created Builder that was supplied.</param>
//    /// <param name="AddtionalServices">The services that need to be included</param>
//    public static ICxConsoleHostBuilder CxConsole_RegisterServices(this ICxConsoleHostBuilder builder, Action<IServiceCollection> AddtionalServices)
//    {
//        _hostBuilder.ConfigureServices(AddtionalServices);

//        return builder.HostBuilder_Check();
//    }

//    /// <summary>
//    /// Will register Any Type in the supplied Assembly with a base type of ConsoleBaseProcess and Attribute of CxConsoleInfo
//    /// </summary>
//    /// <param name="builder">The Host Builder</param>
//    /// <param name="RegisterAssemblies">The Assemblies to add to the console</param>
//    public static ICxConsoleHostBuilder CxConsole_RegisterProcessAssemblies(this ICxConsoleHostBuilder builder, params Assembly[] RegisterAssemblies)
//    {
//        if (RegisterAssemblies?.Length > 0)
//            foreach (var RegAssembly in RegisterAssemblies)
//                _hostBuilder.RegisterAssembly(RegAssembly);

//        return builder.HostBuilder_Check();
//    }

//    /// <summary>
//    /// Check to Error if the call is trying to implement there own Host builder. 
//    /// </summary>
//    /// <param name="builder">The Host Builder</param> 
//    /// <exception cref="Exception">None Supported type get thrown</exception>
//    static ICxConsoleHostBuilder HostBuilder_Check(this ICxConsoleHostBuilder builder)
//    {
//        /*
//          There is no supported need at this point for implementing own custome builder.
//         */

//        if (builder.GetType() != typeof(CxConsoleHostBuilder))
//            throw new Exception("The Build Inteface has a mismatch and the process cannot continue. The Interface used is intended for Building the CxConsoleHost only. Outside Impementations are not Supported.");

//        return builder;
//    }

//    /// <summary>
//    /// The Build and run process for the console
//    /// </summary>
//    /// <param name="builder">The Host Builder</param>
//    public static async Task CxConsole_RunConsole(this ICxConsoleHostBuilder builder)
//    {
//        //Error if not the correct builder class
//        builder.HostBuilder_Check();

//        _hostBuilder.ConfigureServices(services =>
//        {
//            //Register the CxService 
//            services.AddSingleton(obj => new CxCommandService(_hostBuilder._args));

//            services.AddScoped<CxConsoleProcess>();

//            services.AddHostedService<CxProcessHostedService>();

//            if (_hostBuilder.hasRegistedProcesses)
//                foreach (var item in _hostBuilder._RegisteredProcesses.Values)
//                    services.AddTransient(item.processType);                
//        });

//        using var appHost = _hostBuilder.iHost.Build();

//        //Run the process 
//        await appHost.StartAsync();

//        //Stop the process 
//        await appHost.StopAsync();

//        return;
//    }

//}


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