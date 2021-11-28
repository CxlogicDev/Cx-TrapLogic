using Azure.Data.Tables;
using System.Diagnostics.CodeAnalysis;
namespace CxAzure.TableStorage;

public record TableAccess
{

    public string tableName { get; set; }

    [AllowNull]
    string accountUri { get; }
    [AllowNull]
    string accountName { get; }
    [AllowNull]
    string accountKey { get; }

    bool useConnString { get; }

    public TableAccess(string TableName, string AccountUri, string AccountName, string AccountKey)
    {
        tableName = TableName;
        accountUri = AccountUri;
        accountName = AccountName;
        accountKey = AccountKey;

        if (!isValid())
            throw new ArgumentException();
    }

    [AllowNull]
    public string accountConnString { get; }

    public TableAccess(string TableName, string AccountConnString)
    {
        tableName = TableName;
        accountConnString = AccountConnString;
        useConnString = true;
        //if (!isValid())
        //    throw new ArgumentException();
    }

    private TableServiceClient? _client { get; set; }

    public TableServiceClient Client
    {
        get
        {
            if (_client == null)
                _client = useConnString? new TableServiceClient(accountConnString) :
                    new TableServiceClient(new Uri(accountUri), new TableSharedKeyCredential(accountName, accountKey));

            return _client;
        }
    }

    private TableClient? _tableClient { get; set; }

    public TableClient tableClient
    {
        get
        {
            if(_tableClient == null)
                _tableClient = useConnString? new TableClient(accountConnString, tableName):
                    new TableClient(new Uri(accountUri), tableName, new TableSharedKeyCredential(accountName, accountKey));

            return _tableClient;
        }
    }

    //[MemberNotNullWhen(true, nameof(table_name), nameof(table_connection))]
    internal bool isValid() => tableName?.Length > 0 && ((accountUri?.Length > 0 && accountName?.Length > 0 && accountKey?.Length > 0) || accountConnString?.Length > 0);
}