using Azure.Identity;
using OpenAI.Responses;
using System.ClientModel.Primitives;

namespace AIDeveloperAssistant.AI;

internal class AIClientFactory
{
#pragma warning disable OPENAI001

    public ResponsesClient CreateClient(string endpoint)
    {
        BearerTokenPolicy tokenPolicy = new(
            new DefaultAzureCredential(),
            "https://ai.azure.com/.default");

        return new ResponsesClient(
            authenticationPolicy: tokenPolicy,
            options: new ResponsesClientOptions
            {
                Endpoint = new Uri(endpoint)
            });
    }
}