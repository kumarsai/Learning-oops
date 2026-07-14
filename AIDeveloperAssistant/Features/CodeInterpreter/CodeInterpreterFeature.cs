using AIDeveloperAssistant.AI;
using OpenAI.Responses;

namespace AIDeveloperAssistant.Features.CodeInterpreter;

internal class CodeInterpreterFeature : IFeature
{
    private readonly ResponsesService _responsesService;
    private readonly string _instructions;
    private string? _previousResponseId;

    public CodeInterpreterFeature(ResponsesService responsesService)
    {
        _responsesService = responsesService;
        string instructionPath = Path.GetFullPath(
            "Features\\CodeInterpreter\\CodeInterpreterInstructions.txt");
        _instructions = File.ReadAllText(instructionPath);
    }

    public async Task RunAsync()
    {
        Console.Clear();
        Console.WriteLine("AI Code Interpreter");
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
        }
        string code = string.Join(Environment.NewLine, lines);

        await SendMessageAsync(code);
    }

    private async Task SendMessageAsync(string code)
    {
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        _previousResponseId =
            await _responsesService.SendStreamingAsync(
                code,
                _instructions,
                _previousResponseId,
                new List<ResponseTool>{
                    ResponseTool.CreateCodeInterpreterTool(
                        new CodeInterpreterToolContainer(
                            CodeInterpreterToolContainerConfiguration.CreateAutomaticContainerConfiguration([])
                        )
                    )
                });
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        Console.WriteLine();
    }
}
