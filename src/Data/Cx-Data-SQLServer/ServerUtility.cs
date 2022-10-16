global using CxUtility.EFCoreData;
using Microsoft.Extensions.Logging;

namespace CxData.SQLServer
{
    public static class ServerUtility
    {
        internal static ILogger? logger { get; private set; }

        /// <summary>
        /// Will attach a logger to the Utility
        /// </summary>
        /// <param name="_logger">The Logger to Attach</param>
        /// <exception cref="ArgumentNullException">The Exception if the logger is null</exception>
        public static void AttchLogger(ILogger _logger) => 
            logger = _logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// The Database Object and schema to test for.
        /// </summary>
        /// <param name="context">The Db Context to process on</param>
        /// <param name="ObjectName">The Database objectTo test for</param>
        /// <param name="Schema">The Schema To test on Default: dbo</param>
        public static bool DatabaseHasObject(this Microsoft.EntityFrameworkCore.DbContext context, string ObjectName, string Schema = "dbo")
        {
            string _schema = Schema
                .ErrorIfNull_Or_NotValid(v => v.hasCharacters(), new ArgumentException("[D9FB758F-2941-4DE1-9A15-2E9A4B31B213]", nameof(Schema)), new ArgumentNullException(nameof(Schema)))
                .Trim('[',']');

            string _ObjectName = ObjectName
                .ErrorIfNull_Or_NotValid(v => v.hasCharacters(), new ArgumentException("[11107AA7-CF41-4A89-9EA2-8CB9A41EF853]", nameof(ObjectName)), new ArgumentNullException(nameof(Schema)))
                .Trim('[', ']');

            return context
                .DatabaseHasObject($"[{_schema}].{_ObjectName}");
        }

        /// <summary>
        /// The Database Object to test for
        /// </summary>
        /// <param name="context">The Db Context to process on</param>
        /// <param name="FullObjectName">The Name of the object to look for. Format : schema.object</param>       
        public static bool DatabaseHasObject(this Microsoft.EntityFrameworkCore.DbContext context, string FullObjectName)
        {
            int? id = null;

            try
            {
                var query = $"select OBJECT_ID('{FullObjectName}') as id";

                id = context
                        .BuildScriptCommand(query)
                        .Exec_CommandSimple<int>()
                        .FirstOrDefault();
            }
            catch (Exception Ex)
            {

                const string errId = "0D06E2C8-05AF-4FE3-A770-744D825AD35E";
                logger?.LogError(Ex, $"[{errId}] >> {Ex.Message}");
                throw;
            }

            return id > 0;
        }
    }
}
