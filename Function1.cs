using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus;

namespace TestServiceBusFunctionApp
{
    public static class Function1
    {
        //sidtodo: DO NOT CHECK IN THE ACTUAL CONNECTION STRING!
        //Note: connection string below has been deleted.
        //static string _connectionString = "Endpoint=sb://chrissiddall.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=/RTPPRYoMn4gpLtixGWF+eE93WAaePQp73NY8MHE0mU=";
        static string _connectionString = "..";

        static string _queueName = "TestQueue";

        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name ?? "noname";

            ///////////////

            string responseMessage = "";
            try
            {
                await using (var sbClient = new ServiceBusClient(_connectionString))
                await using (var sender = sbClient.CreateSender(_queueName))
                using (var messageBatch = await sender.CreateMessageBatchAsync())
                {

                    messageBatch.TryAddMessage(new ServiceBusMessage(name));
                    await sender.SendMessagesAsync(messageBatch);

                    responseMessage = $"{name} added to the queue successfully.";

                }
            }
            catch (Exception e)
            {
                responseMessage = $"Failed to addd {name} to the queue: {e.ToString()}";
            }

            return new OkObjectResult(responseMessage);

        }
    }
}
