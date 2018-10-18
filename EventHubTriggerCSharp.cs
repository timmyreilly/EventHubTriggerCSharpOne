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

        private static Lazy<DocumentClient> lazyClient = new Lazy<DocumentClient>(CosmosService.InitializeDocumentClient);
        private static DocumentClient dClient => lazyClient.Value;

        [FunctionName("EventHubTriggerCSharp")]
        public static void Run([EventHubTrigger("bighub", Connection = "EventHub"), Disable()]JObject[] myEventHubMessages, ILogger log)
        {
            // log.LogInformation("WAAAA HERE WE GO!");


            dClient.CreateDatabaseIfNotExistsAsync(new Database() { Id = "Challenge7" }).GetAwaiter().GetResult();
            dClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("Challenge7"), new DocumentCollection { Id = "BUYBUYBUY" }).GetAwaiter().GetResult();

            foreach (var message in myEventHubMessages)
            {
                log.LogInformation($"One Of them: {message}");

                dClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri("Challenge7", "BUYBUYBUY"), message).GetAwaiter().GetResult();
            }

        }
    }
}
