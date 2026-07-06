using Azure.Identity;
using Microsoft.Extensions.Configuration;
using OpenAI.Responses;
using System.ClientModel.Primitives;
using System.Text;



namespace AI_Interview_Coach;

internal class InterviewCoachService
{


#pragma warning disable OPENAI001
    private readonly string DeploymentName;
    private readonly string Endpoint;
    private readonly ResponsesClient _client;

    private readonly string _instructions;
    private readonly StringBuilder _conversationHistory = new();
    private string? _previousResponseId;
#pragma warning disable OPENAI001

    public InterviewCoachService()
    {
        IConfiguration config = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json", optional: false)
          .Build();

        Endpoint = config["AzureOpenAI:Endpoint"]
            ?? throw new Exception("Endpoint is missing");

        DeploymentName = config["AzureOpenAI:DeploymentName"]
            ?? throw new Exception("DeploymentName is missing");

        if (string.IsNullOrWhiteSpace(Endpoint))
        {
            throw new Exception("Endpoint environment variable is missing.");
        }

        BearerTokenPolicy tokenPolicy = new(new DefaultAzureCredential(), "https://ai.azure.com/.default");

        _client = new(
            authenticationPolicy: tokenPolicy,
            options: new ResponsesClientOptions
            {

                Endpoint = new Uri(Endpoint)
            });

        //_client = new ResponsesClient(
        //    credential: new ApiKeyCredential(apiKey),
        //    options: new ResponsesClientOptions
        //    {
        //        Endpoint = new Uri(Endpoint)
        //    });

        string instructionPath = Path.GetFullPath("InterviewCoachInstructions.txt");

        if (!File.Exists(instructionPath))
        {
            throw new FileNotFoundException("Instruction file not found.", instructionPath);
        }

        _instructions = File.ReadAllText(instructionPath);
    }

    public async Task SendMessageAsync(string userInput)
    {
        CreateResponseOptions options = new()
        {
            Model = DeploymentName,
            Instructions = _instructions,
            PreviousResponseId = _previousResponseId,
            StreamingEnabled = true,
            InputItems =
            {
                ResponseItem.CreateUserMessageItem(userInput)
            }
        };

        Console.WriteLine();
        Console.Write("[ASSISTANT]: ");

        await foreach (StreamingResponseUpdate update
            in _client.CreateResponseStreamingAsync(options))
        {
            if (update is StreamingResponseOutputTextDeltaUpdate textDelta)
            {
                Console.Write(textDelta.Delta);
            }
            else if (update is StreamingResponseCompletedUpdate completed)
            {
                _previousResponseId = completed.Response.Id;
            }
        }

        Console.WriteLine();
    }

    //public async Task SendMessageAsync(string userInput)
    //{
    //    _conversationHistory.AppendLine();
    //    _conversationHistory.AppendLine($"[USER]: {userInput}");

    //    CreateResponseOptions options = new()
    //    {
    //        Model = DeploymentName,
    //        Instructions = _instructions,
    //        PreviousResponseId = _previousResponseId,
    //        InputItems =
    //        {
    //            //ResponseItem.CreateUserMessageItem(_conversationHistory.ToString())
    //            ResponseItem.CreateUserMessageItem(userInput)
    //        }
    //    };

    //    ResponseResult response = await _client.CreateResponseAsync(options);

    //    string assistantResponse = response.GetOutputText();
    //    _previousResponseId = response.Id;

    //    _conversationHistory.AppendLine();
    //    _conversationHistory.AppendLine($"[ASSISTANT]: {assistantResponse}");

    //    Console.WriteLine();
    //    Console.WriteLine("[ASSISTANT]");
    //    Console.WriteLine(assistantResponse);
    //}
}