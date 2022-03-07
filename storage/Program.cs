using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage;
using Microsoft.Extensions.Configuration;
using System.IO;

//string filePath = Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, @"..\..\secrets.json");

var config = new ConfigurationBuilder()
    //.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
    //.AddJsonFile(filePath)
    .AddUserSecrets<Program>()
    .Build();
    
var configSection = config.GetSection("Storage");
string _BlobStorageConnectionString = configSection.GetValue<string>("ConnectionString");
string _ContainerName = "authors";
string _AccountKey = configSection.GetValue<string>("AccountName");
string _BlobName = "About-KK.Html";
string _BaseUrlPrimary = "https://ctokk.blob.core.windows.net";
string _BaseUrlSecondary = "https://ctokk-secondary.blob.core.windows.net";

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
    new StorageSharedKeyCredential("ctokk", _AccountKey)
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

Console.WriteLine("XX.  Deleting 'authors' container.");
containerClient.DeleteIfExists();


