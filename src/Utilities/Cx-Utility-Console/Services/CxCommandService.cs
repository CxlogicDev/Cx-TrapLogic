namespace CxUtility.Cx_Console;

public sealed record CxCommandService  
{

    public string Process { get; }
    
    public string Command { get; }

    public bool isValid() => Command.hasCharacters() && Process.hasCharacters();

    ///// <summary>
    ///// Will Pull out a Enum Rep of the Process Command is Described
    ///// </summary>
    ///// <typeparam name="CxCommandTypes">The enum to reference</typeparam>
    //public CxCommandTypes CxCommandType<CxCommandTypes>() where CxCommandTypes : struct, IConvertible =>
    //    !typeof(CxCommandTypes).IsEnum ? throw new TypeLoadException("The Loaded Type has to be a Enum") :
    //    (Process.isNumber() ?
    //        (Enum.IsDefined(typeof(CxCommandTypes), Process.Number().Value) ? (CxCommandTypes)Enum.ToObject(typeof(CxCommandTypes), Process.Number().Value) : default) :
    //        (Enum.TryParse(typeof(CxCommandTypes), Process, out var t) ? (CxCommandTypes)t : default));

    internal string[] _args { get; }

    /// <summary>
    /// The Arguments broken down in to Key Value Pairs
    /// </summary>
    internal Dictionary<string, string> _Args = new Dictionary<string, string>();

    public (string arg, string value)[] Args => _Args.Select(s => (s.Key, s.Value)).ToArray();

    public bool getCommandArg(string key, out string? val) => _Args.TryGetValue(key, out val);

    /// <summary>
    /// Will use only for Dev and Testing 
    /// </summary>
    internal System.Diagnostics.Stopwatch watch { get; } = new System.Diagnostics.Stopwatch();

    /// <summary>
    /// 
    /// </summary>
    internal List<(TimeSpan elapedTime, string MainLine, Func<TimeSpan, string[]> extlines)> elpsn = new List<(TimeSpan elapedTime, string MainLine, Func<TimeSpan, string[]> extlines)>();

    //CxProcessTypes
    //CxProcessService
    public CxCommandService(string[] args)
    {
        //CxProcess = args.Length > 0 ? Enum.TryParse<CxProcessTypes>(args[0], true, out var cp) ? cp : default : default;

        _args = args;

        if (args.Length > 0)
        {
            Process = args[0];

            if (args.Length > 1 && !args[1].StartsWith('-'))
            {
                Command = args[1];

                _args = args.Skip(2).ToArray();

            }
            else
                _args = args.Skip(1).ToArray();

            if (_args.Length > 0)
                for (int i = 0; i < _args.Length; i++)
                {
                    if (!_args[i].StartsWith("-"))
                        continue;

                    if (_args.Length == i + 1)
                        _Args.Add(_args[i], "");
                    else if (_args.Length >= i + 1)
                        if (_args[i + 1].StartsWith('-'))
                            _Args.TryAdd(_args[i], "");//Not Value Next set to ""
                        else
                            _Args.TryAdd(_args[i], _args[i + 1]);//Add the Value to the Key
                }

            //return;
        }

        Process = Process ?? string.Empty;
        Command = Command ?? string.Empty;

        //ToDo: Uncomment when Displau is finished 
        //watch.Start();
    }

    bool hasInterface<_interface, _class>(_class myObj)
        where _class : class
    {

        var MyInterfaceType = typeof(_interface);

        var MyObjType = myObj.GetType();

        if (MyInterfaceType.IsInterface && MyObjType.GetInterfaces()?.Length > 0)
            return
                myObj
                .GetType()
                .GetInterfaces().Contains(MyInterfaceType);
        //{
        //MyType
        /*
             //Check From Interface Side
             typeof(IMyInterface).IsAssignableFrom(typeof(MyType))

            //Check from Object Side
            typeof(MyType).GetInterfaces().Contains(typeof(IMyInterface))

        */
        /* 
            //Generic Type Check for interface
            typeof(MyType)
                .GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMyInterface<>))
         */

        // || interfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(_interface<>));
        //.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMyInterface<>));

        //}

        return false;
    }

    /*
    static CxProcessService()
    {

        var MyHostRegistered = typeof(CxProcessService).Assembly.GetTypes().Where(w => w.BaseType == typeof(ConsoleBaseProcess)).ToArray();

        foreach (var item in MyHostRegistered)
        {

            var Att = (CxProcessAttribute)item.GetCustomAttributes(typeof(CxProcessAttribute), false).FirstOrDefault();

            if ((Att?._Register ?? CxRegisterTypes._) == CxRegisterTypes._)
            {
                continue;
            }

            _RegisteredProcesses.Add(Att._CxProcessType, Att);
        }
    }
    //*/

    //internal static Dictionary<CxProcessTypes, string> RegisteredProcesses { get; } = new Dictionary<CxProcessTypes, string>();
    //internal static void registerCxProcess(CxProcessTypes CxProcess, string Description)
    //{
    //    if (Description.hasNoCharacters())
    //        throw new ArgumentException("The Description is required!!");

    //    if (RegisteredProcesses.TryAdd(CxProcess, Description))
    //        RegisteredProcesses[CxProcess] = Description;

    //}



