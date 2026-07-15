using Functions.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Functions.Functions;

public sealed class EmployeeFunctions
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<EmployeeFunctions> _logger;

    public EmployeeFunctions(
        IEmployeeService employeeService,
        ILogger<EmployeeFunctions> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    [Function("GetEmployee")]
    public async Task<HttpResponseData> GetEmployeeAsync(
        [HttpTrigger(
            AuthorizationLevel.Function,
            "get",
            Route = "employees/{employeeId:int}")]
        HttpRequestData request,
        int employeeId)
    {
        _logger.LogInformation(
            "Getting employee {EmployeeId}.",
            employeeId);

        var employee =
            await _employeeService.GetByIdAsync(employeeId);

        if (employee is null)
        {
            HttpResponseData notFound =
                request.CreateResponse(HttpStatusCode.NotFound);

            await notFound.WriteAsJsonAsync(new
            {
                error = $"Employee {employeeId} was not found."
            });

            return notFound;
        }

        HttpResponseData response =
            request.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(employee);

        return response;
    }

    [Function("SearchEmployees")]
    public async Task<HttpResponseData> SearchEmployeesAsync(
        [HttpTrigger(
            AuthorizationLevel.Function,
            "get",
            Route = "employees/search")]
        HttpRequestData request)
    {
        string? name = GetQueryValue(request, "name");

        if (string.IsNullOrWhiteSpace(name))
        {
            return await CreateBadRequestAsync(
                request,
                "Query parameter 'name' is required.");
        }

        var employees =
            await _employeeService.SearchByNameAsync(name);

        HttpResponseData response =
            request.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(employees);

        return response;
    }

    [Function("GetEmployeesByDepartment")]
    public async Task<HttpResponseData> GetEmployeesByDepartmentAsync(
        [HttpTrigger(
            AuthorizationLevel.Function,
            "get",
            Route = "departments/{department}/employees")]
        HttpRequestData request,
        string department)
    {
        var employees =
            await _employeeService.GetByDepartmentAsync(department);

        HttpResponseData response =
            request.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(employees);

        return response;
    }

    [Function("GetLeaveBalance")]
    public async Task<HttpResponseData> GetLeaveBalanceAsync(
        [HttpTrigger(
            AuthorizationLevel.Function,
            "get",
            Route = "employees/{employeeId:int}/leave-balance")]
        HttpRequestData request,
        int employeeId)
    {
        int? leaveBalance =
            await _employeeService.GetLeaveBalanceAsync(employeeId);

        if (leaveBalance is null)
        {
            HttpResponseData notFound =
                request.CreateResponse(HttpStatusCode.NotFound);

            await notFound.WriteAsJsonAsync(new
            {
                error = $"Employee {employeeId} was not found."
            });

            return notFound;
        }

        HttpResponseData response =
            request.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(new
        {
            employeeId,
            leaveBalance
        });

        return response;
    }

    [Function("GetEmployeeProjects")]
    public async Task<HttpResponseData> GetEmployeeProjectsAsync(
        [HttpTrigger(
            AuthorizationLevel.Function,
            "get",
            Route = "employees/{employeeId:int}/projects")]
        HttpRequestData request,
        int employeeId)
    {
        var projects =
            await _employeeService.GetProjectsAsync(employeeId);

        if (projects is null)
        {
            HttpResponseData notFound =
                request.CreateResponse(HttpStatusCode.NotFound);

            await notFound.WriteAsJsonAsync(new
            {
                error = $"Employee {employeeId} was not found."
            });

            return notFound;
        }

        HttpResponseData response =
            request.CreateResponse(HttpStatusCode.OK);

        await response.WriteAsJsonAsync(new
        {
            employeeId,
            projects
        });

        return response;
    }

    private static string? GetQueryValue(
        HttpRequestData request,
        string name)
    {
        return System.Web.HttpUtility
            .ParseQueryString(request.Url.Query)[name];
    }

    private static async Task<HttpResponseData> CreateBadRequestAsync(
        HttpRequestData request,
        string message)
    {
        HttpResponseData response =
            request.CreateResponse(HttpStatusCode.BadRequest);

        await response.WriteAsJsonAsync(new
        {
            error = message
        });

        return response;
    }
}