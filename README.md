```

<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
    <AzureFunctionsVersion>v2</AzureFunctionsVersion>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Sdk.Functions" Version="1.0.22" />
  </ItemGroup>
  <ItemGroup>
    <None Update="host.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="local.settings.json">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      <CopyToPublishDirectory>Never</CopyToPublishDirectory>
    </None>
  </ItemGroup>
</Project>

```

Freaking CS Proj...
It keeps disappearing on me. 

I've had success getting EventHubTriggers to work on JavaScript, but I've never tried it with CSharp... We're off to a weak start as you've seen I've created a new repo just for this purpose.

I selected the C# runtime, and expanded the template triggers and selected eventhub it asked for my connection string, I entered it, then it created EventHubTriggerCSharp, but the connection string was just hard-coded into the function parameters... Then I got a null error of some sort searched it and found this result on the GitHub issues from a long time ago: https://github.com/Azure/Azure-Functions/issues/278. 

So I added `EventHub : 'endpoint://secretstuff'` to the local.settings.json and put EventHub where the connection string was. Then it started working. 

Okay, but before I go any farther, you'll see a new line in `EventHubTriggerDotNetCore.csproj` for this project... and I think I just realized what I did wrong on the previous attempt... Was it that the connectionstringForEventHub has to be `EventHub`... Let me try. 
Not the answer... still getting: `EventHubTrigger: The binding type(s) 'eventHubTrigger' are not registered. Please ensure the type is correct and the binding extension is installed.` 
Not sure what that's about. 



Right now we have an empty project, to begin we're going to create a new Function. 


Message from EventHub: 

```json
C# Event Hub trigger function processed a message: 
{
    "header":{
        "salesNumber":"b36e2d97-24c5-92e6-4132-e7884d063ead",
        "dateTime":"2018-10-15 23:21:11",
        "locationId":"GGG777",
        "locationName":"Bellows College",
        "locationAddress":"8977 FE Road",
        "locationPostcode":"98101",
        "totalCost":"7.98",
        "totalTax":"0.80"
        },
    "details": 
    [
        {
            "productId":"551a9be9-7f1c-447d-83ee-b18f5a6fb018",
            "quantity":"2",
            "unitCost":"3.99",
            "totalCost":"7.98",
            "totalTax":"0.80",
            "productName":"Matcha Green Tea",
            "productDescription":
            "Green tea ice cream is good for you because it is green."
        }         
    ]
}
```

Okay, now I've changed it to an array of objects: 

```JSON
WAAAA HERE WE GO!
WAAAA HERE WE GO!
One Of them: {"header":{"salesNumber":"fd5102a5-7691-f375-b5fd-bf01cbc1ea5c","dateTime":"2018-10-17 05:54:07","locationId":"GGG777","locationName":"Bellows College","locationAddress":"8977 FE Road","locationPostcode":"98101","totalCost":"8.99","totalTax":"0.90"},"details":[{"productId":"65ab124a-9b2c-4294-a52d-18839364ef15","quantity":"1","unitCost":"8.99","totalCost":"8.99","totalTax":"0.90","productName":"Durian Durian","productDescription":"Smells suspect but tastes... also suspect."}]}

One Of them: {"header":{"salesNumber":"88427797-09eb-271e-2d87-7cf6d4fc0a66","dateTime":"2018-10-17 11:41:15","locationId":"JJJ000","locationName":"Liberty's Delightful Sinful Bakery & Cafe","locationAddress":"441 36th Street","locationPostcode":"98133","totalCost":"23.95","totalTax":"2.40"},"details":[{"productId":"e4e7068e-500e-4a00-8be4-630d4594735b","quantity":"1","unitCost":"3.99","totalCost":"3.99","totalTax":"0.40","productName":"It's Grape!","productDescription":"Unraisinably good ice cream."},{"productId":"e94d85bc-7bd0-44f3-854e-d8cd70348b63","quantity":"4","unitCost":"4.99","totalCost":"19.96","totalTax":"2.00","productName":"Just Peachy","productDescription":"Your taste buds and this ice cream were made for peach other."}]}

One Of them: {"header":{"salesNumber":"fe1d055b-7126-2322-82d0-897295ba854f","dateTime":"2018-10-17 07:03:37","locationId":"FFF666","locationName":"Alpine Ski House","locationAddress":"10 Scott Road","locationPostcode":"98133","totalCost":"7.98","totalTax":"0.80"},"details":[{"productId":"551a9be9-7f1c-447d-83ee-b18f5a6fb018","quantity":"2","unitCost":"3.99","totalCost":"7.98","totalTax":"0.80","productName":"Matcha Green Tea","productDescription":"Green tea ice cream is good for you because it is green."}]}

WAAAA HERE WE GO!
```

Okay so next we need to connect to cosmos to write documents for each of these things we're getting. 
To connected to cosmos we need to add endpoint and connection string settings in local.settings.json and add this line to package reference in your .csproj: 
` <PackageReference Include="Microsoft.Azure.WebJobs.Extensions.CosmosDB" Version="3.0.0-beta7" /> ` 
There might be a prompt in the lower right corner of your visual studio code if you have the C# extension installed... should say something about restore. 
Click restore and now you should be able to talk to cosmos from your function: 

Okay, now local settings needs to have your deets and my EventHubTriggerCsharp.cs looks like this: 

```csharp
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
            foreach (var myEventHubMessage in myEventHubMessages)
            {
                log.LogInformation($"One Of them: {myEventHubMessage}");
            }


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
                    dClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri("Challenge7", "BUYBUYBUY"), message).GetAwaiter().GetResult();
                }


            }
        }
    }
}
```

Not sure it's the best way to to do it, but it works. 

Important TidBit: https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-event-processor-host#checkpointing

```
The storage account used for checkpointing probably would not handle this load, but more importantly checkpointing every single event is indicative of a queued messaging pattern for which a Service Bus queue might be a better option than an event hub. The idea behind Event Hubs is that you get "at least once" delivery at great scale. By making your downstream systems idempotent, it is easy to recover from failures or restarts that result in the same events being received multiple times.

```

So if we're dropping into Cosmos... we need to make sure our append method for our array of events is handled differently. It needs to place the info the queue if it doesn't already exist, or we need some way of removing duplicates on the way in... or later? Or what are the consequences of multiple events in the same document? 
