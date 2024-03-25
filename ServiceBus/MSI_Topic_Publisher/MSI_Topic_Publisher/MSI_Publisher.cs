using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using AuthorizationLevel = Microsoft.Azure.WebJobs.Extensions.Http.AuthorizationLevel;

namespace MSI_Topic_Publisher
{
    public static class MSI_Publisher
    {
        [Function("MSI_Publisher")]
        public static async Task<HttpResponseData> Run(
            [Microsoft.Azure.WebJobs.HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("MSI_Publisher");

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
                await sender.SendMessageAsync(message);
                logger.LogInformation($"Message published to Service Bus queue '{queueName}'");
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred while publishing message to Service Bus queue '{queueName}': {ex.Message}");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }
            finally
            {
                // Close the sender and client
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.WriteString("Message published successfully");
            return response;
        }
    }
}
