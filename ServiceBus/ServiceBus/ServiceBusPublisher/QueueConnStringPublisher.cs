using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

/// <summary>
/// This publisher uses connection string method to publish data
/// </summary>
namespace ServiceBusPublisher
{
    public class QueueConnStringPublisher
    {
        private ServiceBusClient _client; 
        private ServiceBusSender _sender;

        public QueueConnStringPublisher(string connectionString, string queueName)
        {
            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
            _client = new ServiceBusClient(connectionString, clientOptions);
            _sender = _client.CreateSender(queueName);
        }

        public async Task SendMessageBatchAsync(int numOfMessages)
        {
            // Create a batch
            using ServiceBusMessageBatch messageBatch = await _sender.CreateMessageBatchAsync();

            for (int i = 1; i <= numOfMessages; i++)
            {
                // Try adding a message to the batch
                if (!messageBatch.TryAddMessage(new ServiceBusMessage($"Message {i}")))
                {
                    // If it is too large for the batch
                    throw new Exception($"The message {i} is too large to fit in the batch.");
                }
            }

            try
            {
                // Use the sender to send the batch of messages to the Service Bus queue
                await _sender.SendMessagesAsync(messageBatch);
                Console.WriteLine($"A batch of {numOfMessages} messages has been published to the queue.");
            }
            finally
            {
                // Dispose of the sender and client to release resources
                await _sender.DisposeAsync();
                await _client.DisposeAsync();
            }
        }
    }
}

