﻿

using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;

namespace CxAzure.TableStorage;

public static class TableStorageUtilityExtentions
{
    /// <summary>
    /// Creates a New Table in azure table storage
    /// </summary>
    /// <param name="_table">The Table Access</param>
    /// <param name="tableName">create a different table then the TableAccess.tableName</param>
    public static TableAccess CreateTable(this TableAccess _table, string? tableName = null)
    {
        string _tableName = tableName ?? _table.tableName;
        TableItem table = _table.Client.CreateTableIfNotExists(_tableName);        
        return _table;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_table"></param>
    /// <param name="tableName"></param>
    /// <returns></returns>
    public static IList<TableItem> StorageTable(this TableAccess _table, string? tableName = null)
    {
        Pageable<TableItem> queryTableResults = _table.Client.Query(filter: $"TableName eq '{tableName}'");

        //Console.WriteLine("The following are the names of the tables in the query results:");

        // Iterate the <see cref="Pageable"> in order to access queried tables.

        if (queryTableResults.Count() <= 0)
            return new List<TableItem>();
        else 
            return new List<TableItem>(queryTableResults);

        //List<TableItem> tableList = new List<TableItem>(queryTableResults);



        //foreach (TableItem table in queryTableResults)
        //{
        //    tableList.Add()
        //    Console.WriteLine(table.Name);
        //}

        //return tableList;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_table"></param>
    /// <param name="tableName"></param>
    public static void DeleteTable(this TableAccess _table, string tableName)
    {
        if(tableName?.Length > 0)
            _table.Client.DeleteTable(tableName);
    }


    /*Table Entities*/

    //Expression<Func<T, bool>> filter
    -/* 
    public static async Task<IEnumerable<Table_Entity>> QueryEntity_Async<Table_Entity>(this TableAccess _table, TableQueryOptions<Table_Entity> options)
        where Table_Entity : class, ITableEntity, new()
    {
        var tt = await _table.tableClient.QueryAsync(filter: options.filter, maxPerPage: options.maxPerPage, select: options.return_Fields, cancellationToken: options.cancellationToken);

        tt.
    }

    public static async Task<IEnumerable<Table_Entity>> QueryEntity_Async<Table_Entity>(this TableAccess _table, System.Linq.Expressions.Expression<Func<Table_Entity, bool>> filter, CancellationToken cancellationToken = default(CancellationToken))
        where Table_Entity : class, ITableEntity, new() => _table.tableClient.QueryAsync(filter: filter, cancellationToken: cancellationToken);
    //*/


    //        public static async Task<IEnumerable<Table_Entity>> QueryEntity_Async<Table_Entity>(this TableAccess _table, string partitionKey, string rowId) 
    //        where Table_Entity : ITableEntity
    //    {
    //         Pageable<TableEntity> queryResultsFilter = _table.tableClient.Query<TableEntity>(filter: TableFilter.Query_OnRowKey(partitionKey, rowId));



    //        /*

    //// Iterate the <see cref="Pageable"> to access all queried entities.
    //foreach (TableEntity qEntity in queryResultsFilter)
    //{
    //    Console.WriteLine($"{qEntity.GetString("Product")}: {qEntity.GetDouble("Price")}");
    //}

    //Console.WriteLine($"The query returned {queryResultsFilter.Count()} entities.");
    //         */



    //    }

    //    /// <summary>
    //    /// Creates or Updates a Record in the storage Table
    //    /// </summary>
    //    /// <param name="entity">The Object that is being updated</param>
    //    public static async Task<Table_Entity> CreateEntity_IfNotExists_Async<Table_Entity>(this TableAccess tableAccess, Table_Entity entity) where Table_Entity : ITableEntity
    //    {
    //        if (entity == null)
    //        {
    //            throw new ArgumentNullException(nameof(entity));
    //        }





    //        var TableOperation = tableAccess.tableClient;

    //        try
    //        {
    //            // Create the InsertOrReplace table operation
    //            TableOperation insertOrMergeOperation = tableAccess.tableClient.InsertOrMerge(entity);

    //            // Execute the operation.
    //            TableResult result = await table.ExecuteAsync(insertOrMergeOperation);
    //            Table_Entity insertedEntity = result.Result as Table_Entity;

    //            //if (result.RequestCharge.HasValue)
    //            //{
    //            //    Console.WriteLine("Request Charge of InsertOrMerge Operation: " + result.RequestCharge);
    //            //}

    //            return insertedEntity;
    //        }
    //        catch (StorageException e)
    //        {
    //            Console.WriteLine(e.Message);
    //            Console.ReadLine();
    //            throw;
    //        }
    //    }

    //    /// <summary>
    //    /// Creates or Updates multiple Records in the storage Table
    //    /// </summary>
    //    /// <param name="entity">The Objects that are being updated</param>
    //    public static async Task<IEnumerable<Table_Entity>> post_EntitiesAsync<Table_Entity>(this TableAccess tableAccess, params Table_Entity[] entitites)
    //        where Table_Entity : ITableEntity
    //    {
    //        List<Table_Entity> Processed_table_Entities = new List<Table_Entity>();

    //        var table = tableAccess.tableClient;

    //        foreach (var i in entitites)
    //        {
    //            try
    //            {
    //                Processed_table_Entities.Add(await tableAccess.post_EntityAsync(i));

    //            }
    //            catch (Exception Ex)
    //            {
    //                Console.WriteLine(Ex);
    //            }
    //        }


    //        return Processed_table_Entities;
    //    }



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


        //public static string field_Equal(string field, string value)



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


    public record TableQueryOptions<Table_Entity>(System.Linq.Expressions.Expression<Func<Table_Entity, bool>> filter, int? maxPerPage = null, IEnumerable<string> return_Fields = null, CancellationToken cancellationToken = default(CancellationToken)) where Table_Entity : class, ITableEntity, new();

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



