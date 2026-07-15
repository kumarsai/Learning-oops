using AIDeveloperAssistant.AI;
using AIDeveloperAssistant.Features.Tools;
using OpenAI.Responses;
using static AIDeveloperAssistant.Features.SmartAssistant.FunctionCalling.CalculatorTool;

namespace AIDeveloperAssistant.Features.FunctionCalling;

internal class FunctionCallingFeature : BaseFeature, IFeature
{
    public FunctionCallingFeature(ResponsesService responsesService)
    {
        _responsesService = responsesService;

        string instructionPath = Path.GetFullPath(
              "Features\\FileSearch\\FileSearchInstructions.txt");

        _instructions = File.ReadAllText(instructionPath);
    }

    public async Task RunAsync()
    {
        Console.Clear();

        Console.WriteLine("Smart Personal Assistant");
        Console.WriteLine("------------------------");
        Console.WriteLine("Example: What is 25 multiplied by 18?");
        Console.WriteLine("Type 'exit' to stop.");
        Console.WriteLine();

        while (true)
        {
            Console.Write("You: ");
            string userInput = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(userInput) ||
                userInput.Equals(
                    "exit",
                    StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            await SendMessageAsync(userInput);

            Console.WriteLine();
        }
    }

    private async Task SendMessageAsync(string userInput)
    {
        List<FunctionToolRegistration> tools =
       [
           CalculatorToolDefinition.Create()
       ];
        _previousResponseId =
            await _responsesService.SendStreamingAsync(
                userInput,
                _instructions,
                _previousResponseId,
                functionTools: tools
                );

        Console.WriteLine();
    }
}
