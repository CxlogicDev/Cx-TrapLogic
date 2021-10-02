global using static System.Console;

namespace CxUtility.Cx_Console.DisplayMethods;

/// <summary>
/// Will Write to the console in using internal methods 
/// </summary>
public interface ICxLogService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="lines"></param>
    void write_Lines(params string[] lines);

    /// <summary>
    /// 
    /// </summary>
    void write_Table();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="helpInfo"></param>
    void write_ProcessActionHelpInfo(ProcessHelpInfo helpInfo);
}

internal class CxLogService : ICxLogService
{
    /// <summary>
    /// Display title code
    /// </summary>
    TitleLineOptions _Title_Options { get;  }

    /// <summary>
    /// Help display 
    /// </summary>
    ProcessActionHelpInfoOptions _Help_Options { get; }

    IConfiguration _Config { get; }

    bool _first_Line_Written { get; set; } = false;

    public CxLogService(IConfiguration Config, Action<TitleLineOptions>? Title_Options, Action<ProcessActionHelpInfoOptions>? Help_Options)
    {
        _Config = Config;

        _Title_Options = new TitleLineOptions(Title_Options ?? (default_override_Title_Options ?? defualt_TitleLineOptions));

        _Help_Options = new ProcessActionHelpInfoOptions(Help_Options ?? (default_override_Help_Options ?? default_ProcessActionHelpInfoOptions));

    }

    /// <summary>
    /// Writes lines starting with the Title then each line. 
    /// </summary>
    /// <param name="lines">The lines to write to console</param>
    void ICxLogService.write_Lines(params string[] lines)
    {
        write_TitleLine();

        WriteLine();
        foreach (var item in lines)
            WriteLine(item);
    }

    void ICxLogService.write_Table()
    {
        write_TitleLine();

        throw new NotImplementedException();
    }

    void ICxLogService.write_ProcessActionHelpInfo(ProcessHelpInfo helpInfo)
    {//string HeaderTitle, string ProcessDescription, ProcessHelpInfo helpInfo, Func<string[]> ExtendInfo_Func = null, Func<string[]> helper_args_Func = null

        WriteLine();
        WriteLine();

        write_TitleLine();
        WriteLine();
        WriteLine();
        WriteLine($"   Description: {_Help_Options.ProcessDescription}");
        var ExtendInfo = _Help_Options.ExtendInfoLines ?? new string[] { };

        if (ExtendInfo.Length > 0)
        {

            WriteLine();

            foreach (var info in ExtendInfo)
            {
                WriteLine($"   -- {info}");
            }
        }
        WriteLine();
        WriteLine();

        WriteLine("   Available Process Commands");
        WriteLine();

        foreach (var proAct in helpInfo.ProcessActions)
        {
            WriteLine($"   >{(proAct.RegisterTypes == CxRegisterTypes.Preview? " [Preview]" : "")} {proAct._Action}: {proAct.Description}");

            if (proAct.ActionArguments?.Length > 0)
                foreach (var item in proAct.ActionArguments)
                {
                    WriteLine($"          {new string(' ', 4)}arg: {item.key} > Use: {item.description}");
                }
            WriteLine();
        }

        helper_args(_Help_Options);

        WriteLine();
        WriteLine();

        if (_Help_Options.display_ShowExamples)
        {
            WriteLine("   Examples: ");
            WriteLine("    > dotnet run -- [process] [action] [[-arg]...]");
            WriteLine("    > [Console-App-exe] [process] [action] [[-arg]...]");
        }


        WriteLine();

        //write_TitleLine(TitleOptions with { isEndLine = true });
    }



    static string indent(int size, char delim = ' ') => new string(' ', size);

    /// <summary>
    /// Write a border delim at the size supplied 
    /// </summary>
    /// <param name="size"></param>
    void write_BorderDelim(int size = 1)
    {
        if (size < 1)
            return;

        //Write the border delim to the size amount
        WriteLine(new string(_Title_Options.BorderDelim, size));
    }

    /// <summary>
    /// Will write the border with the supplied option chain
    /// </summary>
    void write_Border() => write_BorderDelim(_Title_Options.BorderSize);



