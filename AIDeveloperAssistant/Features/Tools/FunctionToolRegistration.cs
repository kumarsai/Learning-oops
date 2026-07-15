using OpenAI.Responses;

namespace AIDeveloperAssistant.Features.Tools;

#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
internal sealed class FunctionToolRegistration
{
    public required string FunctionName { get; init; }

    public required ResponseTool ToolDefinition { get; init; }

    public required Func<FunctionCallResponseItem, Task<string>>
        ExecuteAsync
    { get; init; }
}
