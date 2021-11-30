using Azure.Data.Tables;
using System.Diagnostics.CodeAnalysis;
namespace CxAzure.TableStorage;

public record TableAccess
{

    public string tableName { get; set; }

    //[AllowNull]
    //string accountUri { get; }
    //[AllowNull]
    //string accountName { get; }
    //[AllowNull]
    //string accountKey { get; }


    //public TableAccess(string TableName, string AccountUri, string AccountName, string AccountKey)
    //{
    //    tableName = TableName;
    //    accountUri = AccountUri;
    //    accountName = AccountName;
    //    accountKey = AccountKey;

    //    if (!isValid())
    //        throw new ArgumentException();
    //}

    //bool useConnString { get; }

    [AllowNull]
    string accountConnString { get; }

    public TableAccess(string TableName, string AccountConnString)
    {
        tableName = TableName;
        accountConnString = AccountConnString;

        //if (_client == null)
        //    _client = new TableServiceClient(accountConnString);

        //if (_tableClient == null)
        //    _tableClient = new TableClient(accountConnString, tableName);
    }

    private TableServiceClient? _client { get; set; }

    public TableServiceClient Client
    {
        get
        {
            if (_client == null)
                _client = new TableServiceClient(accountConnString);

            return _client;
        }
    }

    private TableClient? _tableClient { get; set; }

    public TableClient tableClient
    {
        get
        {
            if(_tableClient == null)
                _tableClient = new TableClient(accountConnString, tableName);

            return _tableClient;
        }
    }

    //[MemberNotNullWhen(true, nameof(table_name), nameof(table_connection))]
    internal bool isValid() => tableName?.Length > 0 && (accountConnString?.Length > 0);
}