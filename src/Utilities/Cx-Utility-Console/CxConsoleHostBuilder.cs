using CxUtility.Cx_Console.DisplayMethods;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
namespace CxUtility.Cx_Console;


/// <summary>
/// Common Across All Builders
/// </summary>
public interface ICxCHB { }

/// <summary>
/// Registes or builds configuration
/// </summary>
public interface ICxCHB_ConfigurationBuilder : ICxCHB
{
    /// <summary>
    /// Writes any configuration for the project DI
    /// </summary>
    /// <param name="builder">The Configuration for the blder</param>
    ICxCHB_ConfigureServices ConfigBuilder(Action<IConfigurationBuilder> builder);
}

/// <summary>
/// Register or build Services
/// </summary>
public interface ICxCHB_ConfigureServices : ICxCHB
{
    /// <summary>
    /// Write any services needed for Console processes
    /// </summary>
    /// <param name="AddtionalServices">The User added services</param>
    ICxCHB_Register ConfigureServices(Action<IServiceCollection> AddtionalServices);
}

/// <summary>
/// Run the console process 
/// </summary>
public interface ICxConsoleHostBuilder : ICxCHB
{

    /// <summary>
    /// Runs the console Process
    /// </summary>
    Task RunConsole();

    /*ToDo: Need to build in fluent Methods for help - info display  */

}

/// <summary>
/// Register service for optional options and types
/// </summary>
public interface ICxCHB_Register : ICxConsoleHostBuilder
{
    /// <summary>
    /// Register any Assemblies that will search and add the process types assoceated to the Call
    /// </summary>
    /// <param name="assembly">An asseble that has console processes</param>
    ICxCHB_Register RegisterAssembly(Assembly assembly);

    /// <summary>
    /// Reqister an assembly Console types. 
    /// </summary>
    /// <param name="AssemplyTypes">The types in the asembly</param>
    ICxCHB_Register RegisterAssemblyType(params Type[] AssemplyTypes);

    /// <summary>
    /// Register a default set of Title options
    /// </summary>
    /// <param name="Title_Options">Option that need to be set</param>    
    ICxCHB_Register Register_TitleLineOptions(Action<TitleLineOptions> Title_Options);

    /// <summary>
    /// Register a default set of help options
    /// </summary>
    /// <param name="Help_Options">option that need to be set</param>
    ICxCHB_Register Register_ProcessActionHelpInfoOptions(Action<ProcessActionHelpInfoOptions>? Help_Options);

}

internal record CxConsoleHost
{
    public CxConsoleHost(string[] args)
    {
        _ProcessCmd = new CxCommandService(args);
    }

    /// <summary>
    /// The command service that houses the consle path
    /// </summary>
    public CxCommandService _ProcessCmd { get; }

    /// <summary>
    /// The services that are registed
    /// </summary>
    public Dictionary<string, CxConsoleInfoAttribute> _RegisteredProcesses { get; } = new();

    /// <summary>
    /// Quick ref if there are any registed services
    /// </summary>
    public bool hasRegistedProcesses => _RegisteredProcesses.Count > 0;
}


public sealed record CxConsoleHostBuilder : ICxConsoleHostBuilder, ICxCHB_ConfigurationBuilder, ICxCHB_ConfigureServices, ICxCHB_Register
{
    internal IHostBuilder iHost { get; }

    //internal string[] _args { get; }

    CxConsoleHost _host { get; }

    CxCommandService _ProcessCmd => _host._ProcessCmd;

    /// <summary>
    /// A Dictionary for displaying registered services. 
    /// </summary>
    //internal Dictionary<string, CxConsoleInfoAttribute> _RegisteredProcesses { get; } = new Dictionary<string, CxConsoleInfoAttribute>();
    
    internal Dictionary<string, CxConsoleInfoAttribute> _RegisteredProcesses => _host._RegisteredProcesses;
    
