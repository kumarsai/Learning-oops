namespace AIDeveloperAssistant.Core;

internal static class AppMenu
{
    public static MenuOption ShowMainMenu()
    {
        Console.Clear();

        Console.WriteLine("=========================================");
        Console.WriteLine("      AI Developer Assistant");
        Console.WriteLine("=========================================");
        Console.WriteLine();
        Console.WriteLine("1. Interview Coach");
        Console.WriteLine("2. Code Reviewer");
        Console.WriteLine("3. SQL Assistant");
        Console.WriteLine("4. Web Search");
        Console.WriteLine("5. File Search");
        Console.WriteLine("6. Image Generator");
        Console.WriteLine("7. CSV Analyzer");
        Console.WriteLine("8. AI Code Interpreter");
        Console.WriteLine("9. Smart Assistant"); 
        Console.WriteLine("10. Employee Assistant"); 
        Console.WriteLine("0. Exit");
        Console.WriteLine();

        Console.Write("Select an option: ");

        Enum.TryParse(Console.ReadLine(), out MenuOption choice);

        return choice;
    }
}