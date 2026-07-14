using AIDeveloperAssistant.AI;
using AIDeveloperAssistant.Features;

namespace AIDeveloperAssistant.Features.InterviewCoach;

internal class InterviewCoachFeature : IFeature
{
    private readonly ResponsesService _responsesService;
    private readonly string _instructions;
    private string? _previousResponseId;

    public InterviewCoachFeature(ResponsesService responsesService)
    {
        _responsesService = responsesService;

        string instructionPath = Path.GetFullPath(
            "Features\\InterviewCoach\\InterviewCoachInstructions.txt");

        _instructions = File.ReadAllText(instructionPath);
    }

    public async Task RunAsync()
    {
        Console.WriteLine("AI Interview Coach");
        Console.WriteLine("------------------");

        Console.WriteLine("Select topic:");
        Console.WriteLine("1. .NET");
        Console.WriteLine("2. Azure");
        Console.WriteLine("3. AI-103");
        Console.WriteLine("4. SQL");
        Console.WriteLine("5. Angular");

        Console.Write("Enter choice: ");
        string choice = Console.ReadLine() ?? "";

        string topic = choice switch
        {
            "1" => ".NET",
            "2" => "Azure",
            "3" => "AI-103",
            "4" => "SQL",
            "5" => "Angular",
            _ => "AI-103"
        };

        Console.WriteLine();
        Console.WriteLine($"Starting interview for: {topic}");
        Console.WriteLine("Type 'exit' to stop.");

        _previousResponseId = null;

        await SendMessageAsync($"Start mock interview. Topic: {topic}");

        while (true)
        {
            Console.WriteLine();
            Console.Write("Your answer: ");

            string userInput = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(userInput) ||
                userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            await SendMessageAsync(userInput);
        }
    }

    private async Task SendMessageAsync(string userInput)
    {
        _previousResponseId = await _responsesService.SendStreamingAsync(
            userInput,
            _instructions,
            _previousResponseId);
    }
}