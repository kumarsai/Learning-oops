using Functions.Models;

namespace Functions.Services;

public interface IEmployeeService
{
    Task<Employee?> GetByIdAsync(int employeeId);

    Task<IReadOnlyList<Employee>> SearchByNameAsync(string name);

    Task<IReadOnlyList<Employee>> GetByDepartmentAsync(string department);

    Task<int?> GetLeaveBalanceAsync(int employeeId);

    Task<IReadOnlyList<string>?> GetProjectsAsync(int employeeId);
}