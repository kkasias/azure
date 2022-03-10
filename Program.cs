using Microsoft.Extensions.Configuration;

namespace Cto.KK;

public class Program
{    
    public static async Task Main(string[] args){
      var config = new ConfigurationBuilder()
          .AddUserSecrets<Program>()
          .Build();
      string inputOption;
            
      var configSection = config.GetSection("Storage");
      string _StorageConnectionString = configSection.GetValue<string>("ConnectionString");
      string _AccountName = configSection.GetValue<string>("AccountName");
      string _AccountKey = configSection.GetValue<string>("AccountKey");

      do
      {
	      PrintMenu();
	      inputOption = Console.ReadLine();
	      if (inputOption == null)
	      {
		      Console.WriteLine("Something went wrong...exiting!");
		      break;
	      }

	      switch (inputOption)
	      {
		      case "1":
			      Storage.StorageRunner storageRunner = new Storage.StorageRunner(_StorageConnectionString,
					      _AccountKey, _AccountName);
			      storageRunner.Execute();
			      break;
		      case "2":
			      Queue.QueueRunner queueRunner =
					      new Queue.QueueRunner(_StorageConnectionString, _AccountKey, _AccountName);
			      await queueRunner.Execute();
			      break;
		      case "3":
			      break;
		      default:
			      Console.WriteLine("Invalid selection.  Please try again.");
			      break;
	      }
      } while (inputOption != "3");
    }

    public static void PrintMenu()
    {
	    Console.WriteLine("1.\tBlobs");
	    Console.WriteLine("2.\tQueues");
	    Console.WriteLine("3.\tExit\n");
		Console.Write("Please make your selection: ");
    }
}