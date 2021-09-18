global using static System.Console;

namespace CxUtility.Cx_Console.DisplayMethods;

public static class OutputDisplay
{
    #region Info Helper Methods

    //protected void write_StartStopLine() => Console.WriteLine(new string('*', 20));
    /*  Old Ways 
    public static void write_TitleLine(string Title, bool isEndLine = false, params string[] ExtraLines)
    {
        var lineCount = (Title ?? "").Length <= 0 ? 30 : Title.Length + 10;
        var eLineCount = ExtraLines.Length > 0 ? ExtraLines.Max(d => d.Length) : 0;
        //lineCount = lineCount > eLineCount ? lineCount : eLineCount;

        char delim = '*';
        WriteLine(new string(delim, lineCount));

        if (!isEndLine && Title.hasCharacters())
        {
            WriteLine($"{delim}");
            WriteLine($"{delim} {Title}");
            foreach (var eLine in ExtraLines)
            {
                WriteLine($"{delim} {eLine}");
            }
            WriteLine($"{delim}");
            WriteLine(new string(delim, lineCount));
        }
    }


    public static void write_ProcessActionHelpInfo(string HeaderTitle, string ProcessDescription, ProcessHelpInfo helpInfo, Func<string[]> ExtendInfo_Func = null, Func<string[]> helper_args_Func = null)
    {
        WriteLine();
        WriteLine();

        write_TitleLine(HeaderTitle);
        WriteLine();
        WriteLine();
        WriteLine($"   Description: {ProcessDescription}");
        WriteLine();
        WriteLine();

        WriteLine("   Available Process Actions");
        WriteLine();

        foreach (var proAct in helpInfo.ProcessActions)
        {
            WriteLine($"   > {proAct._Action}: {proAct.Description}");

            if (proAct.ActionArguments?.Length > 0)
                foreach (var item in proAct.ActionArguments)
                {
                    WriteLine($"         > {new string(' ', 4)}{item.key} > Info: {item.key}");
                }

        }

        var ExtendInfo = ExtendInfo_Func?.Invoke() ?? new string[] { };

        if (ExtendInfo.Length > 0)
        {

            WriteLine();

            foreach (var info in ExtendInfo)
            {
                WriteLine($"   -- {info}");
            }
        }

        helper_args(helper_args_Func?.Invoke());

        WriteLine();
        WriteLine();

        WriteLine("   Examples: ");
        WriteLine("    > dotnet run -- [process] [action] [[-arg]...]");
        WriteLine("    > CxLogic.exe [process] [action] [[-arg]...]");


        WriteLine();

        write_TitleLine(HeaderTitle, true);
    }

    //*/
    #endregion

    static string indent(int size, char delim = ' ') => new string(' ', size);

    internal static void write_TitleLine(TitleLineOptions options)
    {

        WriteLine(new string(options.BorderDelim, options.BorderSize));

        if (!options.isEndLine && options.Title.hasCharacters())
        {
            WriteLine($"{options.BorderDelim}");
            WriteLine($"{options.BorderDelim}{indent(options.IndentSize > 0 ? options.IndentSize : 5)}{options.Title}");
            foreach (var eLine in options.ExtraLines)
            {
                WriteLine($"{options.BorderDelim}{indent((options.IndentSize > 0 ? options.IndentSize : 5))} {eLine}");
            }
            WriteLine($"{options.BorderDelim}");
            WriteLine(new string(options.BorderDelim, options.BorderSize));
        }
    }

    internal static void helper_args(ProcessActionHelpInfoOptions options)
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

    internal static void write_ProcessActionHelpInfo(ProcessHelpInfo helpInfo, Action<ProcessActionHelpInfoOptions> POptions, Action<TitleLineOptions> TOptions)
    {//string HeaderTitle, string ProcessDescription, ProcessHelpInfo helpInfo, Func<string[]> ExtendInfo_Func = null, Func<string[]> helper_args_Func = null

        ProcessActionHelpInfoOptions ProcessOptions = new ProcessActionHelpInfoOptions(POptions);
        TitleLineOptions TitleOptions = new TitleLineOptions(TOptions);

        WriteLine();
        WriteLine();

        write_TitleLine(TitleOptions);
        WriteLine();
        WriteLine();
        WriteLine($"   Description: {ProcessOptions.ProcessDescription}");
        var ExtendInfo = ProcessOptions.ExtendInfoLines ?? new string[] { };

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
            WriteLine($"   > {proAct._Action}: {proAct.Description}");

            if (proAct.ActionArguments?.Length > 0)
                foreach (var item in proAct.ActionArguments)
                {
                    WriteLine($"          {new string(' ', 4)}arg: {item.key} > Use: {item.description}");
                }
            WriteLine();
        }

