using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;
using Microsoft.Extensions.Configuration;
using Cto.KK;

namespace Cto.KK.Storage;

class StorageRunner : RunnerBase {

    string _ContainerName = "authors";
    string _BlobName = "About-KK.Html";
    string _BaseUrlPrimary;
    string _BaseUrlSecondary;

    public StorageRunner(string connectionString, string accountKey, string accountName) 
		: base(connectionString, accountKey, accountName){
        _ContainerName = "authors";
        _BlobName = "About-KK.Html";
        _BaseUrlPrimary = $"https://{_AccountName}.blob.core.windows.net";
        _BaseUrlSecondary = $"https://{_AccountName}-secondary.blob.core.windows.net";
    }
    public void Execute() {
        Console.Write("1.  Connecting to Azure Blob Service.");
        BlobServiceClient serviceClient = new BlobServiceClient(_StorageConnectionString);
        Console.WriteLine("\tConnected!");

        Console.WriteLine("2.  Setting up 'authors' Container Client.");
        BlobContainerClient containerClient = new BlobContainerClient(_StorageConnectionString, _ContainerName);
        Console.WriteLine("2a.  Opening 'authors' Container.  (Creating if it doesn't exist)");
        var blobContainerInfo = containerClient.CreateIfNotExists(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);

        Console.WriteLine("3.  Uploading HTML Files.");
		string filePath = Path.GetFullPath($"..\\..\\..\\storage\\{_ContainerName}");
        FileStream reader = File.OpenRead(Path.Combine(filePath, _BlobName));
        try{
            containerClient.UploadBlob(_BlobName, reader);
        }
        catch(Azure.RequestFailedException ex){
            if(ex.ErrorCode == "BlobAlreadyExists") {
                reader.Position = 0;
                Console.WriteLine($"ERROR: {ex.ToString()}");
                containerClient.DeleteBlobIfExists(_BlobName);
				// Upload latest version
                containerClient.UploadBlob(_BlobName, reader);
            }
        }
        finally{
            reader.Close();
        }
        BlobClient blobClient = new BlobClient(new Uri($"{_BaseUrlPrimary}/{_ContainerName}/{_BlobName}"),
            new StorageSharedKeyCredential(_AccountName, _AccountKey)
        );

        BlobSasBuilder b = new BlobSasBuilder();
        b.SetPermissions(BlobContainerSasPermissions.Read);
        b.ExpiresOn = new DateTimeOffset(DateTime.UtcNow.AddMinutes(2));
        b.BlobContainerName = blobClient.BlobContainerName;
        b.BlobName = blobClient.Name;
        b.Resource = "b";

        if(blobClient.CanGenerateSasUri){ 
            var sasUrl = blobClient.GenerateSasUri(b);
            Console.WriteLine(sasUrl.ToString());
        }
        blobClient.DownloadTo(Path.Combine(filePath, "dl-about-kk.html"));

        Console.WriteLine("Read contents from Secondary End-Point");
        BlobContainerClient secContainerClient = new BlobContainerClient(new Uri($"{_BaseUrlSecondary}/{_ContainerName}"),
            new StorageSharedKeyCredential(_AccountName, _AccountKey)
        );

        while (!secContainerClient.Exists())
        {
			Console.WriteLine("Waiting for replication to Secondary Storage.");
			Thread.Sleep(5000);
        }

        Console.WriteLine($"Reading Contents of {_ContainerName}");
        foreach(var blob in secContainerClient.GetBlobs()){
	        Console.WriteLine($"\t{blob.Name}");
        }
        Console.WriteLine("Finished Reading Container Contents.");
	    Console.WriteLine("Container doesn't exist on Secondary Site yet.");

	    Console.WriteLine("XX.  Deleting 'authors' container.");
        containerClient.DeleteIfExists();
    }
}