global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;

using CxUtility.Cx_Console;
using Cx_TrapTestConsole;
using CxAzure.TableStorage;

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
        var TestStorageTable = Environment.GetEnvironmentVariable("Test.Azure.StorageTable");

        if (TestStorageTable.hasCharacters())
            ser.AddScoped<TableAccess>(c => new(Cx_Azure.testStorageTable, TestStorageTable));
        //new(Cx_Azure.testStorageTable, "https://cx0aws.table.core.windows.net", "cx0aws", "9tWo85i/mBWEhgrulhlzg7f3F9sl43VPdShcBtZxpAfg2GlXNjv89SJHjH9nEpBEgFeeO/zOQkSr5Bh6ier0yg==")

    })
    .RegisterAssembly(typeof(Cx_Azure).Assembly)
    //.Register_TitleLineOptions(a =>
    //{
    //    a.Title = "Cx Trap Console";
    //    //a.BorderDelim = '@';
    //})
    .RunConsole();
