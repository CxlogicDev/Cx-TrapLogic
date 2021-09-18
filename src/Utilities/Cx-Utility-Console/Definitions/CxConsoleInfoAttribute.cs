using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxUtility.Cx_Console;

/// <summary>
/// Registers a Console Process
/// This Is required to register the Process App Logic. 
/// </summary>
[System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class CxConsoleInfoAttribute : Attribute
{

    /// <summary>
    /// The Type of the Process that handling Requests
    /// </summary>
    public Type processType { get; }

    /// <summary>
    /// The Name of the process Call. Must be Unique. Ex: website, transaction or invoice
    /// </summary>
    public string processCallName { get; }

    /// <summary>
    /// The Type of Registartion for Application
    /// </summary>
    public CxRegisterTypes registerType { get; }

    /// <summary>
    /// Keys are the Action Method Name
    /// Values are the Display Info
    /// </summary>
    internal Dictionary<string, (CxConsoleActionAttribute _attribute, System.Reflection.MethodInfo method)> ProcessActions { get; }

    /// <summary>
    /// The Description Displayed out in the Console Processs Info List
    /// </summary>
    public string description { get; }

    /// <summary>
    /// The Process Attribute need 
    /// </summary>
    /// <param name="ProcessCallName">The Name of the process Call. Must be Unique. Ex: website, transaction or invoice</param>
    /// <param name="ProcessType">The Type of the Process that handling Requests</param>
    /// <param name="RegisterType">The Type of Registartion for Application</param>
    /// <param name="Description">The Description Displayed out in the Console Processs Info List</param>
    public CxConsoleInfoAttribute(string ProcessCallName, Type ProcessType, CxRegisterTypes RegisterType, string Description)
    {
        //Check Type Throw Error if not The correct Type

        if (ProcessType.IsAssignableTo(typeof(ConsoleBaseProcess)))
        {

            processCallName = ProcessCallName;
            //
            processType = ProcessType;

            //
            registerType = RegisterType;

            //
            description = Description;

            //
            ProcessActions = ProcessType.GetMethods()
                .Where(w => w.GetCustomAttributes(typeof(CxConsoleActionAttribute), true)?.Length > 0)
                .Select(s => (method: s, _act: (CxConsoleActionAttribute)s.GetCustomAttributes(typeof(CxConsoleActionAttribute), true).FirstOrDefault()))
                .ToDictionary(k => k._act.name, v => (v._act, v.method));
            //.Select(s => (CxConsoleActionAttribute)s.GetCustomAttributes(typeof(CxConsoleActionAttribute), true).FirstOrDefault())
            //.ToArray();



            //foreach(string a in _act.Select(s => s.ActionName.ToLower()).Distinct())
            //{
            //    var isGood = ProcessActions.TryAdd(a.ToLower(), _act.Where(w => w.ActionName.Equals(a, StringComparison.OrdinalIgnoreCase)).ToList());
            //    //Run through the List if Arguments if cannot load the action with args
            //    if (!isGood)
            //        throw new FormatException($" {nameof(CxConsoleActionAttribute)} not set up correctly");
            //}

            return;
        }

        //throw new ArgumentException($"The Class {ProcessType.Name} must be a base type of {typeof(ConsoleBaseProcess)}!!!!");
    }
}