    void write_TitleLine()
    {

        if (_first_Line_Written)
            return;

        _first_Line_Written = true;

        write_Border();

        bool write_Bottom = false;

        if (_Title_Options.Title.hasCharacters())
        {
            write_Bottom = true;
            write_BorderDelim();
            WriteLine($"{_Title_Options.BorderDelim}{indent(_Title_Options.IndentSize > 0 ? _Title_Options.IndentSize : 5)}{_Title_Options.Title}");            
        }

        if(_Title_Options.ExtraLines?.Length > 0)
        {
            write_Bottom = true;
            write_BorderDelim();
            foreach (var eLine in _Title_Options.ExtraLines)
            {
                WriteLine($"{_Title_Options.BorderDelim}{indent((_Title_Options.IndentSize > 0 ? _Title_Options.IndentSize : 5))} {eLine}");
            }
        }

        if (write_Bottom)
        {
            write_BorderDelim();
            write_Border();
        }

    }

    void helper_args(ProcessActionHelpInfoOptions options)
    {

        if (!options.display_SystemHelperArgs || (!options.implemented_Time_Report && !options.implemented_WriteData2CSVFile && !options.implemented_WriteData2JsonFile))
            return;


        WriteLine();
        WriteLine();

        WriteLine("   Helper args:");
        if (options.implemented_Time_Report)
            WriteLine("     -time-report : Prints out a Time per Action call. The Processes May add to the Time reports for further clarity");
        if (options.implemented_WriteData2JsonFile)
            WriteLine("     -write-jsonFile : Data will be written to the screen and a json file at the specific path. { Note: the Directory must already Exist DefaultName: [process]_[action].[date(MM/dd/yyy)-time(HH_mm_ss)].json");
        if (options.implemented_WriteData2CSVFile)
            WriteLine("     -write-csvFile : Data will be written to the screen and a csv file at the specific path. { Note: the Directory must already Exist DefaultName: [process]_[action].[date(MM/dd/yyy)-time(HH_mm_ss)].csv");
        //,"     "
    }



    /*
     
     

    public static void write_ConsoleStage(string[] lines, Action<TitleLineOptions> TOptions, int lineIndent = 10)
    {
        TitleLineOptions TitleOptions = new TitleLineOptions(TOptions);

        WriteLine();
        WriteLine();

        write_TitleLine(TitleOptions);
        WriteLine();
        WriteLine();

        foreach (var item in lines)
            WriteLine($"{new string(' ', lineIndent)} {(item ?? "")}");

        WriteLine();
        WriteLine();



        //write_TitleLine(TitleOptions with { isEndLine = true });
    }

    public static void write_ConsoleStageTable(DisplayTable table, Action<TitleLineOptions> TOptions, int lineIndent = 10)
    {
        if (table == null)
            throw new FormatException($"Could not build the formtted table because the {nameof(DisplayTable)} object is not valid.");

        write_ConsoleStage(table.writeConsoleTableLines(), TOptions, lineIndent);
    }

     
     */






    public static Action<TitleLineOptions>? default_override_Title_Options { get; internal set; } = null;
    void defualt_TitleLineOptions(TitleLineOptions options)
    {
        //options.Title = "";
        //options.ExtraLines = new string[0];
        //options.BorderSize = WindowWidth > 0 ? WindowWidth : 100;


        var opt = _Config.GetSection(nameof(TitleLineOptions)).Get<TitleLineOptions>();

        //options.isEndLine = false;
        options.Title = opt?.Title ?? "";
        options.ExtraLines = opt?.ExtraLines ?? new string[] { };
        options.BorderSize = opt?.BorderSize ?? (WindowWidth > 0 ? WindowWidth : 150);
        options.IndentSize = opt?.IndentSize > 0 ? opt.IndentSize : 5;

        var delim = _Config.GetSection(nameof(TitleLineOptions))[nameof(TitleLineOptions.BorderDelim)];
        if (delim?.Length == 1 && char.TryParse(delim, out char BorderDelim))
            options.BorderDelim = BorderDelim;


    }

    public static Action<ProcessActionHelpInfoOptions>? default_override_Help_Options { get; internal set; } = null;
    void default_ProcessActionHelpInfoOptions(ProcessActionHelpInfoOptions options)
    {
        var opt = _Config.GetSection("ProcessActionHelpInfoOptions").Get<ProcessActionHelpInfoOptions>();

        options.display_SystemHelperArgs = true;
        options.display_ShowExamples = true;

        options.ProcessDescription = opt?.ProcessDescription ?? "";
        options.ExtendInfoLines = opt?.ExtendInfoLines ?? new string[] { };
    }

