using CxUtility.Cx_Console;
using CxUtility.Cx_Console.DisplayMethods;
using Microsoft.Extensions.Configuration;
namespace Cx_TrapConsole;

internal static class BuildProcessUtility
{
    /// <summary>
    /// This is a default startucture method that can be used accross all Console Processes
    /// </summary>
    /// <param name="options">set options to set</param>
    /// <param name="processChanges">The the process changes</param>
    public static void _config_ProcessActionHelpInfoOptions(ProcessActionHelpInfoOptions options)
    {
        options.display_SystemHelperArgs = false;
        options.display_ShowExamples = false;
        options.implemented_Time_Report = false;
        options.implemented_WriteData2CSVFile = false;
        options.implemented_WriteData2JsonFile = false;


        options.ExtendInfoLines = new string[] { };

        //processChanges?.Invoke(options);
    }

    /// <summary>
    /// This is a default startucture method that can be used accross all Console Processes
    /// </summary>
    /// <param name="options">set options to set</param>
    /// <param name="processChanges">The the process changes</param>
    public static void _config_TitleLineOptions(this ConsoleBaseProcess me, TitleLineOptions options)
    {
        //CxCommandService _CxProcess
        //IConfiguration _Config
        var opt = me._Config.GetSection(nameof(TitleLineOptions)).Get<TitleLineOptions>();

        //options.isEndLine = false;
        options.Title = opt?.Title ?? "Project Process Console";// "Website Processes"

        int ct = 0;
        foreach (var c in options.Title)
        {
            if (c != ' ')
                break;
            ct++;
        }

        var lst = new List<string>() { $"{new string(' ', ct)}Process: {me._CxCommandService.Process}", $"{new string(' ', ct)}Command: {me._CxCommandService.Command}" };

        foreach (var item in me._CxCommandService.Args)
            if (item.value.hasCharacters())
                lst.Add($"{new string(' ', ct)}arg: {item.arg} >> {item.value}");
            else
                lst.Add($"{new string(' ', ct)}arg: {item.arg}");

        options.ExtraLines = lst.ToArray();

        options.BorderSize = opt?.BorderSize ?? 150;
        options.IndentSize = opt?.IndentSize > 0 ? opt.IndentSize : 5;
        options.BorderDelim = '-';
        //var delim = _Config.GetSection(nameof(TitleLineOptions))[nameof(TitleLineOptions.BorderDelim)];
        //if (delim?.Length == 1 && char.TryParse(delim, out char BorderDelim))
        //    options.BorderDelim = BorderDelim;


        //processChanges?.Invoke(options);

    }

}