    /// <summary>
    /// The action that is being processed 
    /// </summary>
    //internal string ProcessAction => CxProcess != CxProcessTypes.List ? _args.FirstOrDefault() ?? "" : "";

    /*
    /// <summary>
    /// Will save a Elaped Time for displaying report at the end
    /// </summary>
    /// <param name="time">The Elaped time</param>
    /// <param name="DisplayLine"></param>
    /// <param name="addtionalInfoLines">Any data added After the Time Displays</param>
    public void addTimeReportItem(string DisplayLine, Func<TimeSpan, string[]>? addtionalInfoLines = null) =>
        elpsn.Add((watch.Elapsed, DisplayLine, addtionalInfoLines));
    //"Total Sec: {elapsed} Sec.", "Total Sec: {elapsed} MiliSec."
    //*/

    /* ToDo: Build out the following Logic In adifferent way
    /// <summary>
    /// Write out the report for Time Taken per Elaped Time Item
    /// </summary>
    public void DisplayTimeReportItems()
    {
        if (watch.IsRunning)
            watch.Stop();

        if (this.writeTimeReport())
        {
            string seperatorLine(string Title = "") => $">----------{Title ?? ""}";

            string ExtLineHead = $"{new string(' ', 3)}-- ";
            string LineHead = $"{new string(' ', 3)}>> ";
            WriteLine();
            WriteLine();

            WriteLine($"{new string(' ', 15)}>> Time Item Report <<");
            WriteLine();

            foreach (var item in elpsn)
            {
                WriteLine(seperatorLine(item.MainLine));

                //WriteLine();
                WriteLine($"{LineHead} Total milliSeconds: {item.elapedTime.TotalMilliseconds}");
                WriteLine($"{LineHead} Total Seconds: {item.elapedTime.TotalSeconds}");
                if (item.elapedTime.TotalMinutes > 1)
                    WriteLine($"{LineHead} Total Minutes: {item.elapedTime.TotalMinutes}");


                foreach (var i1 in item.extlines?.Invoke(item.elapedTime) ?? new string[] { })
                {
                    //var info = i1?.Invoke(item.elapedTime);
                    WriteLine($"{ExtLineHead} {i1}");
                }

                WriteLine();
            }


            WriteLine(seperatorLine("Total Time"));
            WriteLine($"{LineHead} Total milliSeconds: {watch.Elapsed.TotalMilliseconds}".Replace(",", "."));
            WriteLine($"{LineHead} Total Seconds: {watch.Elapsed.TotalSeconds}".Replace(",", "."));
            if (watch.Elapsed.TotalMinutes > 1)
                WriteLine($"{LineHead} Total Minutes: {watch.Elapsed.TotalMinutes}".Replace(",", "."));

            WriteLine();
            WriteLine();
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------*/

    /* ToDo: Build out the following Logic In adifferent way
     
    internal enum WriteToFileTypes { json, csv } //LocalDatabase
    internal bool fileSaved { get; private set; }

    /// <summary>
    /// Will Write the content to a file. Will not overwrite a file will save with .[n].{FileType.ext}
    /// </summary>
    /// <param name="fileType">The Type of file to create</param>
    /// <param name="content">The content for the file</param>
    /// <param name="filePath">the path to the director or the full file path with the proper Ext</param>
    /// <param name="Process_Action">The Process Action that is writing the file</param>
    /// <returns>The Full Path of the file that was created.</returns>
    internal string Write_ToFile(WriteToFileTypes fileType, string content, string filePath, string Process_Action)
    {
        try
        {

            var fileExt = fileType switch
            {
                WriteToFileTypes.json => "json",
                WriteToFileTypes.csv => "csv",
                _ => throw new FormatException("The format does not exixt..")
            };

            //var DirectoryExists
            var dir = filePath;
            var isFile = filePath.EndsWith($".{fileExt}", StringComparison.OrdinalIgnoreCase);
            if (isFile)
            {
                var lst = dir.Split(System.IO.Path.DirectorySeparatorChar).Last();

                dir = dir.Replace(lst, "");
            }

            if (Directory.Exists(dir))
            {
                //Can Write the file Possibly
                //[process]_[action].[date(MM/dd/yyy)-time(HH_mm_ss)].json
                var writeTo = isFile ?
                    filePath :
                    $"{Path.TrimEndingDirectorySeparator(dir)}{Path.DirectorySeparatorChar}{Process}_{Process_Action}.{DateTime.Now.ToString("MM-dd-yyyy_HH-mm-ss")}.{fileExt}";

                var exists_writeTo = writeTo;
                if (File.Exists(exists_writeTo))
                {
                    int ct = 1;
                    do
                    {
                        ct++;
                        exists_writeTo = writeTo.Replace($".{fileExt}", $".[{ct}].{fileExt}");

                        if (ct >= 1000)//Protection against infinate Loop.
                            throw new Exception("Could not Create File");

                    }
                    while (File.Exists(exists_writeTo));
                }

                if (write_content_to(content, exists_writeTo))
                    return exists_writeTo;
            }
        }
        catch (Exception Ex)
        {
            //WriteLine($"Error: {Ex.Message}");
            WriteLine($"Full Error: {Ex.ToString()}");
        }

        return null;

        bool write_content_to(string content, string filePath)
        {
            File.WriteAllText(filePath, content, Encoding.UTF8);

            return File.Exists(filePath);
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------------*/
}
