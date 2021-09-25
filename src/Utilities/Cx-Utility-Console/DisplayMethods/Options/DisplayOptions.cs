using System.Text;

namespace CxUtility.Cx_Console.DisplayMethods;

public record TitleLineOptions
{
    public string Title { get; set; }
    //public bool isEndLine { get; set; } = false;
    public string[] ExtraLines { get; set; } = new string[] { };
    public int BorderSize { get; set; } = 100;// => ExtraLines.Max(d => d.Length)
    public int IndentSize { get; set; } = 5;
    public char BorderDelim { get; set; } = '*';

    public TitleLineOptions()
    {
        Title = Title ?? string.Empty;
    }

    public TitleLineOptions(Action<TitleLineOptions> setOptions)
    {
        setOptions.Invoke(this);

        Title = Title ?? string.Empty;

        //checks what was set
        validate();
    }

    void validate()
    {
        var tlct = Title.Length <= 0 ? 30 : Title.Length + 10;
        var telct = ExtraLines.Length > 0 ? ExtraLines.Max(d => d.Length) : 0;
        BorderSize = BorderSize > tlct ? BorderSize : tlct;
        BorderSize = BorderSize > telct ? BorderSize : telct;
    }

    public TitleLineOptions Append_CallingInfo(string process, string command, string[] args, bool isPreview = false, bool isPreviewCommand = false)
    {
        StringBuilder sb = new StringBuilder();
        if (process.hasCharacters())
        {
            List<string> lst = ExtraLines.ToList();
            lst.Add($"---> {(isPreview ? "[Preview >> Process]" : "Process")}: {process}");

            if (command.hasCharacters())
            {
                lst.Add($"---> {(isPreviewCommand ? "[Preview >> Command]" : "Command")}: {command}");

                if (args?.Length > 0)
                    lst.Add($"---> Args: {string.Join(" ", args)}");
            }

            ExtraLines = lst.ToArray();
        }

        ExtraLines = ExtraLines.Append(sb.ToString()).ToArray();

        return this;
    }

    public TitleLineOptions Append_ExtraLines(params string[] extraLines)
    {
        if (extraLines?.Length > 0)
        {
            List<string> lst = ExtraLines.ToList();
            lst.AddRange(extraLines);
            ExtraLines = lst.ToArray();
        }

        return this;
    }

    public TitleLineOptions(string title, params string[] extraLines)
    {
        Title = title;
        //isEndLine = false;
        ExtraLines = extraLines;
        validate();

    }

    //public TitleLineOptions()
    //{
    //    validate();
    //}
}

public record ProcessActionHelpInfoOptions
{
    public ProcessActionHelpInfoOptions()
    {
        ProcessDescription = ProcessDescription ?? string.Empty;

        validate();
    }

    public ProcessActionHelpInfoOptions(Action<ProcessActionHelpInfoOptions> setOptions)
    {
        setOptions?.Invoke(this);

        ProcessDescription = ProcessDescription ?? string.Empty;

        //checks what was set
        validate();
    }


    public string ProcessDescription { get; set; }

    public string[]? ExtendInfoLines { get; set; }

    public bool display_SystemHelperArgs { get; set; } = true;

    public bool display_ShowExamples { get; set; } = true;

    public bool implemented_Time_Report { get; set; } = false;

    public bool implemented_WriteData2JsonFile { get; set; } = false;

    public bool implemented_WriteData2CSVFile { get; set; } = false;

    void validate()
    {
        
    }

}