    /*
     Cross Platform: 

    public static string Title { get; set; }
    public static void Beep();
    public static void Clear();
    public static bool KeyAvailable { get; }
    public static int CursorLeft { get; set; }
    public static int CursorTop { get; set; }
    public static void SetCursorPosition(int left, int top);
    public static int WindowHeight { get; set; }
    public static int WindowWidth { get; set; }
    public static int WindowLeft { get; set; }
    public static int WindowTop { get; set; }
    
    


    public static int BufferHeight { get; set; }
    public static int BufferWidth { get; set; }
    public static int LargestWindowHeight { get; }
    public static int LargestWindowWidth { get; }


     */
}

//public static class OutputDisplay
//{
//    #region Info Helper Methods

//    //protected void write_StartStopLine() => Console.WriteLine(new string('*', 20));
//    /*  Old Ways 
//    public static void write_TitleLine(string Title, bool isEndLine = false, params string[] ExtraLines)
//    {
//        var lineCount = (Title ?? "").Length <= 0 ? 30 : Title.Length + 10;
//        var eLineCount = ExtraLines.Length > 0 ? ExtraLines.Max(d => d.Length) : 0;
//        //lineCount = lineCount > eLineCount ? lineCount : eLineCount;

//        char delim = '*';
//        WriteLine(new string(delim, lineCount));

//        if (!isEndLine && Title.hasCharacters())
//        {
//            WriteLine($"{delim}");
//            WriteLine($"{delim} {Title}");
//            foreach (var eLine in ExtraLines)
//            {
//                WriteLine($"{delim} {eLine}");
//            }
//            WriteLine($"{delim}");
//            WriteLine(new string(delim, lineCount));
//        }
//    }


//    public static void write_ProcessActionHelpInfo(string HeaderTitle, string ProcessDescription, ProcessHelpInfo helpInfo, Func<string[]> ExtendInfo_Func = null, Func<string[]> helper_args_Func = null)
//    {
//        WriteLine();
//        WriteLine();

//        write_TitleLine(HeaderTitle);
//        WriteLine();
//        WriteLine();
//        WriteLine($"   Description: {ProcessDescription}");
//        WriteLine();
//        WriteLine();

//        WriteLine("   Available Process Actions");
//        WriteLine();

//        foreach (var proAct in helpInfo.ProcessActions)
//        {
//            WriteLine($"   > {proAct._Action}: {proAct.Description}");

//            if (proAct.ActionArguments?.Length > 0)
//                foreach (var item in proAct.ActionArguments)
//                {
//                    WriteLine($"         > {new string(' ', 4)}{item.key} > Info: {item.key}");
//                }

//        }

//        var ExtendInfo = ExtendInfo_Func?.Invoke() ?? new string[] { };

//        if (ExtendInfo.Length > 0)
//        {

//            WriteLine();

//            foreach (var info in ExtendInfo)
//            {
//                WriteLine($"   -- {info}");
//            }
//        }

//        helper_args(helper_args_Func?.Invoke());

//        WriteLine();
//        WriteLine();

//        WriteLine("   Examples: ");
//        WriteLine("    > dotnet run -- [process] [action] [[-arg]...]");
//        WriteLine("    > CxLogic.exe [process] [action] [[-arg]...]");


//        WriteLine();

//        write_TitleLine(HeaderTitle, true);
//    }

//    //*/
//    #endregion

//    static string indent(int size, char delim = ' ') => new string(' ', size);

//    internal static void write_TitleLine(TitleLineOptions options)
//    {

//        WriteLine(new string(options.BorderDelim, options.BorderSize));

//        if (options.Title.hasCharacters())
//        {
//            WriteLine($"{options.BorderDelim}");
//            WriteLine($"{options.BorderDelim}{indent(options.IndentSize > 0 ? options.IndentSize : 5)}{options.Title}");
//            foreach (var eLine in options.ExtraLines)
//            {
//                WriteLine($"{options.BorderDelim}{indent((options.IndentSize > 0 ? options.IndentSize : 5))} {eLine}");
//            }
//            WriteLine($"{options.BorderDelim}");
//            WriteLine(new string(options.BorderDelim, options.BorderSize));
//        }
//    }

