global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

using CxUtility.Cx_Console;
using Cx_TrapTestConsole;
using CxAzure.TableStorage;
using CxAzure.BlobStorage;

/// <summary>
/// Building and starting a CxConsle hosed project
/// </summary>
await CxConsoleHostBuilder
    .CreateConsole_HostBuilder(args)
    .ConfigBuilder(cfgBld =>
    {
      
    })
    .ConfigureServices(ser =>
    {
        //Write any service that is needed for process in your console Services
        var TestStorage = Environment.GetEnvironmentVariable("Test.Azure.StorageTable");

        if (TestStorage.hasCharacters())
        {
            ser.AddScoped<TableAccess>(c => new(Cx_Azure_Test.testStorageTable, TestStorage));
            ser.AddScoped<BlobAccess>(b => new(Cx_Azure_Test.testBlobContainer, TestStorage));//testBlobContainer
        }

        ser.AddDbContext<CxEFCoreSqliteContext>(op => op.UseSqlite($"Data Source={CxEFCoreSqliteContext.sqliteTest};"));

        ser.AddHttpClient();

    })
    .RegisterAssembly(typeof(Cx_Azure_Test).Assembly)
    //.Register_TitleLineOptions(a =>
    //{
    //    a.Title = "Cx Trap Console";
    //    //a.BorderDelim = '@';
    //})
    .RunConsole();
