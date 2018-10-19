using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Company.Function
{
    public static class CosmosDBToEventHub
    {
        [FunctionName("CosmosDBToEventHub")]
        public static void Run([CosmosDBTrigger(
            databaseName: "Challenge7",
            collectionName: "POSData",
            ConnectionStringSetting = "AccountEndpoint=https://kosmos-staging.documents.azure.com:443/;AccountKey=6vLRnng70XqmIShdWwjDYT7SYccxAxwI9ds04DAqySA74afW4b4YMdjJlqIyxPQM4ON3Rocd1bSJj6r9g2r1hw==;",
            LeaseCollectionName = "leases")]IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);
            }
        }
    }
}
