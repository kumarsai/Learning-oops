using System.Net.Http.Json;
using System.Text;

namespace EmployeeAssistant.ConsoleApp.Agent;

public sealed class EmployeeFunctionClient
{
    private readonly HttpClient _httpClient;

    public EmployeeFunctionClient(
        HttpClient httpClient,
        string functionBaseUrl,
        string? functionKey = null)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(functionBaseUrl);

        if (!string.IsNullOrWhiteSpace(functionKey))
        {
            _httpClient.DefaultRequestHeaders.Add(
                "x-functions-key",
                functionKey);
        }
    }

    public Task<string> GetEmployeeAsync(
        int employeeId,
        CancellationToken cancellationToken)
    {
        return SendAsync(
            HttpMethod.Get,
            $"api/employees/{employeeId}",
            null,
            cancellationToken);
    }

    public Task<string> SearchEmployeesAsync(
        string searchText,
        CancellationToken cancellationToken)
    {
        string encodedText = Uri.EscapeDataString(searchText);

        return SendAsync(
            HttpMethod.Get,
            $"api/employees?searchText={encodedText}",
            null,
            cancellationToken);
    }

    public Task<string> CreateEmployeeAsync(
        string json,
        CancellationToken cancellationToken)
    {
        return SendAsync(
            HttpMethod.Post,
            "api/employees",
            json,
            cancellationToken);
    }

    public Task<string> UpdateEmployeeAsync(
        int employeeId,
        string json,
        CancellationToken cancellationToken)
    {
        return SendAsync(
            HttpMethod.Put,
            $"api/employees/{employeeId}",
            json,
            cancellationToken);
    }

    public Task<string> DeleteEmployeeAsync(
        int employeeId,
        CancellationToken cancellationToken)
    {
        return SendAsync(
            HttpMethod.Delete,
            $"api/employees/{employeeId}",
            null,
            cancellationToken);
    }

    private async Task<string> SendAsync(
        HttpMethod method,
        string url,
        string? json,
        CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(method, url);

        if (json is not null)
        {
            request.Content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");
        }

        using HttpResponseMessage response =
            await _httpClient.SendAsync(request, cancellationToken);

        string responseBody =
            await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return $$"""
            {
              "success": false,
              "statusCode": {{(int)response.StatusCode}},
              "error": {{System.Text.Json.JsonSerializer.Serialize(responseBody)}}
            }
            """;
        }

        return responseBody;
    }
}