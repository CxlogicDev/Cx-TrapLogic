using CxAzure.TableStorage;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CxUtility.Cx_Console;
using Azure;

namespace Cx_TrapTestConsole
{
    [CxConsoleInfo("azure-storage", typeof(Cx_Azure), CxRegisterTypes.Preview, "Live Inplace Testing Ground for Projects")]
    internal class Cx_Azure : ConsoleBaseProcess
    {

        public const string TestStorageTable_constr = nameof(TestStorageTable_constr);

        public const string testStorageTable = "xxcxtrapxxtest";

        TableAccess tableAccess { get; }

        public Cx_Azure(TableAccess _tableAccess, CxCommandService CxProcess, IConfiguration config) :
            base(CxProcess, config)
        {
            tableAccess = _tableAccess;
        }

        [CxConsoleAction("table", "Test Table storage Account", true, CxRegisterTypes.Register)]
        public async Task test_StorageTables(CancellationToken cancellationToken)
        {
            const string PartitionKey = "Cx-Trap";

            //Create a Table
            await tableAccess
                .Create_StorageTable_Async(cancellationToken: cancellationToken);

            //Look up table
            var query_Table_Result = await tableAccess.Query_StorageTables_Async();

            query_Table_Result
                .ErrorIfNull_Or_NotValid(val => val.Count > 0 && val.FirstOrDefault().ErrorIfNull().Name.Equals(testStorageTable));

            WriteOutput_Service.write_Lines(3, "Table Storage Created...Pass");

            var rowKey = Guid.NewGuid().ToString();

            //Add Entity
            testEntity? entity_Result = await tableAccess.Create_Entity_IfNotExists_Async(new testEntity { PartitionKey = PartitionKey, RowKey = rowKey });

            _ = entity_Result.ErrorIfNull();

            WriteOutput_Service.write_Lines(3, "Table Storage Entity Creating");

            IEnumerable<testEntity> query_Entity_Results = (await tableAccess.Query_Entity_ByPartionKey_Async<testEntity>(PartitionKey)).ErrorIfNull();

            testEntity query_Entity_Result = (await tableAccess.Query_Entity_ByRowKey_Async<testEntity>(PartitionKey, rowKey)).ErrorIfNull();

            WriteOutput_Service.write_Lines(3, "Table Storage Entity Created...Pass");

            //Delete entity 
            await tableAccess.Delete_Entity(PartitionKey, new[] { rowKey });

            //Delete Table
            if ((await tableAccess.Query_Entity_ByRowKey_Async<testEntity>(PartitionKey, rowKey)).isNotNull())
                throw new InvalidOperationException("The Table entity was not deleted");

            WriteOutput_Service.write_Lines(3, "Table Storage Entity Deleted...");
            
            await tableAccess.Delete_StorageTable_Async(testStorageTable);

            (await tableAccess.Query_StorageTables_Async())
                .Error_IfNotValid(val => val.isNull() || (val.Count == 0));

            WriteOutput_Service.write_Lines(3, "Table Storage Processing... Pass");
        }

        public record testEntity : Azure.Data.Tables.ITableEntity
        {
            [System.Diagnostics.CodeAnalysis.AllowNull]
            public string PartitionKey { get; set; }
            [System.Diagnostics.CodeAnalysis.AllowNull]
            public string RowKey { get; set; }
            public DateTimeOffset? Timestamp { get; set; }
            public ETag ETag { get; set; }
        }
    }
}
