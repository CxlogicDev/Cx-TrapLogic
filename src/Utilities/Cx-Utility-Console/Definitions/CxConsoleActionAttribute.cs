namespace CxUtility.Cx_Console;

/// <summery>
/// Describes a Process >> Action. 
/// </summery>
[System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class CxConsoleActionAttribute : System.Attribute
{

    /// <summery>
    /// The command that is being call in the process
    /// </summery>
    public string name { get; }


    /// <summary>
    /// A description of the command being called
    /// </summary>
    internal string description { get; }

    /// <summary>
    /// Tells the Console Process if the Action is active and can be called 
    /// </summary>
    internal bool isActive { get; }

    /// <summary>
    /// Tells the Console process what the Registed type of the action is. 
    /// </summary>
    internal CxRegisterTypes registerType { get; }

    /// <summary>
    /// Entry to the CxConsoleActionAttribute
    /// </summary>
    /// <param name="Name">The command that is being call in the process</param>
    /// <param name="Description">A description of the command being called</param>
    /// <param name="IsActive">Tells the Console Process if the Action is active and can be called </param>
    /// <param name="RegisterType">Tells the Console process what the Registed type of the action is. </param>
    public CxConsoleActionAttribute(string Name, string Description, bool IsActive, CxRegisterTypes RegisterType)
    {
        name = Name;
        description = Description;
        //isActive = IsActive;
        registerType = RegisterType;
    }
}
