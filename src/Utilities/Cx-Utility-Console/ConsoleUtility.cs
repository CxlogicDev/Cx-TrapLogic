
using CxUtility.Cx_Console.DisplayMethods;
using System.Reflection;

namespace CxUtility.Cx_Console;

public static class ConsoleUtility
{
    //public static ProcessHelpInfo addActionHelpInfo(this ProcessHelpInfo HelpInfo, ActionHelpInfo ProcessAction)
    //{
    //    HelpInfo?.addProcessActionHelpInfo(ProcessAction);

    //    return HelpInfo;
    //}

    //public static ActionHelpInfo addArgsHelpInfo(this ActionHelpInfo ActionHelpInfo, ActionArgsHelpInfo ActionArg) => 
    //    ActionHelpInfo.addAction_ArgsHelpInfo(ActionArg);


    internal static CxConsoleInfoAttribute? getInfoAttribute(this ConsoleBaseProcess process) => process
        .GetType()
        .GetCustomAttributes(typeof(CxConsoleInfoAttribute), false)
        .FirstOrDefault() as CxConsoleInfoAttribute;


    internal static CxConsoleActionArgAttribute[] getInfoActionArgs(this MethodInfo me) =>
        me.GetCustomAttributes<CxConsoleActionArgAttribute>(true).ToArray();

    //internal static ProcessHelpInfo pull_InfoDisplay(this ConsoleBaseProcess process)
    //{

    //    var stack = process.getInfoAttribute();

    //    //stack.ProcessCallName
    //    //stack.Description
    //    //stack.RegisterType
    //    //stack.ProcessActions



    //    if (stack != null)
    //    {
    //        var p = new ProcessHelpInfo();

    //        if (stack.ProcessActions.Count > 0)
    //            foreach (var item in stack.ProcessActions)
    //            {
    //                p.addActionHelpInfo(new ProcessActionHelpInfo())
    //                    foreach (var i in item.Value)
    //                {

    //                }
    //            }
    //    }

    //    return null;
    //}

    internal static bool isHelpCall(this CxCommandService ProcessServ) => ProcessServ.Process.hasNoCharacters() || //Command.hasNoCharacters();
       ProcessServ._Args.Keys.Any(a => a.EndsWith("-h", StringComparison.OrdinalIgnoreCase) || a.EndsWith("-help", StringComparison.OrdinalIgnoreCase));

    /* ToDO: Reconfigure how Time Report works
    /// <summary>
    /// Tells the system if it can write a Time report for the running process 
    /// </summary>
    /// <param name="ProcessServ"></param>
    internal static bool writeTimeReport(this CxCommandService ProcessServ) =>
        !ProcessServ.isHelpCall() && ProcessServ._Args.ContainsKey("-time-report");
    //------------------------------------------------------------------------------------------------------*/
}

