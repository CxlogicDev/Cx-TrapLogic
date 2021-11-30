using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Blobs;

namespace CxAzure.BlobStorage;

public class BlobAccess
{
    [AllowNull]
    private string accountConnString { get; }

    public string containerName { get; }

    public BlobAccess(string ContainerName, string AccountConnString)
    {
        accountConnString = AccountConnString;
        containerName = ContainerName;
    }


    BlobServiceClient? _serviceClient { get; set; }//= new BlobServiceClient(connectionString);

    public BlobServiceClient ServiceClient
    {
        get
        {
            if (_serviceClient == null)
                _serviceClient = new BlobServiceClient(accountConnString);

            return _serviceClient;
        }
    }

    public BlobContainerClient ConatinerClient(string ContainerName)
    {
        if(ContainerName?.Length > 0)
            return new BlobContainerClient(accountConnString, ContainerName);        

        throw new InvalidOperationException($"The {nameof(ContainerName)} value is null or empty!!");
    }

    



}