        helper_args(ProcessOptions);

        WriteLine();
        WriteLine();

        if (ProcessOptions.display_ShowExamples)
        {
            WriteLine("   Examples: ");
            WriteLine("    > dotnet run -- [process] [action] [[-arg]...]");
            WriteLine("    > [Console-App-exe] [process] [action] [[-arg]...]");
        }


        WriteLine();

        write_TitleLine(TitleOptions with { isEndLine = true });
    }

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



        write_TitleLine(TitleOptions with { isEndLine = true });
    }

    public static void write_ConsoleStageTable(DisplayTable table, Action<TitleLineOptions> TOptions, int lineIndent = 10)
    {
        if (table == null)
            throw new FormatException($"Could not build the formtted table because the {nameof(DisplayTable)} object is not valid.");

        write_ConsoleStage(table.writeConsoleTableLines(), TOptions, lineIndent);
    }


    /*


     async Task ListSites()
    {
        /*
            Needs to get a list of All sites.
               -c or -clientId [Optional: Not Supplied pulls for all 
               -s or -status [Optional: Will only return Active State]
               -t or -type [Optional: Will only return the type: Prod or Test]
         * /

        var list = (await _logicFlow.website_Records(siteData.ClientId, (int?)siteData.websiteStatus, (int?)siteData.websiteType))
            .OrderByDescending(o => o.status).Select(s => new WebsiteItem(s)).ToArray();

        var TnvItemType = typeof(WebsiteItem);

        (string propName, string Title)[] props = new[] {
            ("1000000", "ct"),
            (nameof(WebsiteItem.itemId), "Id"),
            (nameof(WebsiteItem.inserted), "Started"),
            //(nameof(InventoryItem.name), "Name"),
            (nameof(WebsiteItem.sku), "DNS"),
            (nameof(WebsiteItem.status), "Status"),
            (nameof(WebsiteItem.itype), "Type"),
            (nameof(WebsiteItem.Price), "Price")
        };

        List<ListTableHeaderItem> HeaderList = new List<ListTableHeaderItem>();

        foreach (var item in props)
        {

            if (item.Title.Equals("ct"))
            {
                HeaderList.Add(new ListTableHeaderItem("", list.Length.ToString().Length, null));
                continue;
            }

            var proType = TnvItemType.GetProperty(item.propName);

            var ct = list.Length > 0 ? list.Select(s => proType.GetValue(s)?.ToString().Length ?? 0).Max() : 0;

            HeaderList.Add(new ListTableHeaderItem(item.Title, (item.Title.Length > ct ? item.Title.Length : ct) , proType));
        }

        //var test = $"{testVar}{new string('*', 15 - testVar.Length)}";
        var HeaderLine = HeaderList.Select(s => $"{s.ColumnTitle}{new string(' ', s.MaxValueCount - s.ColumnTitle.Length)}");
        var headerLine = $"   {string.Join(" | ", HeaderLine)}";

        TitleLineOptions options = new TitleLineOptions("          Cx Logic List Websites", $"          >> {list.Length} Total Sites")
        {
            BorderSize = headerLine.Length
        };

        Console.WriteLine("");
        Console.WriteLine("");

        write_TitleLine(options);
        options.isEndLine = true;


        Console.WriteLine("");
        Console.WriteLine("");

        WriteLine($"   {string.Join(" | ", HeaderLine)}");
        WriteLine($"   {string.Join("---", HeaderLine.Select(s => new string('-', s.Length)))}");

        if (list.Length > 0)
        {
            var ct = 0;
            foreach (var i in list)
            {
                ct++;
                var lineparts = HeaderList.Select(s => p_title(ct, i, s));// $"{(s.objectPlace.GetValue(i)?.ToString() ?? "")}{new string(' ', s.MaxValueCount - (s.objectPlace.GetValue(i)?.ToString() ?? "").Length)}");
                var line = $"   {string.Join(" | ", lineparts)}";
                WriteLine(line);
            }
        }
        Console.WriteLine("");
        write_TitleLine(options);
        //write_TitleLine("Cx Logic Websites", true);
        Console.WriteLine("");
        Console.WriteLine("");

        string p_title(int row, WebsiteItem iTtem, ListTableHeaderItem hItm)
        {
            if(hItm.ColumnTitle.Equals(""))
            {
                return $"{new string(' ', hItm.MaxValueCount - row.ToString().Length)}{row}";
            }

            return $"{(hItm.objectPlace.GetValue(iTtem)?.ToString() ?? "")}{new string(' ', hItm.MaxValueCount - (hItm.objectPlace.GetValue(iTtem)?.ToString() ?? "").Length)}";
        }

    }




     */



}