//    internal static void helper_args(ProcessActionHelpInfoOptions options)
//    {

//        if (!options.display_SystemHelperArgs || (!options.implemented_Time_Report && !options.implemented_WriteData2CSVFile && !options.implemented_WriteData2JsonFile))
//            return;


//        WriteLine();
//        WriteLine();

//        WriteLine("   Helper args:");
//        if (options.implemented_Time_Report)
//            WriteLine("     -time-report : Prints out a Time per Action call. The Processes May add to the Time reports for further clarity");
//        if (options.implemented_WriteData2JsonFile)
//            WriteLine("     -write-jsonFile : Data will be written to the screen and a json file at the specific path. { Note: the Directory must already Exist DefaultName: [process]_[action].[date(MM/dd/yyy)-time(HH_mm_ss)].json");
//        if (options.implemented_WriteData2CSVFile)
//            WriteLine("     -write-csvFile : Data will be written to the screen and a csv file at the specific path. { Note: the Directory must already Exist DefaultName: [process]_[action].[date(MM/dd/yyy)-time(HH_mm_ss)].csv");
//        //,"     "
//    }

//    internal static void write_ProcessActionHelpInfo(ProcessHelpInfo helpInfo, Action<ProcessActionHelpInfoOptions> POptions, Action<TitleLineOptions> TOptions)
//    {//string HeaderTitle, string ProcessDescription, ProcessHelpInfo helpInfo, Func<string[]> ExtendInfo_Func = null, Func<string[]> helper_args_Func = null

//        ProcessActionHelpInfoOptions ProcessOptions = new ProcessActionHelpInfoOptions(POptions);
//        TitleLineOptions TitleOptions = new TitleLineOptions(TOptions);

//        WriteLine();
//        WriteLine();

//        write_TitleLine(TitleOptions);
//        WriteLine();
//        WriteLine();
//        WriteLine($"   Description: {ProcessOptions.ProcessDescription}");
//        var ExtendInfo = ProcessOptions.ExtendInfoLines ?? new string[] { };

//        if (ExtendInfo.Length > 0)
//        {

//            WriteLine();

//            foreach (var info in ExtendInfo)
//            {
//                WriteLine($"   -- {info}");
//            }
//        }
//        WriteLine();
//        WriteLine();

//        WriteLine("   Available Process Commands");
//        WriteLine();

//        foreach (var proAct in helpInfo.ProcessActions)
//        {
//            WriteLine($"   > {proAct._Action}: {proAct.Description}");

//            if (proAct.ActionArguments?.Length > 0)
//                foreach (var item in proAct.ActionArguments)
//                {
//                    WriteLine($"          {new string(' ', 4)}arg: {item.key} > Use: {item.description}");
//                }
//            WriteLine();
//        }

//        helper_args(ProcessOptions);

//        WriteLine();
//        WriteLine();

//        if (ProcessOptions.display_ShowExamples)
//        {
//            WriteLine("   Examples: ");
//            WriteLine("    > dotnet run -- [process] [action] [[-arg]...]");
//            WriteLine("    > [Console-App-exe] [process] [action] [[-arg]...]");
//        }


//        WriteLine();

//        //write_TitleLine(TitleOptions with { isEndLine = true });
//    }

//    public static void write_ConsoleStage(string[] lines, Action<TitleLineOptions> TOptions, int lineIndent = 10)
//    {
//        TitleLineOptions TitleOptions = new TitleLineOptions(TOptions);

//        WriteLine();
//        WriteLine();

//        write_TitleLine(TitleOptions);
//        WriteLine();
//        WriteLine();

//        foreach (var item in lines)
//            WriteLine($"{new string(' ', lineIndent)} {(item ?? "")}");

//        WriteLine();
//        WriteLine();



//        //write_TitleLine(TitleOptions with { isEndLine = true });
//    }

//    public static void write_ConsoleStageTable(DisplayTable table, Action<TitleLineOptions> TOptions, int lineIndent = 10)
//    {
//        if (table == null)
//            throw new FormatException($"Could not build the formtted table because the {nameof(DisplayTable)} object is not valid.");

//        write_ConsoleStage(table.writeConsoleTableLines(), TOptions, lineIndent);
//    }


