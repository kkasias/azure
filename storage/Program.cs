using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();
    
var configSection = config.GetSection("Storage");
string _BlobStorageConnectionString = configSection.GetValue<string>("ConnectionString");
string _AccountName = configSection.GetValue<string>("AccountName");
string _AccountKey = configSection.GetValue<string>("AccountKey");
string _ContainerName = "authors";
string _BlobName = "About-KK.Html";
string _BaseUrlPrimary = $"https://{_AccountName}.blob.core.windows.net";
string _BaseUrlSecondary = $"https://{_AccountName}-secondary.blob.core.windows.net";

Console.WriteLine("1.  Connecting to Azure Blob Service.");
BlobServiceClient serviceClient = new BlobServiceClient(_BlobStorageConnectionString);
Console.WriteLine("1a.  Connected!");

Console.WriteLine("2.  Setting up 'authors' Container Client.");
BlobContainerClient containerClient = new BlobContainerClient(_BlobStorageConnectionString, _ContainerName);
Console.WriteLine("2a.  Opening 'authors' Container.  (Creating if it doesn't exist)");
var blobContainerInfo = containerClient.CreateIfNotExists(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);

Console.WriteLine("3.  Uploading HTML Files.");
FileStream reader = File.OpenRead($".\\{_ContainerName}\\about-kk.html");
try{
    containerClient.UploadBlob(_BlobName, reader);
}
catch(Azure.RequestFailedException ex){
    if(ex.ErrorCode == "BlobAlreadyExists") {
        reader.Position = 0;
        Console.WriteLine($"ERROR: {ex.ToString()}");
        containerClient.DeleteBlobIfExists(_BlobName);
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
blobClient.DownloadTo($".\\{_ContainerName}\\dl-about-kk.html");

Console.WriteLine("Read contents from Secondary End-Point");
BlobContainerClient secContainerClient = new BlobContainerClient(new Uri($"{_BaseUrlSecondary}/{_ContainerName}"),
    new StorageSharedKeyCredential(_AccountName, _AccountKey)
);

Console.WriteLine($"Reading Contents of {_ContainerName}");
foreach(var blob in secContainerClient.GetBlobs()){
    Console.WriteLine($"\t{blob.Name}");
}
Console.WriteLine("Finished Reading Container Contents.");


Console.WriteLine("XX.  Deleting 'authors' container.");
containerClient.DeleteIfExists();


