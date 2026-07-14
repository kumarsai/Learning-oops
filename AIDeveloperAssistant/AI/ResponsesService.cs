using OpenAI.Assistants;
using OpenAI.Responses;

namespace AIDeveloperAssistant.AI;

internal class ResponsesService
{

    #pragma warning disable OPENAI001

    private readonly ResponsesClient _client;
    private readonly string _deploymentName;

    public ResponsesService(ResponsesClient client, string deploymentName)
    {
        _client = client;
        _deploymentName = deploymentName;
    }

    public async Task<string?> SendStreamingAsync(
        string userInput,
        string instructions,
        string? previousResponseId, List<ResponseTool>? tools =  null)
    {
        CreateResponseOptions options = new()
        {
            Model = _deploymentName,
            Instructions = instructions,
            PreviousResponseId = previousResponseId,
            StreamingEnabled = true,
            InputItems =
            {
                ResponseItem.CreateUserMessageItem(userInput)
            }
        };
        // Tools is a read-only collection property — add items to it after construction.
        if (tools != null)
        {
            foreach (var tool in tools)
            {
                options.Tools.Add(tool);
            }
        }

        Console.WriteLine();
        Console.Write("[ASSISTANT]: ");

        string? newResponseId = previousResponseId;

        await foreach (StreamingResponseUpdate update
            in _client.CreateResponseStreamingAsync(options))
        {
            if (update is StreamingResponseOutputTextDeltaUpdate textDelta)
            {
                Console.Write(textDelta.Delta);
            }
            else if (update is StreamingResponseCompletedUpdate completed)
            {
                newResponseId = completed.Response.Id;
            }
        }

   //     await foreach (StreamingResponseUpdate update
   //in _client.CreateResponseStreamingAsync(options))
   //     {
   //         Console.WriteLine(update.GetType().FullName);

   //         if (update is StreamingResponseOutputTextDeltaUpdate textDelta)
   //         {
   //             Console.Write(textDelta.Delta);
   //         }
   //         else if (update is StreamingResponseCompletedUpdate completed)
   //         {
   //             //_previousResponseId = completed.Response.Id;
   //         }
   //     }

        Console.WriteLine();

        return newResponseId;
    }
}
