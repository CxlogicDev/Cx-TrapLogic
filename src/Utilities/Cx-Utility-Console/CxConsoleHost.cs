using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace CxUtility.Cx_Console;

public sealed class CxConsoleApplication : IHost, IAsyncDisposable
{
    private readonly IHost _host;

    /// <summary>
    /// The default logger for the application.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// The application's configured services.
    /// </summary>
    public IServiceProvider Services => _host.Services;

    /// <summary>
    /// The application's configured <see cref="IConfiguration"/>.
    /// </summary>
    public IConfiguration Configuration => _host.Services.GetRequiredService<IConfiguration>();

    /// <summary>
    /// Allows consumers to be notified of application lifetime events.
    /// </summary>
    public IHostApplicationLifetime Lifetime => _host.Services.GetRequiredService<IHostApplicationLifetime>();

    internal CxConsoleApplication(IHost host)
    {
        _host = host;
        Logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ConsoleApplication");
    }

    /*****************************************************************************************************************************************************/

    /// <summary>
    /// Disposes the application.
    /// </summary>
    void IDisposable.Dispose() => _host.Dispose();

    /// <summary>
    /// Disposes the application.
    /// </summary>
    public ValueTask DisposeAsync() => ((IAsyncDisposable)_host).DisposeAsync();

    /// <summary>
    /// Start the application.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A <see cref="Task"/> that represents the startup of the <see cref="WebApplication"/>.
    /// Successful completion indicates the HTTP server is ready to accept new requests.
    /// </returns>
    public Task StartAsync(CancellationToken cancellationToken = default) =>
        _host.StartAsync(cancellationToken);

    /// <summary>
    /// Shuts down the application.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>
    /// A <see cref="Task"/> that represents the shutdown of the <see cref="WebApplication"/>.
    /// Successful completion indicates that all the HTTP server has stopped.
    /// </returns>
    public Task StopAsync(CancellationToken cancellationToken = default) =>
        _host.StopAsync(cancellationToken);

    /*****************************************************************************************************************************************************/

    /// <summary>
    /// Initializes a new instance of the class with preconfigured defaults.
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>The <see cref="WebApplication"/>.</returns>
    public static CxConsoleApplication Create(string[]? args = null) =>
        (new CxConsoleBuilder(args)).Build();

    /// <summary>
    /// Initializes a new instance of the <see cref="WebApplicationBuilder"/> class with preconfigured defaults.
    /// </summary>
    /// <param name="options">The <see cref="WebApplicationOptions"/> to configure the <see cref="WebApplicationBuilder"/>.</param>
    /// <returns>The <see cref="WebApplicationBuilder"/>.</returns>
    public static CxConsoleBuilder CreateBuilder(string[]? args) =>
        new(args);
}
