namespace AIDeveloperAssistant.Features.EmployeeAssistant;

internal sealed class EmployeeAssistantFeature1 : IFeature
{
    private readonly EmployeeAssistantService _assistantService;

    public EmployeeAssistantFeature1(
        EmployeeAssistantService assistantService)
    {
        _assistantService = assistantService;
    }

    public async Task RunAsync()
    {
        Console.Clear();

        Console.WriteLine("=========================================");
        Console.WriteLine("          Employee Assistant");
        Console.WriteLine("=========================================");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("- Show employee 1001");
        Console.WriteLine("- How much leave does Sai have?");
        Console.WriteLine("- Which projects is employee 1005 working on?");
        Console.WriteLine("- Show all Engineering employees");
        Console.WriteLine();
        Console.WriteLine("Type 'exit' to return to the main menu.");
        Console.WriteLine();

        _assistantService.StartNewConversation();

        while (true)
        {
            Console.Write("[USER]: ");

            string input = Console.ReadLine() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            if (input.Equals(
                    "exit",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                string answer =
                    await _assistantService.SendAsync(input);

                Console.WriteLine();
                Console.WriteLine($"[ASSISTANT]: {answer}");
                Console.WriteLine();
            }
            catch (Exception exception)
            {
                Console.WriteLine();
                Console.WriteLine(
                    $"[ERROR]: {exception.Message}");
                Console.WriteLine();
            }
        }
    }
}