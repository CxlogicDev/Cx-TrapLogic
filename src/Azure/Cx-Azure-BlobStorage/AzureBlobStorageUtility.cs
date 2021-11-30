using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace CxAzure.BlobStorage;


public static class AzureBlobStorageUtility
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_Access"></param>
    /// <param name="containerName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task Create_blobContainer_Async(this BlobAccess _Access, string? ContainerName = null, CancellationToken cancellationToken = default)
    {
        _ = await _Access.ConatinerClient(ContainerName ?? _Access.containerName)
            .CreateIfNotExistsAsync(cancellationToken: cancellationToken);
    }

    public static async Task<bool> Delete_blobContainer_Async(this BlobAccess _Access, string ContainerName, CancellationToken cancellationToken = default)
    {
        bool isDeleted = false;

        if(ContainerName.Equals(_Access.containerName))
            isDeleted = await _Access.ConatinerClient(_Access.containerName)
            .DeleteIfExistsAsync(cancellationToken: cancellationToken);

        return isDeleted;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_Access"></param>
    /// <param name="ContainerName"></param>
    /// <returns></returns>
    public static async Task<List<BlobItem>> List_Blob_Async(this BlobAccess _Access, string? ContainerName = null, CancellationToken cancellationToken = default)
    {
        List<BlobItem> items = new List<BlobItem>();

        await foreach (BlobItem blobItem in _Access.ConatinerClient(ContainerName ?? _Access.containerName).GetBlobsAsync(cancellationToken: cancellationToken))
            items.Add(blobItem);
        
        return items;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_Access"></param>
    /// <param name="filePath"></param>
    /// <param name="ContainerName"></param>
    /// <param name="blob_directory"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task Upload_Blob_Async(this BlobAccess _Access, string filePath, string? ContainerName = null, string? blob_directory = null, CancellationToken cancellationToken = default)
    {
        if (!(filePath?.Length > 0) || !File.Exists(filePath))
            return;

        string blob = filePath.Split(Path.DirectorySeparatorChar).Last();

        if (blob_directory?.Length > 0)
            blob = $"{blob_directory.TrimEnd('/')}/{blob}";
       
        BlobClient blobClient = _Access.ConatinerClient(ContainerName ?? _Access.containerName).GetBlobClient(blob);

        if(!(await blobClient.ExistsAsync(cancellationToken)))
            _ = await blobClient.UploadAsync(filePath, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_Access"></param>
    /// <param name="Destination_Path"></param>
    /// <param name="ContainerName"></param>
    /// <param name="blob_Path_Name"></param>
    /// <param name="blob_directory"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<System.Net.HttpStatusCode> Download_Blob_Async(this BlobAccess _Access, string Destination_Path, string? blob_Path_Name, string? ContainerName = null, CancellationToken cancellationToken = default)
    {
        if (!(Destination_Path?.Length > 0))
            return System.Net.HttpStatusCode.BadRequest;

        var pathParts = Destination_Path.Split(Path.DirectorySeparatorChar);
               
        if (!Directory.Exists(Path.Combine(pathParts[0..^2])))
            _ = Directory.CreateDirectory(Path.Combine(pathParts[0..^2]));        

        BlobClient blobClient = _Access.ConatinerClient(ContainerName ?? _Access.containerName)
            .GetBlobClient(blob_Path_Name);

        if (await blobClient.ExistsAsync(cancellationToken))
        {
            var result = await blobClient.DownloadToAsync(Destination_Path);

            return (System.Net.HttpStatusCode)result.Status;

        }

        return System.Net.HttpStatusCode.BadRequest;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_Access"></param>
    /// <param name="blob_Path_Name"></param>
    /// <param name="ContainerName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<bool> Delete_Blob_Async(this BlobAccess _Access, string? blob_Path_Name, string? ContainerName = null, CancellationToken cancellationToken = default)
    {
       var result = await _Access.ConatinerClient(ContainerName ?? _Access.containerName)
            .DeleteBlobIfExistsAsync(blob_Path_Name, cancellationToken: cancellationToken);

        return result.Value;
    }
}


/*
 Modles from Link: https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet
 
 */