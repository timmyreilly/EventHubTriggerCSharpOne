using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Net.Http;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Company.Function
{

    public static class CreateRating
    {
        private static Lazy<DocumentClient> lazyClient = new Lazy<DocumentClient>(CosmosService.InitializeDocumentClient);
        private static DocumentClient documentClient => lazyClient.Value;
        private static readonly HttpClient client = new HttpClient();



        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = new StreamReader(req.Body).ReadToEnd();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("Challenge7", "ratings");
            documentClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("Challenge7"), new DocumentCollection { Id = "ratings" }).GetAwaiter().GetResult();

            var id = Guid.NewGuid();

            var postData = new
            {
                documents = new[]
                    {
                        new
                        {
                            language = "en",
                            id = id,
                            text = data.userNotes
                        }
                    }
            };

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("CogServKey"));
            var sentimentResponse = await client.PostAsJsonAsync("https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment", postData);

            double sentiment = 0;

            if (sentimentResponse.IsSuccessStatusCode)
            {
                var scores = await sentimentResponse.Content.ReadAsAsync<dynamic>();
                var documentScore = ((IEnumerable<dynamic>)scores.documents).SingleOrDefault();
                sentiment = double.Parse((string)documentScore.score);
                if (sentiment < .3)
                {
                    log.LogWarning($"{sentiment} is below threshold");
                }
            }

            log.LogInformation("Sentiment: " + sentiment);
            // data.sentiment = sentiment; 

            Rating document = new Rating 
            {
                id = id.ToString(),
                userId = data.userId ?? "userIdPlaceHolder",
                productId = data.productId ?? "productIdPlaceHolder",
                timestamp = DateTime.UtcNow,
                locationName = data.locationName ?? "locationPlaceholder",
                rating = data.rating ?? 3,
                userNotes = data.userNotes ?? "userNotesPlaceHolder", 
                sentimentScore = sentiment
            };


            await documentClient.UpsertDocumentAsync(collectionUri, document);


            return data != null
                ? (ActionResult)new OkObjectResult(document)
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }

        public class Rating
        {
            public Rating() => this.id = Guid.NewGuid().ToString();
            public string id { get; set; }
            public string userId { get; set; }
            public string productId { get; set; }
            public DateTime timestamp { get; set; }
            public string locationName { get; set; }
            public int rating { get; set; }
            public string userNotes { get; set; }
            public int magicNumber { get; set; }
            public double sentimentScore { get; set; }
        }
    }
}
