using AIDeveloperAssistant.AI;

namespace AIDeveloperAssistant.Features.CodeReviewer;

internal class CodeReviewerFeature : IFeature
{
    private readonly ResponsesService _responsesService;

    private readonly string _instructions;

    private string? _previousResponseId;

    public CodeReviewerFeature(ResponsesService responsesService)
    {
        _responsesService = responsesService;

        string path = Path.GetFullPath(
            "Features\\CodeReviewer\\CodeReviewerInstructions.txt");

        _instructions = File.ReadAllText(path);
    }

    public async Task RunAsync()
    {
        Console.Clear();

        Console.WriteLine("AI Code Reviewer");
        Console.WriteLine("----------------");
        Console.WriteLine("Paste your code.");
        Console.WriteLine("Type END on a new line when finished.");
        Console.WriteLine();

        List<string> lines = new();

        while (true)
        {
            string line = Console.ReadLine() ?? "";

            if (line.Equals("END", StringComparison.OrdinalIgnoreCase))
                break;

            lines.Add(line);
        }

        string code = string.Join(Environment.NewLine, lines);

        _previousResponseId = null;

        _previousResponseId =
            await _responsesService.SendStreamingAsync(
                code,
                _instructions,
                _previousResponseId);

        Console.WriteLine();
    }
}