using Azure.Messaging.ServiceBus;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusPublisher
{

    public class TopicConnStringPublisher
    {
        private readonly ServiceBusClient _client;

        public TopicConnStringPublisher(string connectionString)
        {
            var clientOptions = new ServiceBusClientOptions()
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };
            _client = new ServiceBusClient(connectionString, clientOptions);
        }

        public async Task SendMessageBatchAsync(string topicName, string subscriptionName, int numOfMessages)
        {
            // Create a sender for the topic
            var sender = _client.CreateSender(topicName);

            for (int i = 1; i <= numOfMessages; i++)
            {
                // Create a message using your custom format
                var message = new ServiceBusMessage(Encoding.UTF8.GetBytes($"Message {i}"));

                // Assign subscription name to the message via the "To" property
                message.ApplicationProperties["To"] = subscriptionName;

                // Send the message
                await sender.SendMessageAsync(message);
                Console.WriteLine($"Message {i} sent to subscription: {subscriptionName}.");
            }

            // Dispose of the sender to release resources
            await sender.DisposeAsync();
        }

        public async Task CloseAsync()
        {
            // Dispose of the client to release resources
            await _client.DisposeAsync();
        }
    }
}
