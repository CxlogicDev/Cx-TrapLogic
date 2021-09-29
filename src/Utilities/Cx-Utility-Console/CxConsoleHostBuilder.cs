using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
namespace CxUtility.Cx_Console;

public interface ICxConsoleHostBuilder {


}


public interface ICxCHB_ConfigurationBuilder
{
    ICxCHB_ConfigureServices ConfigBuilder(Action<IConfigurationBuilder> builder);
}

public interface ICxCHB_ConfigureServices
{
    ICxCHB_Register ConfigureServices(Action<IServiceCollection> AddtionalServices);
}


public interface ICxCHB_Register
{

    ICxCHB_Register RegisterAssembly(Assembly assembly);

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
    

    internal ICxConsoleHostBuilder ConfigureServices(Action<IServiceCollection> AddtionalServices)
    {
        iHost.ConfigureServices(AddtionalServices);
        return this;
    }

    internal ICxConsoleHostBuilder ConfigBuilder(Action<IConfigurationBuilder> builder)
    {
        if (builder is not null)
            iHost.ConfigureAppConfiguration(builder);

        return this;
    }


    //internal IEnumerable<CxConsoleInfoAttribute> RegisteredProcesses => _RegisteredProcesses.Values;

    internal bool hasRegistedProcesses => _RegisteredProcesses.Count > 0;

    internal ICxCHB_Register RegisterAssembly(Assembly assembly)
    {
        RegisterAssemblyType(
            assembly
            .GetTypes()
            .Where(w => w.BaseType == typeof(ConsoleBaseProcess) && w.GetCustomAttribute<CxConsoleInfoAttribute>()?.registerType != CxRegisterTypes.Skip)
            .ToArray()
        );
        return this;
    }

    internal ICxCHB_Register RegisterAssemblyType(params Type[] AssemplyTypes)
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


}