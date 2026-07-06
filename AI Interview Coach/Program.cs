using OpenAI.Responses;
using System.ClientModel;
using AI_Interview_Coach;

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

var coach = new InterviewCoachService();

Console.WriteLine();
Console.WriteLine($"Starting interview for: {topic}");
Console.WriteLine("Type 'exit' to stop.");
Console.WriteLine();

string firstPrompt = $"Start mock interview. Topic: {topic}";
await coach.SendMessageAsync(firstPrompt);

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

    await coach.SendMessageAsync(userInput);
}

Console.WriteLine("Interview ended. Press any key to exit.");
Console.ReadKey();
