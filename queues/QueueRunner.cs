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
	SendReceipt receipt = InsertMessage(queueName, "Message", 20);
	// Display number of messages.
	Console.WriteLine($"Number of messages in queue: {GetQueueLength(queueName)}");
	PeekNextMessage(queueName);

	if(receipt?.MessageId != null)
	{
		UpdateMessage(queueName, receipt);
	}

	DequeueMessage(queueName);
	Console.WriteLine($"Number of messages in queue: {GetQueueLength(queueName)}");

	DequeueMessageNoDelete(queueName);
	Console.WriteLine($"Number of messages in queue: {GetQueueLength(queueName)}");

	DequeueAllMessages(queueName);

	Console.WriteLine("Ending Queue Runner");
  }

  private void DequeueAllMessages(string queueName)
  {
	  QueueClient queueClient = GetQueueClient(queueName);

	  Console.WriteLine("Dequeuing all messages:");

	  var msg = queueClient.ReceiveMessage();
	  while (msg.Value != null)
	  {
		  Console.WriteLine(msg.Value.Body.ToString());
		  queueClient.DeleteMessage(msg.Value.MessageId, msg.Value.PopReceipt);
		  msg = queueClient.ReceiveMessage();
	  }
	  Console.WriteLine("Queue is empty");
  }

  private void DequeueMessageNoDelete(string queueName)
  {
	QueueClient queueClient = GetQueueClient(queueName);

	// Get the next message
	QueueMessage[] retrievedMessage = queueClient.ReceiveMessages();

	// Process (i.e. print) the message in less than 30 seconds
	Console.WriteLine($"Dequeued message: '{retrievedMessage[0].Body}'");
	// Because no delete message will be re-queued. DequeueCount will be +1 and it will be at the end of the queue
	Thread.Sleep(35000);
  }

  private void DequeueMessage(string queueName)
  {
	QueueClient queueClient = GetQueueClient(queueName);

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
	QueueClient queueClient = GetQueueClient(queueName);
	var msg = queueClient.PeekMessage();
	if (msg == null)
	  return;

	Console.WriteLine(msg.Value.Body.ToString());
  }

  public async Task<bool> CreateQueue(string queueName)
  {
	QueueClient queueClient = GetQueueClient(queueName);
	bool retVal;

	try
	{
	  await queueClient.CreateIfNotExistsAsync();
	  if (await queueClient.ExistsAsync())
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

  public SendReceipt InsertMessage(string queueName, string messageBody, int messageCount)
  {
	QueueClient queueClient = GetQueueClient(queueName);

	try
	{
	  if (GetQueueLength(queueName) <= 0)
	  {
		for (int i = 0; i < messageCount; i++)
		{
		  var response = queueClient.SendMessage($"{messageBody} - {i}");
		  return response.Value;
		}
	  }
	}
	catch (Exception e)
	{
	  Console.WriteLine(e);
	  throw;
	}

	return null;
  }

  public void UpdateMessage(string queueName, SendReceipt receipt)
  {
	QueueClient queueClient = GetQueueClient(queueName);

	queueClient.UpdateMessage(receipt.MessageId, receipt.PopReceipt, "This message was updated.");
  }
  //-----------------------------------------------------
  // Get the approximate number of messages in the queue
  //-----------------------------------------------------
  public int GetQueueLength(string queueName)
  {
	// Instantiate a QueueClient which will be used to manipulate the queue
	QueueClient queueClient = GetQueueClient(queueName);

	if (queueClient.Exists())
	{
	  QueueProperties properties = queueClient.GetProperties();

	  // Retrieve the cached approximate message count.
	  int cachedMessagesCount = properties.ApproximateMessagesCount;

	  return cachedMessagesCount;
	}

	return -1;
  }

  private QueueClient GetQueueClient(string queueName)
  {
	// Instantiate a QueueClient which will be used to manipulate the queue
	return new QueueClient(_StorageConnectionString, queueName);
  }
}