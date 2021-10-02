// See https://aka.ms/new-console-template for more information
using Cx_TrapConsole;
using CxUtility.Cx_Console;

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

    })
    .RegisterAssembly(typeof(BuildProcess).Assembly)
    //.Register_TitleLineOptions(a =>
    //{
    //    a.Title = "Cx Trap Console";
    //    //a.BorderDelim = '@';
    //})
    .RunConsole();
