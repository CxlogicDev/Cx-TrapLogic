global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
namespace CxUtility.Cx_Console;

public static class CxConsoleHost
{
    static IHostBuilder? iHost;
    static string[] _args { get; set; }

    /// <summary>
    /// Build the Cx Console Parts 
    /// </summary>
    /// <param name="registerAssemblies"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IHostBuilder CxConsole_BuildHost(string[] args)
    {
        iHost = Host.CreateDefaultBuilder(args);
        _args = args;
        return iHost;
    }

    public static IHostBuilder CxConsole_RegisterServices(this IHostBuilder builder, Action<IServiceCollection> AddtionalServices) =>
         iHost.ConfigureServices(AddtionalServices);

    static List<Assembly> RegisterList { get; } = new();

    public static IHostBuilder CxConsole_RegisterProcessAssemblies(this IHostBuilder builder, params Assembly[] RegisterAssemblies)
    {
        foreach (var RegisterAssembly in RegisterAssemblies)
            if (!RegisterList.Contains(RegisterAssembly))
                RegisterList.Add(RegisterAssembly);

        return builder;
    }


    public static async Task CxConsole_RunConsole(this IHostBuilder builder)
    {
        if (iHost == null)
            return;
                
        iHost.ConfigureServices(services =>
        {
            //Register the Cxservice 
            services.AddScoped(obj => new CxCommandService(_args));
        });

        using var appHost = iHost.Build();

        //Run the process 
        await appHost.StartAsync();

        //Stop the process 
        await appHost.StopAsync();

        return;
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