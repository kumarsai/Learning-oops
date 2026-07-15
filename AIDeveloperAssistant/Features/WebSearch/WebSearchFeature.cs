using AIDeveloperAssistant.AI;

namespace AIDeveloperAssistant.Features.WebSearch;

internal class WebSearchFeature : IFeature
{
    private readonly ResponsesService _responsesService;
    private readonly string _instructions;
    private string? _previousResponseId;

    public WebSearchFeature(ResponsesService responsesService)
    {
        _responsesService = responsesService;
        string instructionPath = Path.GetFullPath(
            "Features\\WebSearch\\WebSearchInstructions.txt");
        _instructions = File.ReadAllText(instructionPath);
    }

    public async Task RunAsync()
    {
        Console.Clear();
        Console.WriteLine("AI Web Search");
        Console.WriteLine("-------------------");
        Console.WriteLine("Paste your code.");
        Console.WriteLine("Type END on a new line when finished.");
        Console.WriteLine();

        _previousResponseId = null;

        List<string> lines = new();
        while (true)
        {
            string line = Console.ReadLine() ?? "";
            if (line.Equals("END", StringComparison.OrdinalIgnoreCase))
                break;
            lines.Add(line);
            string userInput = string.Join(Environment.NewLine, lines);

            await SendMessageAsync(userInput);
        }
    }

    private async Task SendMessageAsync(string userInput)
    {
        _previousResponseId =
            await _responsesService.SendStreamingAsync(
                userInput,
                _instructions,
                _previousResponseId,
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                new List<OpenAI.Responses.ResponseTool>
                {
                    OpenAI.Responses.ResponseTool.CreateWebSearchTool()
                });

        Console.WriteLine();
    }

}
