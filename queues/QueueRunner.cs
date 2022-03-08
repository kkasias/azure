namespace Cto.KK.Queue;

public class QueueRunner {        
    string _StorageConnectionString;
    string _AccountName;
    string _AccountKey;

    public QueueRunner(string connectionString, string accountKey, string accountName){
        _StorageConnectionString = connectionString;
        _AccountKey = accountKey;
        _AccountName = accountName;
    }

    public void Execute(){
        Console.WriteLine("Starting Queue Runner");

        //  Do some stuff

        Console.WriteLine("Ending Queue Runner");
    }
}
