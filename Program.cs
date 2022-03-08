using Microsoft.Extensions.Configuration;

namespace Cto.KK;

public class Program{
    
    public static void Main(string[] args){
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();
            
        var configSection = config.GetSection("Storage");
        string _StorageConnectionString = configSection.GetValue<string>("ConnectionString");
        string _AccountName = configSection.GetValue<string>("AccountName");
        string _AccountKey = configSection.GetValue<string>("AccountKey");

        Storage.StorageRunner storageRunner = new Storage.StorageRunner(_StorageConnectionString,
            _AccountKey, _AccountName);
        storageRunner.Execute();
    }
}