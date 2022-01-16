using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace CxAzure.BlobStorage;


public static class AzureBlobStorageUtility
{

    /// <summary>
    /// WIll Create a new Blob container if it does not exist.
    /// </summary>
    /// <param name="_Access">The blob storage Access</param>
    /// <param name="ContainerName">The Alternate conatiner then the one in the Access object</param>
    /// <param name="cancellationToken">The supplied Cancle Token</param>
    public static async Task Create_blobContainer_Async(this BlobAccess _Access, string? ContainerName = null, CancellationToken cancellationToken = default)
    {
        _ = await _Access.ConatinerClient(ContainerName ?? _Access.containerName)
            .CreateIfNotExistsAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Deletes the whole blob container.
    /// </summary>
    /// <param name="_Access">The blob storage Access</param>
    /// <param name="ContainerName">The Alternate conatiner then the one in the Access object</param>
    /// <param name="cancellationToken">The supplied Cancle Token</param>
    public static async Task<bool> Delete_blobContainer_Async(this BlobAccess _Access, string ContainerName, CancellationToken cancellationToken = default)
    {
        bool isDeleted = false;

        if(ContainerName.Equals(_Access.containerName))
            isDeleted = await _Access.ConatinerClient(_Access.containerName)
            .DeleteIfExistsAsync(cancellationToken: cancellationToken);

        return isDeleted;
    }

    /// <summary>
    /// Get a list of all blobs in the container.
    /// </summary>
    /// <param name="_Access">The blob storage Access</param>
    /// <param name="ContainerName">The Alternate conatiner then the one in the Access object</param>
    public static async Task<List<BlobItem>> List_Blob_Async(this BlobAccess _Access, string? ContainerName = null, CancellationToken cancellationToken = default)
    {
        List<BlobItem> items = new List<BlobItem>();

        await foreach (BlobItem blobItem in _Access.ConatinerClient(ContainerName ?? _Access.containerName).GetBlobsAsync(cancellationToken: cancellationToken))
            items.Add(blobItem);
        
        return items;
    }

    /// <summary>
    /// Will Upload a file to the azure container.
    /// </summary>
    /// <param name="_Access">The blob storage Access</param>
    /// <param name="filePath">The File Path to the blob to upload</param>
    /// <param name="ContainerName">The Alternate conatiner then the one in the Access object</param>
    /// <param name="blob_directory">Add to a directory</param>
    /// <param name="cancellationToken">The supplied Cancle Token</param>
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
    /// Will Upload a blob file. If the file already it will be deleted first.
    /// </summary>
    /// <param name="_Access">The blob storage Access</param>
    /// <param name="filePath">The File Path to the blob to upload</param>
    /// <param name="ContainerName">The Alternate conatiner then the one in the Access object</param>
    /// <param name="blob_directory">Add to a directory</param>
    /// <param name="cancellationToken">The supplied Cancle Token</param>
    public static async Task Upload_Blob_Delete_If_Exists_Async(this BlobAccess _Access, string filePath, string? ContainerName = null, string? blob_directory = null, CancellationToken cancellationToken = default)
    {
        if (!(filePath?.Length > 0) || !File.Exists(filePath))
            return;

        string blob = filePath.Split(Path.DirectorySeparatorChar).Last();

        if (blob_directory?.Length > 0)
            blob = $"{blob_directory.TrimEnd('/')}/{blob}";

        BlobClient blobClient = _Access.ConatinerClient(ContainerName ?? _Access.containerName).GetBlobClient(blob);

        if (await blobClient.ExistsAsync(cancellationToken))
            _ = await _Access.Delete_Blob_Async(blob, cancellationToken: cancellationToken);

        //Upload in place of Delete blob
        _ = await blobClient.UploadAsync(filePath, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Will download the blob to the supplied system
    /// </summary>
    /// <param name="_Access">The blob storage Access</param>
    /// <param name="Destination_Path">The path to down load the file too</param>
    /// <param name="ContainerName">The Alternate conatiner then the one in the Access object</param>
    /// <param name="blob_Path_Name">Path to the blob to download</param>
    /// <param name="blob_directory">if the blob is in a directory</param>
    /// <param name="cancellationToken">The supplied Cancle Token</param>
    public static async Task<System.Net.HttpStatusCode> Download_Blob_Async(this BlobAccess _Access, string Destination_Path, string? blob_Path_Name, string? ContainerName = null, CancellationToken cancellationToken = default)
    {
        if (!(Destination_Path?.Length > 0))
            return System.Net.HttpStatusCode.BadRequest;

        var pathParts = Destination_Path.Split(Path.DirectorySeparatorChar);
               
        if (!Directory.Exists(Path.Combine(pathParts[0..^1])))
            _ = Directory.CreateDirectory(Path.Combine(pathParts[0..^1]));        

        BlobClient blobClient = _Access.ConatinerClient(ContainerName ?? _Access.containerName)
            .GetBlobClient(blob_Path_Name);

        if (await blobClient.ExistsAsync(cancellationToken))
        {
            var result = await blobClient.DownloadToAsync(Destination_Path, cancellationToken);

            return (System.Net.HttpStatusCode)result.Status;
        }

        return System.Net.HttpStatusCode.BadRequest;
    }

    /// <summary>
    /// Will delete a blob. 
    /// </summary>
    /// <param name="_Access">The blob storage Access</param>
    /// <param name="blob_Path_Name">The blob to delete with the directory added</param>
    /// <param name="ContainerName">The Alternate conatiner then the one in the Access object</param>
    /// <param name="cancellationToken">The supplied Cancle Token</param>
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