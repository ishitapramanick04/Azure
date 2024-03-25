using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System;

namespace MSI_Topic_Publisher
{
    public static class MSI_Publisher
    {
        [FunctionName("MSI_Publisher")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestMessage req,
            ILogger log)
        {
            var queueName = "demo-publisher";
            var messageBody = new { Id = 1, Message = "Hello, world!" }; // Example message body
            var connectionString = "https://sb-msi-publisher.azurewebsites.net";

            // Create a managed identity credential
            var credential = new DefaultAzureCredential();

            // Create a Service Bus client
            var client = new ServiceBusClient(connectionString, credential);

            // Create a sender for the queue
            var sender = client.CreateSender(queueName);

            // Create a message using the message body
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageBody)));

            try
            {
                // Send the message to the queue
                await sender.SendMessageAsync(message).ConfigureAwait(false);
                log.LogInformation($"Message published to Service Bus queue '{queueName}'");
            }
            catch (Exception ex)
            {
                log.LogError($"An error occurred while publishing message to Service Bus queue '{queueName}': {ex.Message}");
                return new StatusCodeResult((int)HttpStatusCode.InternalServerError);
            }
            finally
            {
                // Close the sender and client
                                await sender.CloseAsync().ConfigureAwait(false);

                await client.DisposeAsync();
            }

            return new OkObjectResult("Message published successfully");
        }
    }
}

