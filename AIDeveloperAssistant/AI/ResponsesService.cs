using AIDeveloperAssistant.Features.Tools;
using OpenAI.Responses;

namespace AIDeveloperAssistant.AI;

internal class ResponsesService
{

#pragma warning disable OPENAI001

    public readonly ResponsesClient _client;
    public readonly string _deploymentName;

    public ResponsesService(ResponsesClient client, string deploymentName)
    {
        _client = client;
        _deploymentName = deploymentName;
    }

    public async Task<string?> SendStreamingAsync2(
        string userInput,
        string instructions,
        string? previousResponseId, List<ResponseTool>? tools = null)
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

        Console.WriteLine();

        return newResponseId;
    }


    public async Task<string?> SendStreamingAsync(
        string userInput,
        string instructions,
        string? previousResponseId,
        List<ResponseTool>? builtInTools = null,
        List<FunctionToolRegistration>? functionTools = null)
    {
        string? currentResponseId = previousResponseId;

        List<ResponseItem> nextInputItems =
        [
            ResponseItem.CreateUserMessageItem(userInput)
        ];

        Console.WriteLine();
        Console.Write("[ASSISTANT]: ");

        while (true)
        {
            CreateResponseOptions options = new()
            {
                Model = _deploymentName,
                Instructions = instructions,
                PreviousResponseId = currentResponseId,
                StreamingEnabled = true
            };

            foreach (ResponseItem inputItem in nextInputItems)
            {
                options.InputItems.Add(inputItem);
            }

            if (functionTools is not null)
            {
                foreach (FunctionToolRegistration functionTool
                         in functionTools)
                {
                    options.Tools.Add(
                        functionTool.ToolDefinition);
                }
            }

            if (builtInTools is not null)
            {
                foreach (ResponseTool builtInTool in builtInTools)
                {
                    options.Tools.Add(builtInTool);
                }
            }

            ResponseResult? completedResponse = null;

            await foreach (StreamingResponseUpdate update
                in _client.CreateResponseStreamingAsync(options))
            {
                if (update is
                    StreamingResponseOutputTextDeltaUpdate textDelta)
                {
                    Console.Write(textDelta.Delta);
                }
                else if (update is
                    StreamingResponseCompletedUpdate completed)
                {
                    completedResponse = completed.Response;
                    currentResponseId = completed.Response.Id;
                }
            }

            if (completedResponse is null)
            {
                throw new InvalidOperationException(
                    "The response did not complete.");
            }

            List<FunctionCallResponseItem> functionCalls =
                completedResponse.OutputItems
                    .OfType<FunctionCallResponseItem>()
                    .ToList();

            // The model returned a normal text answer.
            if (functionCalls.Count == 0)
            {
                Console.WriteLine();
                return currentResponseId;
            }

            /*
             * The next request should contain only function-call outputs.
             * PreviousResponseId links it to the model's function calls.
             */
            nextInputItems = [];

            foreach (FunctionCallResponseItem functionCall
                     in functionCalls)
            {
                FunctionToolRegistration registration =
                    FindFunctionTool(
                        functionCall.FunctionName,
                        functionTools);

                string toolOutput =
                    await registration.ExecuteAsync(functionCall);

                nextInputItems.Add(
                    ResponseItem.CreateFunctionCallOutputItem(
                        functionCall.CallId,
                        toolOutput));
            }
        }
    }

    private static FunctionToolRegistration FindFunctionTool(
        string functionName,
        IReadOnlyCollection<FunctionToolRegistration>? tools)
    {
        FunctionToolRegistration? tool =
            tools?.FirstOrDefault(
                item => item.FunctionName.Equals(
                    functionName,
                    StringComparison.OrdinalIgnoreCase));

        return tool
            ?? throw new NotImplementedException(
                $"No C# implementation is registered for " +
                $"the function '{functionName}'.");
    }
}
