using Azure;
using Azure.AI.Agents.Persistent;
using Azure.AI.Projects;
using Azure.Identity;
//using OpenAI.Assistants;
using System.Text.Json;
using MessageRole = Azure.AI.Agents.Persistent.MessageRole;

namespace EmployeeAssistant.ConsoleApp.Agent;

#pragma warning disable OPENAI001

public sealed class EmployeeAgentService
{
    private readonly PersistentAgentsClient _agentsClient;
    private readonly EmployeeFunctionClient _functionClient;
    private readonly string _modelDeploymentName;
    // Persist agent and thread so the assistant retains conversation state
    private PersistentAgent? _agent;
    private string? _threadId;
    private readonly System.Threading.SemaphoreSlim _initSemaphore = new(1, 1);

    public EmployeeAgentService(
        string projectEndpoint,
        string modelDeploymentName,
        EmployeeFunctionClient functionClient)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectEndpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(modelDeploymentName);

        AIProjectClient projectClient = new(
            endpoint: new Uri(projectEndpoint),
            tokenProvider: new DefaultAzureCredential());

        _agentsClient = projectClient.GetPersistentAgentsClient();
        _functionClient = functionClient;
        _modelDeploymentName = modelDeploymentName;
    }

    public async Task<string> AskAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);

        // _agent and _threadId are guaranteed to be non-null after initialization
        await _agentsClient.Messages.CreateMessageAsync(
            threadId: _threadId!,
            role: MessageRole.User,
            content: question,
            cancellationToken: cancellationToken);

        // create a run for the existing thread and agent so the assistant keeps context
        var run = await _agentsClient.Runs.CreateRunAsync(
            _threadId!,
            _agent!.Id,
            cancellationToken: cancellationToken);

        //Response createRunResponse =
        //    await _agentsClient.Runs.CreateRunAsync(
        //        threadId: thread.Id,
        //        content: createRunContent,
        //        context: new RequestContext
        //        {
        //            CancellationToken = cancellationToken
        //        });

        //ThreadRun run = JsonSerializer.Deserialize<ThreadRun>(
        //    createRunResponse.Content.ToString(),
        //    new JsonSerializerOptions
        //    {
        //        PropertyNameCaseInsensitive = true
        //    }) ?? throw new InvalidOperationException(
        //        "Unable to deserialize the agent run.");

        var run2 = await ProcessRunAsync(
            _threadId!,
            run,
            cancellationToken);

        if (run2.Status != Azure.AI.Agents.Persistent.RunStatus.Completed)
        {
            throw new InvalidOperationException(
                $"Agent run ended with status: {run2.Status}");
        }

        return await GetLatestAssistantMessageAsync(
            _threadId!,
            cancellationToken);
    }

    /// <summary>
    /// Ensure a single agent and thread are created and reused so the assistant retains
    /// conversation state across AskAsync calls.
    /// </summary>
    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_agent is not null && _threadId is not null)
        {
            return;
        }

        await _initSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_agent is null)
            {
                _agent = await CreateAgentAsync(cancellationToken);
            }

            if (_threadId is null)
            {
                var thread = await _agentsClient.Threads.CreateThreadAsync(
                    cancellationToken: cancellationToken);
                _threadId = thread?.Value.Id;
            }
        }
        finally
        {
            _initSemaphore.Release();
        }
    }

    /// <summary>
    /// Shutdown and delete the persistent agent. Call when the service is no longer needed.
    /// </summary>
    public async Task ShutdownAsync(CancellationToken cancellationToken = default)
    {
        if (_agent is null)
        {
            return;
        }

        try
        {
            await _agentsClient.Administration.DeleteAgentAsync(
                _agent.Id,
                cancellationToken);
        }
        finally
        {
            _agent = null;
            _threadId = null;
        }
    }

    private async Task<PersistentAgent> CreateAgentAsync(
        CancellationToken cancellationToken)
    {
        Azure.AI.Agents.Persistent.FunctionToolDefinition getEmployeeTool = new(
            name: "get_employee",
            description: "Gets an employee using the employee ID.",
            parameters: BinaryData.FromObjectAsJson(new
            {
                type = "object",
                properties = new
                {
                    employeeId = new
                    {
                        type = "integer",
                        description = "The unique employee ID."
                    }
                },
                required = new[] { "employeeId" },
                additionalProperties = false
            }));

        FunctionToolDefinition searchEmployeesTool = new(
            name: "search_employees",
            description: "Searches employees by name or department.",
            parameters: BinaryData.FromObjectAsJson(new
            {
                type = "object",
                properties = new
                {
                    searchText = new
                    {
                        type = "string",
                        description = "Employee name or department."
                    }
                },
                required = new[] { "searchText" },
                additionalProperties = false
            }));

        Azure.AI.Agents.Persistent.FunctionToolDefinition createEmployeeTool = new(
            name: "create_employee",
            description: "Creates a new employee.",
            parameters: BinaryData.FromObjectAsJson(new
            {
                type = "object",
                properties = new
                {
                    name = new { type = "string" },
                    email = new { type = "string" },
                    department = new { type = "string" }
                },
                required = new[] { "name", "email", "department" },
                additionalProperties = false
            }));

        Azure.AI.Agents.Persistent.FunctionToolDefinition updateEmployeeTool = new(
            name: "update_employee",
            description: "Updates an existing employee.",
            parameters: BinaryData.FromObjectAsJson(new
            {
                type = "object",
                properties = new
                {
                    employeeId = new { type = "integer" },
                    name = new { type = "string" },
                    email = new { type = "string" },
                    department = new { type = "string" }
                },
                required = new[] { "employeeId" },
                additionalProperties = false
            }));

        Azure.AI.Agents.Persistent.FunctionToolDefinition deleteEmployeeTool = new(
            name: "delete_employee",
            description: "Deletes an employee using the employee ID.",
            parameters: BinaryData.FromObjectAsJson(new
            {
                type = "object",
                properties = new
                {
                    employeeId = new
                    {
                        type = "integer",
                        description = "The employee ID to delete."
                    }
                },
                required = new[] { "employeeId" },
                additionalProperties = false
            }));

        Response<PersistentAgent> response =
            await _agentsClient.Administration.CreateAgentAsync(
                model: _modelDeploymentName,
                name: "employee-assistant",
                instructions:
                """
                You are an Employee Assistant.

                Use the available tools for all employee operations.
                Never invent employee information.
                Ask for confirmation before deleting an employee.
                Give short and clear responses.
                """,
                tools:
                [
                    getEmployeeTool,
                    searchEmployeesTool,
                    createEmployeeTool,
                    updateEmployeeTool,
                    deleteEmployeeTool
                ],
                cancellationToken: cancellationToken);

        return response.Value;
    }

    private async Task<ThreadRun> ProcessRunAsync(
        string threadId,
        ThreadRun run,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Use equality checks instead of a switch because the
            // RunStatus members are not compile-time constants in the
            // SDK (they are static readonly strings). A switch's case
            // labels must be compile-time constants, so use if/else.
            if (run.Status == RunStatus.Queued || run.Status == RunStatus.InProgress)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(1),
                    cancellationToken);

                run = await _agentsClient.Runs.GetRunAsync(
                    threadId,
                    run.Id,
                    cancellationToken);

                continue;
            }

            if (run.Status == RunStatus.RequiresAction)
            {
                var outputs = new List<ToolOutput>();

                foreach (RequiredAction requiredAction in run.RequiredActions)
                {
                    if (requiredAction is not RequiredFunctionToolCall functionCall)
                    {
                        throw new InvalidOperationException(
                            $"Unsupported required action: {requiredAction.GetType().Name}");
                    }

                    string functionResult;

                    try
                    {
                        functionResult = await ExecuteFunctionAsync(
                            functionCall.Name,
                            functionCall.Arguments,
                            cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        // Return the function error to the agent instead
                        // of leaving the run waiting until it expires.
                        functionResult = JsonSerializer.Serialize(new
                        {
                            success = false,
                            error = ex.Message,
                            function = functionCall.Name
                        });
                    }

                    outputs.Add(new ToolOutput
                    {
                        ToolCallId = functionCall.Id,
                        Output = functionResult
                    });
                }

                if (outputs.Count == 0)
                {
                    throw new InvalidOperationException(
                        "The run requires tool outputs, but no supported function calls were found.");
                }

                Response<ThreadRun> submitResponse =
                    await _agentsClient.Runs
                        .SubmitToolOutputsToRunAsync(
                            run,
                            outputs,
                            cancellationToken);

                run = submitResponse.Value;
                continue;
            }

            if (run.Status == RunStatus.Completed ||
                run.Status == RunStatus.Failed ||
                run.Status == RunStatus.Cancelled ||
                run.Status == RunStatus.Expired)
            {
                return run;
            }

            // Fallback: wait and refresh
            await Task.Delay(
                TimeSpan.FromSeconds(1),
                cancellationToken);

            run = await _agentsClient.Runs.GetRunAsync(
                threadId,
                run.Id,
                cancellationToken);

            continue;
        }
    }
    

    private async Task<string> ExecuteFunctionAsync(
    string functionName,
    string functionArguments,
    CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(functionArguments))
        {
            functionArguments = "{}";
        }

        using JsonDocument document =
            JsonDocument.Parse(functionArguments);

        JsonElement root = document.RootElement;

        return functionName switch
        {
            "get_employee" =>
                await _functionClient.GetEmployeeAsync(
                    ReadRequiredInt(root, "employeeId"),
                    cancellationToken),

            "search_employees" =>
                await _functionClient.SearchEmployeesAsync(
                    ReadRequiredString(root, "searchText"),
                    cancellationToken),

            "create_employee" =>
                await _functionClient.CreateEmployeeAsync(
                    functionArguments,
                    cancellationToken),

            "update_employee" =>
                await _functionClient.UpdateEmployeeAsync(
                    ReadRequiredInt(root, "employeeId"),
                    functionArguments,
                    cancellationToken),

            "delete_employee" =>
                await _functionClient.DeleteEmployeeAsync(
                    ReadRequiredInt(root, "employeeId"),
                    cancellationToken),

            _ => throw new InvalidOperationException(
                $"Unknown function requested by agent: {functionName}")
        };
    }

    private static int ReadRequiredInt(
        JsonElement root,
        string propertyName)
    {
        if (!root.TryGetProperty(
                propertyName,
                out JsonElement value))
        {
            throw new InvalidOperationException(
                $"Missing required argument '{propertyName}'.");
        }

        if (value.ValueKind == JsonValueKind.Number &&
            value.TryGetInt32(out int numericValue))
        {
            return numericValue;
        }

        if (value.ValueKind == JsonValueKind.String &&
            int.TryParse(value.GetString(), out int stringValue))
        {
            return stringValue;
        }

        throw new InvalidOperationException(
            $"Argument '{propertyName}' must be an integer.");
    }

    private static string ReadRequiredString(
        JsonElement root,
        string propertyName)
    {
        if (!root.TryGetProperty(
                propertyName,
                out JsonElement value))
        {
            throw new InvalidOperationException(
                $"Missing required argument '{propertyName}'.");
        }

        string? result = value.GetString();

        if (string.IsNullOrWhiteSpace(result))
        {
            throw new InvalidOperationException(
                $"Argument '{propertyName}' cannot be empty.");
        }

        return result;
    }

    private async Task<string> GetLatestAssistantMessageAsync(
        string threadId,
        CancellationToken cancellationToken)
    {
        await foreach (PersistentThreadMessage message
                       in _agentsClient.Messages.GetMessagesAsync(
                           threadId: threadId,
                           order: ListSortOrder.Descending,
                           cancellationToken: cancellationToken))
        {
            if (message.Role != MessageRole.Agent)
            {
                continue;
            }

            foreach (MessageContent content in message.ContentItems)
            {
                if (content is MessageTextContent textContent)
                {
                    return textContent.Text;
                }
            }
        }

        return "The agent completed without returning a message.";
    }
}