    internal CxConsoleHostBuilder(string[] args)
    {
        _host = new CxConsoleHost(args ?? new string[0]);

        iHost = Host.CreateDefaultBuilder(args)
            .ConfigureLogging((hbld, cfg_Log) => {

            cfg_Log.SetMinimumLevel(LogLevel.None);

            //cfg_Log.ClearProviders();
            //cfg_Log.AddConsole();
            //cfg_Log.Properties
        });
    }

    public static ICxCHB_ConfigurationBuilder CreateConsole_HostBuilder(string[] args) => new CxConsoleHostBuilder(args);

    ICxCHB_Register ICxCHB_ConfigureServices.ConfigureServices(Action<IServiceCollection> AddtionalServices)
    {
            iHost.ConfigureServices(AddtionalServices);
        return this;
    }

    ICxCHB_ConfigureServices ICxCHB_ConfigurationBuilder.ConfigBuilder(Action<IConfigurationBuilder> builder)
    {
        if (builder is not null)
            iHost.ConfigureAppConfiguration(builder);

        return this;
    }


    //internal IEnumerable<CxConsoleInfoAttribute> RegisteredProcesses => _RegisteredProcesses.Values;

    internal bool hasRegistedProcesses => _RegisteredProcesses.Count > 0;

    ICxCHB_Register ICxCHB_Register.RegisterAssembly(Assembly assembly)
    {
        ((ICxCHB_Register)this).RegisterAssemblyType(
            assembly
            .GetTypes()
            .Where(w => w.BaseType == typeof(ConsoleBaseProcess) && w.GetCustomAttribute<CxConsoleInfoAttribute>()?.registerType != CxRegisterTypes.Skip)
            .ToArray()
        );
        return this;
    }

    ICxCHB_Register ICxCHB_Register.RegisterAssemblyType(params Type[] AssemplyTypes)
    {
        if (AssemplyTypes.Length > 0)
        {

            IEnumerable<CxConsoleInfoAttribute?> assemblyTypes =
                AssemplyTypes.Where(w => w.BaseType == typeof(ConsoleBaseProcess))
                .Select(s => s.GetCustomAttributes(typeof(CxConsoleInfoAttribute), true).FirstOrDefault() as CxConsoleInfoAttribute);

            CxConsoleInfoAttribute? InfoAtt = null;
            if (_ProcessCmd.isValid())
                InfoAtt = assemblyTypes.FirstOrDefault(a => _ProcessCmd.Process.Equals(a?.processCallName));

            if (InfoAtt is not null)
            {
                _RegisteredProcesses.TryAdd(InfoAtt.processCallName, InfoAtt);
                return this;
            }

            foreach (var cxAttribute in assemblyTypes.Where(w => w is not null && w.registerType != CxRegisterTypes.Skip))
                if (cxAttribute != null)
                    _RegisteredProcesses.TryAdd(cxAttribute.processCallName, cxAttribute);            
        }

        return this;
    }

    ICxCHB_Register ICxCHB_Register.Register_TitleLineOptions(Action<TitleLineOptions> Title_Options)
    {
        if (Title_Options != null)
            CxLogService.default_override_Title_Options = Title_Options;

        return this;
    }

    ICxCHB_Register ICxCHB_Register.Register_ProcessActionHelpInfoOptions(Action<ProcessActionHelpInfoOptions>? Help_Options)
    {
        if (Help_Options != null)
            CxLogService.default_override_Help_Options = Help_Options;

        return this;
    }

    async Task ICxConsoleHostBuilder.RunConsole()
    {
       
        iHost.ConfigureServices(services =>
        {            
            //Register the CxService 
            services.AddSingleton(obj => _ProcessCmd);

            services.AddScoped(f => _host);

            services.AddScoped<CxConsoleProcess>();

            services.AddHostedService<CxProcessHostedService>();

            if (hasRegistedProcesses)
                foreach (var item in _RegisteredProcesses.Values)
                        services.AddTransient(item.processType);                    
        });

        using var appHost = iHost.Build();

        //Run the process 
        await appHost.StartAsync();

        //Stop the process 
        await appHost.StopAsync();

        return;
    }
}