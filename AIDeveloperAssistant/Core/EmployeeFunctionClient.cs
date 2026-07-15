using AIDeveloperAssistant.Core;
using AIDeveloperAssistant.Models;
using System.Net;
using System.Net.Http.Json;

namespace AIDeveloperAssistant.Services;

internal sealed class EmployeeFunctionClient
{
    private readonly HttpClient _httpClient;

    public EmployeeFunctionClient()
    {
        var appConfig = AppConfig.Instance;
        string baseUrl = appConfig.EmployeeFunctionBaseUrl;
        string functionKey = appConfig.EmployeeFunctionBaseUrl;


        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new ArgumentException(
                "Employee Function base URL is required.",
                nameof(baseUrl));
        }

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(
                baseUrl.EndsWith('/')
                    ? baseUrl
                    : $"{baseUrl}/")
        };

        if (!string.IsNullOrWhiteSpace(functionKey))
        {
            _httpClient.DefaultRequestHeaders.Add(
                "x-functions-key",
                functionKey);
        }
    }

    public async Task<Employee?> GetEmployeeAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"employees/{employeeId}",
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response);

        return await response.Content.ReadFromJsonAsync<Employee>(
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> SearchEmployeesAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        string encodedName = Uri.EscapeDataString(name);

        using HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"employees/search?name={encodedName}",
                cancellationToken);

        await EnsureSuccessAsync(response);

        return await response.Content
                   .ReadFromJsonAsync<List<Employee>>(
                       cancellationToken: cancellationToken)
               ?? [];
    }

    public async Task<IReadOnlyList<Employee>> GetEmployeesByDepartmentAsync(
        string department,
        CancellationToken cancellationToken = default)
    {
        string encodedDepartment =
            Uri.EscapeDataString(department);

        using HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"departments/{encodedDepartment}/employees",
                cancellationToken);

        await EnsureSuccessAsync(response);

        return await response.Content
                   .ReadFromJsonAsync<List<Employee>>(
                       cancellationToken: cancellationToken)
               ?? [];
    }

    public async Task<LeaveBalanceResult?> GetLeaveBalanceAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"employees/{employeeId}/leave-balance",
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response);

        return await response.Content
            .ReadFromJsonAsync<LeaveBalanceResult>(
                cancellationToken: cancellationToken);
    }

    public async Task<EmployeeProjectsResult?> GetProjectsAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response =
            await _httpClient.GetAsync(
                $"employees/{employeeId}/projects",
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await EnsureSuccessAsync(response);

        return await response.Content
            .ReadFromJsonAsync<EmployeeProjectsResult>(
                cancellationToken: cancellationToken);
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string error =
            await response.Content.ReadAsStringAsync();

        throw new HttpRequestException(
            $"Employee Function returned " +
            $"{(int)response.StatusCode} " +
            $"{response.ReasonPhrase}. Response: {error}");
    }
}