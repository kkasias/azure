using System.Net;
using Azure.Storage;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;

namespace Cto.KK.Queue;

public class QueueRunner : RunnerBase
{
  private readonly string _BaseUrlPrimary = @"https://ctokk.queue.core.windows.net/";

  public QueueRunner(string connectionString, string accountKey, string accountName)
		  : base(connectionString, accountKey, accountName)
  {
  }

  public async Task Execute()
  {
	const string queueName = "motion-detected"; // Lowercase only

	Console.WriteLine("Starting Queue Runner");

	await CreateQueue(queueName);
	InsertMessage(queueName, "Message", 20);
	// Display number of messages.
	Console.WriteLine($"Number of messages in queue: {GetQueueLength(queueName)}");
	PeekNextMessage(queueName);
	
	DequeueMessage(queueName);
	Console.WriteLine($"Number of messages in queue: {GetQueueLength(queueName)}");

	DequeueMessageNoDelete(queueName);
	Console.WriteLine($"Number of messages in queue: {GetQueueLength(queueName)}");

	Console.WriteLine("Ending Queue Runner");
  }

  private void DequeueMessageNoDelete(string queueName)
  {
	  QueueClient queueClient = new QueueClient(_StorageConnectionString, queueName);

	  // Get the next message
	  QueueMessage[] retrievedMessage = queueClient.ReceiveMessages();

	  // Process (i.e. print) the message in less than 30 seconds
	  Console.WriteLine($"Dequeued message: '{retrievedMessage[0].Body}'");
	  // Because no delete message will be re-queued. DequeueCount will be +1 and it will be at the end of the queue
	  Thread.Sleep(35000);
  }

  private void DequeueMessage(string queueName)
  {
	  QueueClient queueClient = new QueueClient(_StorageConnectionString, queueName);

	  // Get the next message
	  QueueMessage[] retrievedMessage = queueClient.ReceiveMessages();

	  // Process (i.e. print) the message in less than 30 seconds
	  Console.WriteLine($"Dequeued message: '{retrievedMessage[0].Body}'");

	  // Delete the message
	  queueClient.DeleteMessage(retrievedMessage[0].MessageId, retrievedMessage[0].PopReceipt);
	  Console.WriteLine("Deleted Message.");
  }
  private void PeekNextMessage(string queueName)
  {
	  QueueClient queueClient = new QueueClient(_StorageConnectionString, queueName);
	  var msg = queueClient.PeekMessage();
	  if (msg == null)
		  return;

	  Console.WriteLine(msg.Value.Body.ToString());
  }

  public async Task<bool> CreateQueue(string queueName)
  {
	QueueClient queueClient = new QueueClient(_StorageConnectionString, queueName);
	bool retVal = false;

	try
	{ 
		await queueClient.CreateIfNotExistsAsync();
	  if(await queueClient.ExistsAsync())
		retVal = true;
	  else
	  {
		  retVal = false;
	  }
	}
	catch (Exception e)
	{
	  Console.WriteLine(e);
	  throw;
	}

	return retVal;
  }

  public void InsertMessage(string queueName, string messageBody, int messageCount)
  {
	QueueClient queueClient = new QueueClient(_StorageConnectionString, queueName);

	try
	{
		if (GetQueueLength(queueName) <= 0)
		{
			for (int i = 0; i < messageCount; i++)
			{
				queueClient.SendMessage($"{messageBody} - {i}");
			}
		}
	}
	catch (Exception e)
	{
		Console.WriteLine(e);
		throw;
	}
  }

//-----------------------------------------------------
// Get the approximate number of messages in the queue
//-----------------------------------------------------
  public int GetQueueLength(string queueName)
  {
	  // Instantiate a QueueClient which will be used to manipulate the queue
	  QueueClient queueClient = new QueueClient(_StorageConnectionString, queueName);

	  if (queueClient.Exists())
	  {
		  QueueProperties properties = queueClient.GetProperties();

		  // Retrieve the cached approximate message count.
		  int cachedMessagesCount = properties.ApproximateMessagesCount;

		  return cachedMessagesCount;
	  }

	  return -1;
  }
}