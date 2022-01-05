using static CxUtility.Cx_Console.CxConsoleActionArgAttribute;

namespace CxUtility.Cx_Console.DisplayMethods;

/// <summary>
/// The Argument for the Action
/// Key: the Args will always need to start with -arg Ex: -id usr-01
/// description: just decribes the Arguments meaning
/// </summary>
public record CommandArgsHelpInfo(string key, string description, arg_Types arg_Type = arg_Types.Value)
{
    public bool isRequired { get; init; }
};