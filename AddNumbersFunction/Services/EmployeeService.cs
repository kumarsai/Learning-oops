using Functions.Models;
using Functions.Services;
using System.Text.Json;

namespace Functions.Services;

public sealed class EmployeeService : IEmployeeService
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _serializerOptions;

    public EmployeeService()
    {
        _filePath = Path.Combine(
            AppContext.BaseDirectory,
            "Data",
            "employees.json");

        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<Employee?> GetByIdAsync(int employeeId)
    {
        IReadOnlyList<Employee> employees = await LoadEmployeesAsync();

        return employees.FirstOrDefault(
            employee => employee.Id == employeeId);
    }

    public async Task<IReadOnlyList<Employee>> SearchByNameAsync(string name)
    {
        IReadOnlyList<Employee> employees = await LoadEmployeesAsync();

        return employees
            .Where(employee =>
                employee.Name.Contains(
                    name,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<IReadOnlyList<Employee>> GetByDepartmentAsync(
        string department)
    {
        IReadOnlyList<Employee> employees = await LoadEmployeesAsync();

        return employees
            .Where(employee =>
                employee.Department.Equals(
                    department,
                    StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<int?> GetLeaveBalanceAsync(int employeeId)
    {
        Employee? employee = await GetByIdAsync(employeeId);

        return employee?.LeaveBalance;
    }

    public async Task<IReadOnlyList<string>?> GetProjectsAsync(int employeeId)
    {
        Employee? employee = await GetByIdAsync(employeeId);

        return employee?.Projects;
    }

    private async Task<IReadOnlyList<Employee>> LoadEmployeesAsync()
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException(
                "Employee data file was not found.",
                _filePath);
        }

        await using FileStream stream = File.OpenRead(_filePath);

        List<Employee>? employees =
            await JsonSerializer.DeserializeAsync<List<Employee>>(
                stream,
                _serializerOptions);

        return employees ?? [];
    }
}