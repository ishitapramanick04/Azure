using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using System.Text;
using System.Text.Json;
using System;

namespace MSI_Topic_Publisher
{
    public static class MSI_Publisher
    {
        [FunctionName("MSI_Publisher")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            var queueName = "demo-publisher";
            var messageBody = new { Id = 1, Message = "Hello, world!" }; // Example message body

            // Create a managed identity credential
            var credential = new DefaultAzureCredential();

            // Create a Service Bus client
            var client = new ServiceBusClient("servicebus_msi", credential);

            // Create a sender for the queue
            var sender = client.CreateSender(queueName);

            // Create a message using the message body
            var message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(messageBody)));

            try
            {
                // Send the message to the queue
                await sender.SendMessageAsync(message);
                //log.LogInformation($"Message published to Service Bus queue '{queueName}'");
            }
            catch (Exception ex)
            {
                //log.LogError($"An error occurred while publishing message to Service Bus queue '{queueName}': {ex.Message}");
            }
            finally
            {
                // Close the sender and client
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }
            // parse query parameter
            string name = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
                .Value;

            if (name == null)
            {
                // Get request body
                dynamic data = await req.Content.ReadAsAsync<object>();
                name = data?.name;
            }

            return name == null
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body")
                : req.CreateResponse(HttpStatusCode.OK, "Hello " + name);
        }
    }
}
