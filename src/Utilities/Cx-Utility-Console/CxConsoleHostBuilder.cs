using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
namespace CxUtility.Cx_Console;

public interface ICxConsoleHostBuilder {



    /// <summary>
    /// Runs the console Process
    /// </summary>
    Task RunConsole();

}


public interface ICxCHB_ConfigurationBuilder
{
    ICxCHB_ConfigureServices ConfigBuilder(Action<IConfigurationBuilder> builder);
}

public interface ICxCHB_ConfigureServices
{
    ICxCHB_Register ConfigureServices(Action<IServiceCollection> AddtionalServices);
}


public interface ICxCHB_Register : ICxConsoleHostBuilder
{

    ICxCHB_Register RegisterAssembly(Assembly assembly);

    /// <summary>
    /// Reqister an assembly Console types. 
    /// </summary>
    /// <param name="AssemplyTypes">The types in the asembly</param>
    ICxCHB_Register RegisterAssemblyType(params Type[] AssemplyTypes);

    
}
    
public sealed record CxConsoleHostBuilder : ICxConsoleHostBuilder, ICxCHB_ConfigurationBuilder, ICxCHB_ConfigureServices, ICxCHB_Register
{
    internal IHostBuilder iHost;
    internal string[] _args { get; set; }

    /// <summary>
    /// A Dictionary for displaying registered services. 
    /// </summary>
    internal Dictionary<string, CxConsoleInfoAttribute> _RegisteredProcesses { get; } = new Dictionary<string, CxConsoleInfoAttribute>();

    internal CxConsoleHostBuilder(string[] args)
    {
        _args = args ?? new string[0];
        iHost = Host.CreateDefaultBuilder(_args);

        iHost.ConfigureLogging((hbld, cfg_Log) => {

           

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
            foreach (var AssemplyType in AssemplyTypes.Where(w => w.BaseType == typeof(ConsoleBaseProcess)))
            {
                try
                {
                    CxConsoleInfoAttribute? cxAttribute = AssemplyType
                        .GetCustomAttributes(typeof(CxConsoleInfoAttribute), true)
                        .FirstOrDefault() as CxConsoleInfoAttribute;

                    if (cxAttribute == null || cxAttribute.registerType == CxRegisterTypes.Skip)
                        continue;

                    _RegisteredProcesses.TryAdd(cxAttribute.processCallName, cxAttribute);
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        return this;
    }

    Task ICxCHB_Register.RunConsole()
    {
       
        iHost.ConfigureServices(services =>
        {
            //Register the CxService 
            services.AddSingleton(obj => new CxCommandService(_hostBuilder._args));

            services.AddScoped<CxConsoleProcess>();

            services.AddHostedService<CxProcessHostedService>();

            if (_hostBuilder.hasRegistedProcesses)
                foreach (var item in _hostBuilder._RegisteredProcesses.Values)
                    services.AddTransient(item.processType);
        });

        using var appHost = _hostBuilder.iHost.Build();

        //Run the process 
        await appHost.StartAsync();

        //Stop the process 
        await appHost.StopAsync();

        return;


        //return default; // Task.FromResult(this);
    } 
}