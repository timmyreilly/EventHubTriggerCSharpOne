using System;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Company.Function
{
    public static class CosmosService
    {
        public static DocumentClient InitializeDocumentClient()
        {
            // Perform any initialization here
            var uri = new Uri(Environment.GetEnvironmentVariable("CosmosEndpoint", EnvironmentVariableTarget.Process));
            var authKey = Environment.GetEnvironmentVariable("CosmosKey", EnvironmentVariableTarget.Process);

            return new DocumentClient(uri, authKey);
        }
    }
}