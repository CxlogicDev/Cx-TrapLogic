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
    internal string arg_Description { get; }//To Do Allow this to multi lines or Accept Allowed Values

    /// <summary>
    /// Tell the console Process if the arg should be used or not
    /// </summary>
    internal bool arg_isActive { get; }

    /// <summary>
    /// Tell the processing console that the arg is requred. and display error message if missing. 
    /// </summary>
    internal bool arg_isRequired { get; }


    public CxConsoleActionArgAttribute(string argKey, arg_Types argType, string argDescription, bool argisActive, bool argisRequired)
    {
        arg_Key = argKey;
        arg_Type = argType;
        arg_Description = argDescription;
        arg_isActive = argisActive;
        arg_isRequired = argisRequired;
    }
}