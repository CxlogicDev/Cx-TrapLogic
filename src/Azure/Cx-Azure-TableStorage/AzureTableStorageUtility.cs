

using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;

namespace CxAzure.TableStorage;


public static class TableStorageUtilityExtentions
{
    /// <summary>
    /// Creates a New Table in azure table storage
    /// </summary>
    /// <param name="tableAccess">The Table Access</param>
    /// <param name="tableName">create a different table then the TableAccess.tableName</param>
    public static async Task<TableAccess> Create_StorageTable_Async(this TableAccess tableAccess, string? tableName = null, CancellationToken cancellationToken = default)
    {
        string _tableName = tableName ?? tableAccess.tableName;
        TableItem? table;
        try
        {
             table = await tableAccess.Client.CreateTableIfNotExistsAsync(_tableName, cancellationToken);
        }
        catch (Exception Ex)
        {
            Console.WriteLine(Ex);
            throw;
        }   
        
        return tableAccess;
    }

    /// <summary>
    /// Queries for a tables in the list of tables 
    /// </summary>
    /// <param name="tableAccess">The Table Access </param>
    /// <param name="tableName">The Table Name to override in the table access.</param>
    public static async Task<IList<TableItem>> Query_StorageTables_Async(this TableAccess tableAccess, string? tableName = null, CancellationToken cancellationToken = default)
    {
        AsyncPageable<TableItem> queryTableResults = tableAccess.Client.QueryAsync(filter: $"TableName eq '{tableName ?? tableAccess.tableName}'", cancellationToken: cancellationToken);

        //Console.WriteLine("The following are the names of the tables in the query results:");

        // Iterate the <see cref="Pageable"> in order to access queried tables.
        List<TableItem> tableList = new();

        await foreach (var queryTableResult in queryTableResults)
            if (queryTableResult != null)
                tableList.Add(queryTableResult);        

        return tableList;
    }

    /// <summary>
    /// Deletes the full Table
    /// </summary>
    /// <param name="tableAccess">The table access</param>
    /// <param name="tableName">The name of the table</param>
    public static Task Delete_StorageTable_Async(this TableAccess tableAccess, string tableName, CancellationToken cancellationToken = default)
    {
        if(tableName?.Length > 0)
        {
            if (!tableName.Equals(tableAccess.tableName))
                throw new InvalidOperationException($"Only the tableAccess.tableName can be delete and must match the supplied tableName to delete. The TableName: {tableName} does not match the tableAccess.tableName: {tableAccess.tableName}");

                return tableAccess.Client.DeleteTableAsync(tableName, cancellationToken: cancellationToken);
        }

        return Task.CompletedTask;
    }

    /*Table Entities*/

    /// <summary>
    /// Queries the storage table by PartitionKey. 
    /// </summary>
    /// <typeparam name="Table_Entity">The entity being queried</typeparam>
    /// <param name="tableAccess">The Table access object</param>
    /// <param name="PartitionKey">The table partition Key</param>
    public static async Task<IEnumerable<Table_Entity>> Query_Entity_ByPartionKey_Async<Table_Entity>(this TableAccess tableAccess, string PartitionKey, CancellationToken cancellationToken = default) where Table_Entity : class, ITableEntity, new()
    {

        AsyncPageable<Table_Entity>? tt = tableAccess.tableClient.QueryAsync<Table_Entity>(filter: (x => x.PartitionKey.Equals(PartitionKey) ), cancellationToken: cancellationToken);
        List<Table_Entity> Result_Entities = new();

        //write the entity results
        await foreach (var item in tt.AsPages())
            Result_Entities.AddRange(item.Values);

        return Result_Entities;
    }

    /// <summary>
    /// Queries the storage table by RowKey. 
    /// </summary>
    /// <typeparam name="Table_Entity">The entity being queried</typeparam>
    /// <param name="tableAccess">The Table access object</param>
    /// <param name="PartitionKey">The table partition Key</param>
    /// <param name="RowKey">The Row from the partition</param>
    public static async Task<Table_Entity?> Query_Entity_ByRowKey_Async<Table_Entity>(this TableAccess tableAccess, string PartitionKey, string RowKey, CancellationToken cancellationToken = default) where Table_Entity : class, ITableEntity, new()
    {

        AsyncPageable<Table_Entity>? tt = tableAccess.tableClient.QueryAsync<Table_Entity>(filter: (x => x.PartitionKey.Equals(PartitionKey) && x.RowKey.Equals(RowKey)), cancellationToken: cancellationToken);
        List<Table_Entity> Result_Entities = new();

        //write the entity results
        await foreach (var item in tt.AsPages())
            Result_Entities.AddRange(item.Values);

        return Result_Entities.FirstOrDefault();
    }

