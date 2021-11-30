using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Blobs;

namespace CxAzure.BlobStorage;

public class BlobAccess
{
    [AllowNull]
    string accountConnString { get; }

    public BlobAccess(string AccountConnString)
    {
        accountConnString = AccountConnString;
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

    BlobContainerClient ConatinerClient(string ContainerName)
    {
        if(ContainerName?.Length > 0)
            return new BlobContainerClient(accountConnString, ContainerName);        

        throw new InvalidOperationException($"The {nameof(ContainerName)} value is null or empty!!");
    }



}

