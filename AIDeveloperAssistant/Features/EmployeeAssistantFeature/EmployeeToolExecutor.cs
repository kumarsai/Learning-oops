using AIDeveloperAssistant.Services;
using System.Text.Json;

namespace AIDeveloperAssistant.Features.EmployeeAssistant;

internal sealed class EmployeeToolExecutor
{
    private readonly EmployeeFunctionClient _employeeClient;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public EmployeeToolExecutor(
        EmployeeFunctionClient employeeClient)
    {
        _employeeClient = employeeClient
            ?? throw new ArgumentNullException(nameof(employeeClient));
    }

    public async Task<string> ExecuteAsync(
        string functionName,
        string functionArguments,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using JsonDocument argumentsDocument =
                JsonDocument.Parse(functionArguments);

            JsonElement arguments = argumentsDocument.RootElement;

            return functionName switch
            {
                "get_employee" =>
                    await GetEmployeeAsync(
                        ReadRequiredInt(arguments, "employeeId"),
                        cancellationToken),

                "search_employees" =>
                    await SearchEmployeesAsync(
                        ReadRequiredString(arguments, "name"),
                        cancellationToken),

                "get_leave_balance" =>
                    await GetLeaveBalanceAsync(
                        ReadRequiredInt(arguments, "employeeId"),
                        cancellationToken),

                "get_employee_projects" =>
                    await GetEmployeeProjectsAsync(
                        ReadRequiredInt(arguments, "employeeId"),
                        cancellationToken),

                "get_employees_by_department" =>
                    await GetEmployeesByDepartmentAsync(
                        ReadRequiredString(arguments, "department"),
                        cancellationToken),

                _ => Serialize(new
                {
                    success = false,
                    error = $"Unknown function: {functionName}"
                })
            };
        }
        catch (JsonException exception)
        {
            return Serialize(new
            {
                success = false,
                error = "The function arguments were invalid JSON.",
                details = exception.Message
            });
        }
        catch (Exception exception)
        {
            return Serialize(new
            {
                success = false,
                error = exception.Message
            });
        }
    }

    private async Task<string> GetEmployeeAsync(
        int employeeId,
        CancellationToken cancellationToken)
    {
        var employee =
            await _employeeClient.GetEmployeeAsync(
                employeeId,
                cancellationToken);

        return employee is null
            ? Serialize(new
            {
                success = false,
                error = $"Employee {employeeId} was not found."
            })
            : Serialize(new
            {
                success = true,
                employee
            });
    }

    private async Task<string> SearchEmployeesAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var employees =
            await _employeeClient.SearchEmployeesAsync(
                name,
                cancellationToken);

        return Serialize(new
        {
            success = true,
            count = employees.Count,
            employees
        });
    }

    private async Task<string> GetLeaveBalanceAsync(
        int employeeId,
        CancellationToken cancellationToken)
    {
        var result =
            await _employeeClient.GetLeaveBalanceAsync(
                employeeId,
                cancellationToken);

        return result is null
            ? Serialize(new
            {
                success = false,
                error = $"Employee {employeeId} was not found."
            })
            : Serialize(new
            {
                success = true,
                result.EmployeeId,
                result.LeaveBalance
            });
    }

    private async Task<string> GetEmployeeProjectsAsync(
        int employeeId,
        CancellationToken cancellationToken)
    {
        var result =
            await _employeeClient.GetProjectsAsync(
                employeeId,
                cancellationToken);

        return result is null
            ? Serialize(new
            {
                success = false,
                error = $"Employee {employeeId} was not found."
            })
            : Serialize(new
            {
                success = true,
                result.EmployeeId,
                result.Projects
            });
    }

    private async Task<string> GetEmployeesByDepartmentAsync(
        string department,
        CancellationToken cancellationToken)
    {
        var employees =
            await _employeeClient.GetEmployeesByDepartmentAsync(
                department,
                cancellationToken);

        return Serialize(new
        {
            success = true,
            department,
            count = employees.Count,
            employees
        });
    }

    private string Serialize(object value)
    {
        return JsonSerializer.Serialize(value, _jsonOptions);
    }

    private static int ReadRequiredInt(
        JsonElement arguments,
        string propertyName)
    {
        if (!arguments.TryGetProperty(
                propertyName,
                out JsonElement property) ||
            !property.TryGetInt32(out int value))
        {
            throw new ArgumentException(
                $"Required integer argument '{propertyName}' is missing.");
        }

        return value;
    }

    private static string ReadRequiredString(
        JsonElement arguments,
        string propertyName)
    {
        if (!arguments.TryGetProperty(
                propertyName,
                out JsonElement property))
        {
            throw new ArgumentException(
                $"Required string argument '{propertyName}' is missing.");
        }

        string? value = property.GetString();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"Argument '{propertyName}' cannot be empty.");
        }

        return value;
    }
}