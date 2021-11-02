namespace CxUtility.Cx_Console;

/// <summary>
/// Definds a Process >> Action >> Argument
/// </summary>
[System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class CxConsoleActionArgAttribute : Attribute
{
    /// <summery>
    /// The type of values that a arg can be expressed as shows intent
    /// </summery>
    public enum arg_Types
    {
        ///<summary>String Value</summary>
        Value,
        ///<summary>Number Value: int, decimal, float, etc..</summary>
        Number,
        ///<summary>Boolean Value</summary>
        Switch,
        ///<summary>Enum Type: Does not fall in the intent types. </summary>
        type
    }

    /// <summary>
    /// The Arg Key. Express by -{arg_key} when call in to an action
    /// </summary>
    internal string arg_Key { get; }

    /// <summary>
    /// The arg type to be used
    /// </summary>
    internal arg_Types arg_Type { get; }

    /// <summary>
    /// a short Description of the arg
    /// </summary>
    internal string arg_Description => arg_Descriptions.FirstOrDefault() ?? "";//To Do Allow this to multi lines or Accept Allowed Values

    /// <summary>
    /// Multiple Line description. 
    /// </summary>
    internal List<string> arg_Descriptions { get; } = new();

    /// <summary>
    /// Tell the console Process if the arg should be used or not
    /// </summary>
    internal bool arg_isActive { get; }

    /// <summary>
    /// Tell the processing console that the arg is requred. and display error message if missing. 
    /// </summary>
    internal bool arg_isRequired { get; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="argKey"></param>
    /// <param name="argType"></param>
    /// <param name="argDescription"></param>
    /// <param name="argisActive"></param>
    /// <param name="argisRequired"></param>
    public CxConsoleActionArgAttribute(string argKey, arg_Types argType, string argDescription, bool argisActive, bool argisRequired)
    {
        arg_Key = argKey;
        arg_Type = argType;
        arg_Descriptions.Add(argDescription);
        arg_isActive = argisActive;
        arg_isRequired = argisRequired;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="argKey"></param>
    /// <param name="argType"></param>
    /// <param name="argisActive"></param>
    /// <param name="argisRequired"></param>
    /// <param name="argDescription"></param>
    public CxConsoleActionArgAttribute(string argKey, arg_Types argType, bool argisActive, bool argisRequired, params string[] argDescription)
    {
        arg_Key = argKey;
        arg_Type = argType;
        arg_Descriptions.AddRange(argDescription);
        arg_isActive = argisActive;
        arg_isRequired = argisRequired;
    }
}