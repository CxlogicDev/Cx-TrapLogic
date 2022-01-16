using Azure;
using CxAzure.BlobStorage;
using CxAzure.TableStorage;
using CxUtility.Cx_Console;

namespace Cx_TrapTestConsole
{
    [CxConsoleInfo("azure-storage", typeof(Cx_Azure_Test), CxRegisterTypes.Preview, "Live Inplace Testing Ground for Projects")]
    internal class Cx_Azure_Test : ConsoleBaseProcess
    {
        public const string testStorageTable = "xxcxtrapxxtest";

        TableAccess tableAccess { get; }
        BlobAccess blobAccess { get; }

        public Cx_Azure_Test(TableAccess _tableAccess, BlobAccess _blobAccess, CxCommandService CxProcess, IConfiguration config) :
            base(CxProcess, config)
        {
            tableAccess = _tableAccess;
            blobAccess = _blobAccess;
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

        public const string testBlobContainer = "xxcxtrapxxtestxx";

        [CxConsoleAction("blob-delete", "Test container delete > storage Account", true, CxRegisterTypes.Register)]
        public async Task Test_Delete_blob(CancellationToken cancellationToken)
        {
           var isDelete =  await blobAccess.Delete_blobContainer_Async(testBlobContainer, cancellationToken);//testBlobContainer

            WriteOutput_Service.write_Lines(3, $"Container: {testBlobContainer} statue: {(isDelete ? "Deleted" : "No Action")}");
        }

        [CxConsoleAction("blob", "Test Table storage Account", true, CxRegisterTypes.Register)]
        public async Task Test_blobs(CancellationToken cancellationToken)
        {

            //Create Container
            await blobAccess.Create_blobContainer_Async(cancellationToken: cancellationToken);

            WriteOutput_Service.write_Lines(3, "Blob Container Created");

            var filePath = "Some-File.txt";

            if (File.Exists(filePath))
                File.Delete(filePath);

            await File.AppendAllLinesAsync(filePath, new[] {

                    $"Date To Process: {DateTime.Now.ToString("MMM-dd-yyyy")}",
                    "I am a test File",
                    "Azure Blob Testing",
                    $"save Me in azure in the directory /.app_data/"

                }, cancellationToken);


            WriteOutput_Service.write_Lines(3,
                $"File: {filePath} created at {Directory.GetCurrentDirectory().TrimEnd(Path.DirectorySeparatorChar)}{Path.DirectorySeparatorChar}{filePath}"
            );

            //Upload
            await blobAccess.Upload_Blob_Delete_If_Exists_Async(filePath, blob_directory: ".app_data/", cancellationToken: cancellationToken);

            WriteOutput_Service.write_Lines(3, "blob uploaded");

            //list  
            var lst_Blobs = await blobAccess.List_Blob_Async();

            WriteOutput_Service.write_Lines(3, lst_Blobs.Select(s => s.Name).ToArray());

            var pDir = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}.app_data";
            //if (Directory.Exists(pDir))
            //    Directory.Delete(Directory.GetCurrentDirectory());            

            foreach (var blob in lst_Blobs)
            {

                var localPath = $"{pDir}{Path.DirectorySeparatorChar}{filePath}";
                if(File.Exists(localPath))
                    File.Delete(localPath);

                //$".app_data/{filePath}"
                //Download
                var downloadBlob = await blobAccess.Download_Blob_Async(localPath, blob.Name, cancellationToken: cancellationToken);

                //WriteOutput_Service.write_Lines(3, $"Download status: {downloadBlob}");

                //deleted
                var blob_Deleted = await blobAccess.Delete_Blob_Async($".app_data/{filePath}", cancellationToken: cancellationToken);

                WriteOutput_Service.write_Lines(3, $"Blob Deleted: {blob_Deleted}");
            }
        }


    }
}
