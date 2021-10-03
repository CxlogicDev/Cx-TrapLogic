namespace CxUtility.Cx_Console.DisplayMethods;

/// <summary>
/// Info Display for the Process actions
/// </summary>
public record CommandHelpInfo(string _Action, string Description, CxRegisterTypes RegisterTypes)
{
    /// <summary>
    /// The Help display for each Arg on the Action. Can be updated internaly 
    /// </summary>
    Dictionary<string, CommandArgsHelpInfo> _ActionArguments { get; } = new();

    /// <summary>
    /// All Arguments for the process Action
    /// </summary>
    public CommandArgsHelpInfo[] ActionArguments => _ActionArguments.Values.ToArray();

    /// <summary>
    /// Adds the Arg help Info the Action Help Display
    /// </summary>
    /// <param name="ActionArgs"></param>
    public CommandHelpInfo addAction_ArgsHelpInfo(params CommandArgsHelpInfo[] ActionArgs)
    {
        if (ActionArgs == null || ActionArgs.Length <= 0)
            return this;

        foreach (var i in ActionArgs)
            _ActionArguments[i.key] = i;

        return this;
    }
}