    /// <summary>
    /// Creates a Record in the storage Table will not update!!
    /// </summary>
    /// <param name="entity">The Object that is being updated</param>
    public static async Task<Table_Entity?> Create_Entity_IfNotExists_Async<Table_Entity>(this TableAccess tableAccess, Table_Entity entity, CancellationToken cancellationToken = default) where Table_Entity : class, ITableEntity, new()
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        try
        {
            _ = await tableAccess.tableClient.AddEntityAsync(entity, cancellationToken: cancellationToken); 
        }
        catch (RequestFailedException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (Exception)
        {
            throw;
        }

        return await tableAccess.Query_Entity_ByRowKey_Async<Table_Entity>(entity.PartitionKey, entity.RowKey);        
    }

    /// <summary>
    /// Add in a new Entity if not in table. Else it will delete old entity and create the new one.
    /// </summary>
    /// <typeparam name="Table_Entity">The type of entity being uploaded</typeparam>
    /// <param name="tableAccess">The table access</param>
    /// <param name="entity">The Entity to update</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static async Task<Table_Entity?> Create_Entity_Clean_UpdateIfExists_Async<Table_Entity>(this TableAccess tableAccess, Table_Entity entity, CancellationToken cancellationToken = default) where Table_Entity : class, ITableEntity, new()
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        try
        {
            _ = tableAccess.tableClient.DeleteEntity(entity.PartitionKey, entity.RowKey, cancellationToken: cancellationToken);

            _ = await tableAccess.tableClient.AddEntityAsync(entity, cancellationToken: cancellationToken);

        }
        catch (RequestFailedException)
        {
            return null;
        }
        catch (Exception)
        {
            throw;
        }

        return await tableAccess.Query_Entity_ByRowKey_Async<Table_Entity>(entity.PartitionKey, entity.RowKey);
    }

    public static async Task Delete_Entity(this TableAccess tableAccess, string PartitionKey, string[] RowKeys, CancellationToken cancellationToken = default)
    {
        foreach (var RowKey in RowKeys)
            if (RowKey?.Length > 0)
                try
                {
                    _ = await tableAccess.tableClient.DeleteEntityAsync(PartitionKey, RowKey, cancellationToken: cancellationToken);
                }
                catch (RequestFailedException ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Ex);
                    throw;
                }
    }

    /*Filter help */
    /// <summary>
    /// Helps build Queries for table access filetering
    /// <see cref="https://docs.microsoft.com/en-us/rest/api/storageservices/querying-tables-and-entities"/>
    /// </summary>
    internal static class TableFilter
    {
        /// <summary>
        /// Queries on Partion Key
        /// </summary>
        /// <param name="PartitionKey">The value of the key</param>
        /// <returns>a query</returns>
        public static string Query_OnPartitionKey(string PartitionKey) => $"{nameof(ITableEntity.PartitionKey)} eq '{PartitionKey}'";

        public static string Query_OnRowKey(string PartitionKey, string RowKey) => $"{nameof(ITableEntity.PartitionKey)} eq '{PartitionKey}' and {nameof(ITableEntity.RowKey)} eq '{RowKey}'";


        



        /*  Notes: 

            Helper Links: 
            Supported Comparison Operators

                Within a $filter clause, you can use comparison operators to specify the criteria against which to filter the query results.

                For all property types, the following comparison operators are supported:
                Supported Comparison Operators
                    -- Operator 	URI expression
                    -- Equal 	eq
                    -- GreaterThan 	gt
                    -- GreaterThanOrEqual 	ge
                    -- LessThan 	lt
                    -- LessThanOrEqual 	le
                    -- NotEqual 	ne

                Additionally, the following operators are supported for Boolean properties:
                Supported Comparison Operators
                    -- Operator 	URI expression
                    -- And 	and
                    -- Not 	not
                    -- Or 	or

            Query String Encoding

                The following characters must be encoded if they are to be used in a query string:

                - Forward slash (/)
                - Question mark (?)
                - Colon (:)
                - 'At' symbol (@)
                - Ampersand (&)
                - Equals sign (=)
                - Plus sign (+)
                - Comma (,)
                - Dollar sign ($)

            Single quote (')

                Single quotes in query strings must be represented as two consecutive single quotes ('')

         */
    }


   
}




