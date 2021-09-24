// See https://aka.ms/new-console-template for more information
using Cx_TrapConsole;
using CxUtility.Cx_Console;

//Console.WriteLine("Hello, World!");
await CxConsoleHost
    .CxConsole_BuildHost(args)
    .CxConsole_RegisterServices(ser =>
    {
        //Write any service that is needed for process in your console Services

    })
    .CxConsole_RegisterOptions(a =>
    {
        a.Title = "Test Title";
        a.BorderDelim = '+';
    }, default)
    .CxConsole_RegisterProcessAssemblies(typeof(BuildProcess).Assembly)
    .CxConsole_RunConsole();

