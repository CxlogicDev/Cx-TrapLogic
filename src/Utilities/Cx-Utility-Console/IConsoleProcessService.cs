namespace CxUtility.Cx_Console;

public interface IConsoleProcessService
{
    /// <summary>
    /// Displays Info for the Processe >> Actions to the console output
    /// </summary>
    Task Info();    

    /// <summary>
    /// The Process the Action/Command.
    /// </summary>
    /// <param name="cancellationToken">The Token to Cancel the program</param>
    Task ProcessAction(CancellationToken cancellationToken);
}
