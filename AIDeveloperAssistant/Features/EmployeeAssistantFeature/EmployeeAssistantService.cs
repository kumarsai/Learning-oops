using AIDeveloperAssistant.Features.EmployeeAssistantFeature;
using OpenAI.Responses;

namespace AIDeveloperAssistant.Features.EmployeeAssistant;

internal sealed class EmployeeAssistantService
{
#pragma warning disable OPENAI001

    private const int MaximumToolRounds = 10;

    private readonly ResponsesClient _client;
    private readonly string _deploymentName;
    private readonly EmployeeToolExecutor _toolExecutor;
    private readonly IReadOnlyList<ResponseTool> _tools;

    private string? _previousResponseId;

    private const string Instructions =
        """
        You are an employee information assistant.

        Use the available employee tools whenever the user asks about:
        - employee details
        - employee names
        - leave balances
        - employee projects
        - employees in a department

        Never invent employee information.

        If the user gives an employee name but the required operation needs
        an employee ID, first call search_employees to find the employee.
        Then call the appropriate employee-ID function.

        If multiple employees match a name, ask the user to clarify.

        Keep the final response concise and easy to understand.
        """;

    public EmployeeAssistantService(
        ResponsesClient client,
        string deploymentName,
        EmployeeToolExecutor toolExecutor)
    {
        _client = client
            ?? throw new ArgumentNullException(nameof(client));

        _deploymentName = !string.IsNullOrWhiteSpace(deploymentName)
            ? deploymentName
            : throw new ArgumentException(
                "Deployment name is required.",
                nameof(deploymentName));

        _toolExecutor = toolExecutor
            ?? throw new ArgumentNullException(nameof(toolExecutor));

        _tools = EmployeeTools.Create();
    }

    public void StartNewConversation()
    {
        _previousResponseId = null;
    }

    public async Task<string> SendAsync(
        string userInput,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ResponseItem> inputItems =
        [
            ResponseItem.CreateUserMessageItem(userInput)
        ];

        for (int round = 1;
             round <= MaximumToolRounds;
             round++)
        {
            CreateResponseOptions options =
                CreateOptions(inputItems);

            ResponseResult response =
                await _client.CreateResponseAsync(
                    options,
                    cancellationToken);

            _previousResponseId = response.Id;

            List<FunctionCallResponseItem> functionCalls =
                response.OutputItems
                    .OfType<FunctionCallResponseItem>()
                    .ToList();

            if (functionCalls.Count == 0)
            {
                return response.GetOutputText();
            }

            List<ResponseItem> functionOutputs = [];

            foreach (FunctionCallResponseItem functionCall
                     in functionCalls)
            {
                Console.WriteLine();
                Console.WriteLine(
                    $"[TOOL REQUEST] {functionCall.FunctionName}");

                Console.WriteLine(
                    $"[ARGUMENTS] {functionCall.FunctionArguments}");

                string functionResult =
                    await _toolExecutor.ExecuteAsync(
                        functionCall.FunctionName,
                        functionCall.FunctionArguments.ToString(),
                        cancellationToken);

                Console.WriteLine(
                    $"[TOOL RESULT] {functionResult}");

                functionOutputs.Add(
                    ResponseItem.CreateFunctionCallOutputItem(
                        functionCall.CallId,
                        functionResult));
            }

            // The next request contains function_call_output items.
            // PreviousResponseId links them to the model's function calls.
            inputItems = functionOutputs;
        }

        throw new InvalidOperationException(
            $"The model exceeded {MaximumToolRounds} tool-call rounds.");
    }

    private CreateResponseOptions CreateOptions(
        IReadOnlyList<ResponseItem> inputItems)
    {
        CreateResponseOptions options = new()
        {
            Model = _deploymentName,
            Instructions = Instructions,
            PreviousResponseId = _previousResponseId
        };

        foreach (ResponseTool tool in _tools)
        {
            options.Tools.Add(tool);
        }

        foreach (ResponseItem item in inputItems)
        {
            options.InputItems.Add(item);
        }

        return options;
    }
}