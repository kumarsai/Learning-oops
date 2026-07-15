using AIDeveloperAssistant.AI;
using AIDeveloperAssistant.Core;

namespace AIDeveloperAssistant.Features.FileSearch
{
    internal class FileSearchFeature : IFeature
    {
        private readonly ResponsesService _responsesService;
        private readonly FileSearchIndexService _indexService;
        private readonly string _instructions;
        private string? _previousResponseId;

        public FileSearchFeature(ResponsesService responsesService)
        {
            _responsesService = responsesService;
            _indexService = new FileSearchIndexService();

            string instructionPath = Path.GetFullPath(
                "Features\\FileSearch\\FileSearchInstructions.txt");

            _instructions = File.ReadAllText(instructionPath);
        }

        public async Task RunAsync()
        {
            Console.Clear();

            Console.WriteLine("AI File Search / RAG");
            Console.WriteLine("--------------------");

            string pdfPath = Path.GetFullPath("Features\\FileSearch\\Employment_contract.pdf");

            if (!File.Exists(pdfPath))
            {
                Console.WriteLine($"PDF not found: {pdfPath}");
                return;
            }

            Console.WriteLine("Preparing vector store...");

            string vectorStoreId =
                await _indexService.GetOrCreateVectorStoreAsync(pdfPath);

            Console.WriteLine($"Vector Store Ready: {vectorStoreId}");
            Console.WriteLine();
            Console.WriteLine("Ask questions about the PDF.");
            Console.WriteLine("Type 'exit' to stop.");
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

                await SendMessageAsync(userInput, vectorStoreId);
            }
        }

        private async Task SendMessageAsync(string userInput, string vectorStoreId)
        {
            _previousResponseId =
                await _responsesService.SendStreamingAsync(
                    userInput,
                    _instructions,
                    _previousResponseId,
#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                    new List<OpenAI.Responses.ResponseTool>
                    {
                        OpenAI.Responses.ResponseTool.CreateFileSearchTool(new List<string>{ vectorStoreId })
                    });

            Console.WriteLine();
        }
    }
}
