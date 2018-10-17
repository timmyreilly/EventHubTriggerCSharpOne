using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Company.Function
{
    public static class EventHubTriggerCSharp
    {
        [FunctionName("EventHubTriggerCSharp")]
        public static void Run([EventHubTrigger("bighub", Connection = "EventHub")]JObject[] myEventHubMessages, ILogger log)
        {
            log.LogInformation("WAAAA HERE WE GO!");

            var cosmosEndpointUri = new Uri(Environment.GetEnvironmentVariable("CosmosEndpoint", EnvironmentVariableTarget.Process));
            var cosmosKey = Environment.GetEnvironmentVariable("CosmosKey", EnvironmentVariableTarget.Process);

            using (var dClient = new DocumentClient(cosmosEndpointUri, cosmosKey))
            {

                log.LogInformation("GOT HERE");
                dClient.CreateDatabaseIfNotExistsAsync(new Database() { Id = "Challenge7" }).GetAwaiter().GetResult();

                dClient.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri("Challenge7"),
                new DocumentCollection { Id = "BUYBUYBUY" }).
                GetAwaiter()
                .GetResult();

                // var rating = cosmosService.CreateRating(data);
                foreach (var message in myEventHubMessages)
                {
                    log.LogInformation($"One Of them: {message}");

                    dClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri("Challenge7", "BUYBUYBUY"), message).GetAwaiter().GetResult();
                }


            }
        }
    }
}