/*
    public static class AzureTableStorageUtility
    {

        /// <summary>
        /// Creates a storage Table connection Account.
        /// </summary>
        /// <param name="storageConnectionString"></param>
        static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the application.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                Console.ReadLine();
                throw;
            }

            return storageAccount;
        }

        /// <summary>
        /// Creates a new Table
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public static async Task<CloudTable> CreateTableAsync(Action<TableOptions> action)
        {

            TableOptions op = new TableOptions(action);
            string tableName = op.table_name;

            CloudStorageAccount storageAccount = op.cloudStorageAccount();

            CloudTableClient tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());


            CloudTable table = tableClient.GetTableReference(tableName);
            if (await table.CreateIfNotExistsAsync())
            {
                Console.WriteLine("Created Table named: {0}", tableName);
            }
            else
            {
                Console.WriteLine("Table {0} already exists", tableName);
            }

            Console.WriteLine();
            return table;

        }

        /// <summary>
        /// Use the Table Options to pull a Cloud storage account.
        /// </summary>
        public static CloudStorageAccount cloudStorageAccount(this TableOptions op) =>
            CreateStorageAccountFromConnectionString(op.connection_Key);

        /// <summary>
        /// Creates or Updates multiple Records in the storage Table
        /// </summary>
        /// <param name="entity">The Objects that are being updated</param>
        public static async Task<IEnumerable<Table_Entity>> post_EntitiesAsync<Table_Entity>(this CloudTable table, params Table_Entity[] entitites)
            where Table_Entity : TableEntity, new()
        {
            List<Table_Entity> Processed_table_Entities = new List<Table_Entity>();

            foreach (var i in entitites)
            {
                try
                {
                    Processed_table_Entities.Add(await table.post_EntityAsync(i));

                }
                catch (Exception Ex)
                {
                    Console.WriteLine(Ex);
                }
            }


            return Processed_table_Entities;
        }

        /// <summary>
        /// Creates or Updates a Record in the storage Table
        /// </summary>
        /// <param name="entity">The Object that is being updated</param>
        public static async Task<Table_Entity> post_EntityAsync<Table_Entity>(this CloudTable table, Table_Entity entity) where Table_Entity : TableEntity, new()
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            try
            {
                // Create the InsertOrReplace table operation
                TableOperation insertOrMergeOperation = TableOperation.InsertOrMerge(entity);

                // Execute the operation.
                TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
                Table_Entity insertedEntity = result.Result as Table_Entity;

                //if (result.RequestCharge.HasValue)
                //{
                //    Console.WriteLine("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
                //}

                return insertedEntity;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        /// <summary>
        /// Get a List of Entityies with the supplied PartitionKey
        /// </summary>
        /// <param name="partitionKey">The key to pull values for</param>
        public static IEnumerable<Table_Entity> get_Entities<Table_Entity>(this CloudTable table, string partitionKey) where Table_Entity : TableEntity, new()
        {

            IEnumerable<Table_Entity> results = null;

            try
            {

                TableQuery<Table_Entity> query = new TableQuery<Table_Entity>()
                    .Where(TableQuery.GenerateFilterCondition(nameof(TableEntity.PartitionKey), QueryComparisons.Equal, partitionKey));

                results = table.ExecuteQuery(query) ?? new Table_Entity[] { };

            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }


            foreach (var result in results)
                yield return result;

        }

        /// <summary>
        /// Get a Entity with the supplied PatitionKey and RowKey
        /// </summary>
        /// <typeparam name="Table_Entity"></typeparam>
        /// <param name="table"></param>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        public static async Task<Table_Entity> get_EntityAsync<Table_Entity>(this CloudTable table, string partitionKey, string rowKey) where Table_Entity : TableEntity, new()
        {
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<Table_Entity>(partitionKey, rowKey);
                TableResult result = await table.ExecuteAsync(retrieveOperation);
                Table_Entity ent = result.Result as Table_Entity;


                //if (result.RequestCharge.HasValue)
                //{
                //    Console.WriteLine("Request Charge of Retrieve Operation: " + result.RequestCharge);
                //}

                return ent;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }

        public static async Task delete_EntityAsync<Table_Entity>(this CloudTable table, Table_Entity deleteEntity) where Table_Entity : TableEntity, new()
        {
            try
            {
                if (deleteEntity == null)
                {
                    throw new ArgumentNullException("deleteEntity");
                }

                TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
                TableResult result = await table.ExecuteAsync(deleteOperation);

                //if (result.RequestCharge.HasValue)
                //{
                //    Console.WriteLine("Request Charge of Delete Operation: " + result.RequestCharge);
                //}
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                throw;
            }
        }
    }
//*/



