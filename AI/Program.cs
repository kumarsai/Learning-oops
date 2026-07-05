
using OpenAI;
using OpenAI.Responses;
using System.ClientModel;

#pragma warning disable OPENAI001

Console.WriteLine("Hello, World!");

const string deploymentName = "gpt-5.4-mini";
const string endpoint = "https://suku-ai-103-learning-resource.services.ai.azure.com/openai/v1";
const string apiKey = "1O68BhKFEZWcXyVl2oSedqrQLxPBHmDobhKX1hg4p9FYy1iTVUbbJQQJ99CGACHYHv6XJ3w3AAAAACOGbXxd";
ResponsesClient client = new(
    credential: new ApiKeyCredential(apiKey),
    options: new ResponsesClientOptions
    {
        Endpoint = new Uri(endpoint)
    });

CreateResponseOptions options = new()
{
    Model = deploymentName,
    InputItems =
    {
        ResponseItem.CreateUserMessageItem(
            "What's the weather like today for my current location?")
    }
};

ResponseResult response = client.CreateResponse(options);

Console.WriteLine($"[ASSISTANT]: {response.GetOutputText()}");
