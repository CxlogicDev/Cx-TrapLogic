// See https://aka.ms/new-console-template for more information
using CxUtility.Cx_Console;

//Console.WriteLine("Hello, World!");

const string helloWorld = "hello";

await CxConsoleHost
    .CxConsole_BuildHost(args)
    .CxConsole_RegisterServices(ser =>
    {
        //Write any service that is needed for process in your console Services

    })
    .CxConsole_RegisterProcessAssemblies(helloWorld.GetType().Assembly)
    .CxConsole_RunConsole();

