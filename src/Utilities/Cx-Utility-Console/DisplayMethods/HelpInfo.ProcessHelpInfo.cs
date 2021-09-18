using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxUtility.Cx_Console.DisplayMethods;

/// <summary>
/// Top Level display Item for Dispalying Help Info
/// </summary>
public record ProcessHelpInfo
{
    public ProcessHelpInfo()
    {       
    }

    public ProcessHelpInfo(IEnumerable<CommandHelpInfo> ProcessActions) 
    {
        if (ProcessActions?.Count() > 0)
            addProcessActionHelpInfo(ProcessActions.ToArray());
    }

    public bool canDisplay() => _ProcessActions.Count > 0;

    Dictionary<string, CommandHelpInfo> _ProcessActions { get; } = new();

    /// <summary>
    /// Show entered ActionsInfo
    /// </summary>
    public CommandHelpInfo[] ProcessActions => _ProcessActions.Values.ToArray();

    /// <summary>
    /// Adds Process Action HelpInfo to the process Help 
    /// </summary>
    /// <param name="ProcessActions">List of ActionInfo to add</param>
    public ProcessHelpInfo addProcessActionHelpInfo(params CommandHelpInfo[] ProcessActions)
    {
        if (!(ProcessActions?.Length > 0))
            return this;

        //Adds in a new Process Action as long 
        if (ProcessActions.Length > 0)
            foreach (var i in ProcessActions)
                _ProcessActions[i._Action] = i;

        return this;
    }
}