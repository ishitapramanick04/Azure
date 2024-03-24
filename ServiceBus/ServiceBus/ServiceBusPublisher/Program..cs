using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBusPublisher
{
    public class Program
    {
        public static async Task Main()
        {
            const string connectionString = "<YOUR SERVICE BUS NAMESPACE - CONNECTION STRING>";
            const string topicName = "topic-demo";
            const string subscriptionName = "topicfilter1";
            const int numOfMessages = 10;

            var publisher = new TopicConnStringPublisher(connectionString);

            try
            {
                await publisher.SendMessageBatchAsync(topicName, subscriptionName, numOfMessages);
                Console.WriteLine("Messages sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                await publisher.CloseAsync();
            }
        }
    }
}