//    /// <summary>
//    /// Will Write to the console in using internal methods 
//    /// </summary>
//    /// <param name="CmdServ"></param>
//    //public static void Write_lineToConsole(this Microsoft.Extensions.Logging.ILogger logger)
//    //{
            
//    //}

//    /*


//     async Task ListSites()
//    {
//        /*
//            Needs to get a list of All sites.
//               -c or -clientId [Optional: Not Supplied pulls for all 
//               -s or -status [Optional: Will only return Active State]
//               -t or -type [Optional: Will only return the type: Prod or Test]
//         * /

//        var list = (await _logicFlow.website_Records(siteData.ClientId, (int?)siteData.websiteStatus, (int?)siteData.websiteType))
//            .OrderByDescending(o => o.status).Select(s => new WebsiteItem(s)).ToArray();

//        var TnvItemType = typeof(WebsiteItem);

//        (string propName, string Title)[] props = new[] {
//            ("1000000", "ct"),
//            (nameof(WebsiteItem.itemId), "Id"),
//            (nameof(WebsiteItem.inserted), "Started"),
//            //(nameof(InventoryItem.name), "Name"),
//            (nameof(WebsiteItem.sku), "DNS"),
//            (nameof(WebsiteItem.status), "Status"),
//            (nameof(WebsiteItem.itype), "Type"),
//            (nameof(WebsiteItem.Price), "Price")
//        };

//        List<ListTableHeaderItem> HeaderList = new List<ListTableHeaderItem>();

//        foreach (var item in props)
//        {

//            if (item.Title.Equals("ct"))
//            {
//                HeaderList.Add(new ListTableHeaderItem("", list.Length.ToString().Length, null));
//                continue;
//            }

//            var proType = TnvItemType.GetProperty(item.propName);

//            var ct = list.Length > 0 ? list.Select(s => proType.GetValue(s)?.ToString().Length ?? 0).Max() : 0;

//            HeaderList.Add(new ListTableHeaderItem(item.Title, (item.Title.Length > ct ? item.Title.Length : ct) , proType));
//        }

//        //var test = $"{testVar}{new string('*', 15 - testVar.Length)}";
//        var HeaderLine = HeaderList.Select(s => $"{s.ColumnTitle}{new string(' ', s.MaxValueCount - s.ColumnTitle.Length)}");
//        var headerLine = $"   {string.Join(" | ", HeaderLine)}";

//        TitleLineOptions options = new TitleLineOptions("          Cx Logic List Websites", $"          >> {list.Length} Total Sites")
//        {
//            BorderSize = headerLine.Length
//        };

//        Console.WriteLine("");
//        Console.WriteLine("");

//        write_TitleLine(options);
//        options.isEndLine = true;


//        Console.WriteLine("");
//        Console.WriteLine("");

//        WriteLine($"   {string.Join(" | ", HeaderLine)}");
//        WriteLine($"   {string.Join("---", HeaderLine.Select(s => new string('-', s.Length)))}");

//        if (list.Length > 0)
//        {
//            var ct = 0;
//            foreach (var i in list)
//            {
//                ct++;
//                var lineparts = HeaderList.Select(s => p_title(ct, i, s));// $"{(s.objectPlace.GetValue(i)?.ToString() ?? "")}{new string(' ', s.MaxValueCount - (s.objectPlace.GetValue(i)?.ToString() ?? "").Length)}");
//                var line = $"   {string.Join(" | ", lineparts)}";
//                WriteLine(line);
//            }
//        }
//        Console.WriteLine("");
//        write_TitleLine(options);
//        //write_TitleLine("Cx Logic Websites", true);
//        Console.WriteLine("");
//        Console.WriteLine("");

//        string p_title(int row, WebsiteItem iTtem, ListTableHeaderItem hItm)
//        {
//            if(hItm.ColumnTitle.Equals(""))
//            {
//                return $"{new string(' ', hItm.MaxValueCount - row.ToString().Length)}{row}";
//            }

//            return $"{(hItm.objectPlace.GetValue(iTtem)?.ToString() ?? "")}{new string(' ', hItm.MaxValueCount - (hItm.objectPlace.GetValue(iTtem)?.ToString() ?? "").Length)}";
//        }

//    }




//     */



//}