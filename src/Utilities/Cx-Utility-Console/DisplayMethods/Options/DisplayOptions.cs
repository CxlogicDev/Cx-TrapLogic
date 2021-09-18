namespace CxUtility.Cx_Console.DisplayMethods;

public record TitleLineOptions
{
    public string Title { get; set; }
    public bool isEndLine { get; set; } = false;
    public string[] ExtraLines { get; set; } = new string[] { };
    public int BorderSize { get; set; } = 100;// => ExtraLines.Max(d => d.Length)
    public int IndentSize { get; set; } = 5;
    public char BorderDelim { get; set; } = '*';

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

    public TitleLineOptions(string title, params string[] extraLines)
    {
        Title = title;
        isEndLine = false;
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
