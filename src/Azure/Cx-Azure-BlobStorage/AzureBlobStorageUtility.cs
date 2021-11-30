using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace CxAzure.BlobStorage;


public static class AzureBlobStorageUtility
{

    public static async Task Create_blobContainer_Async(this BlobAccess _Access, string containerName, CancellationToken cancellationToken = default)
    {

        BlobContainerClient containerClient = await _Access.ServiceClient..CreateBlobContainerAsync(containerName);

        containerClient.cre

    }


}
