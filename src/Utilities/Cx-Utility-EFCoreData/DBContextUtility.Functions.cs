using Microsoft.EntityFrameworkCore;

namespace CxUtility.EFCoreData;

public static partial class DBContextUtility
{

    /// <summary>
    /// Test to see if the Database object Exists
    /// </summary>
    /// <param name="context">The Context to check for a object</param>
    /// <param name="FullObjectName">The Full name Ex: {Schema}.{ObjectName}</param>
    public static bool ObjectExists(this DbContext context, string FullObjectName)
    {
        string? id = null;

        try
        {
            id = context
                    .BuildScriptCommand($"select OBJECT_ID('{FullObjectName}') as id")
                    .Exec_StoredProc<string>()
                    .FirstOrDefault();
        }
        catch (Exception)
        {
        }

        return id.hasCharacters();
    }
}
