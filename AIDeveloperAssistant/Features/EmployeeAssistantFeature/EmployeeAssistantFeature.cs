using AIDeveloperAssistant.Models;
using AIDeveloperAssistant.Services;

namespace AIDeveloperAssistant.Features.EmployeeAssistant;

internal sealed class EmployeeAssistantFeature : IFeature
{
    private readonly EmployeeFunctionClient _employeeClient;

    public EmployeeAssistantFeature(
        EmployeeFunctionClient employeeClient)
    {
        _employeeClient = employeeClient;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("=========================================");
            Console.WriteLine("          Employee Assistant");
            Console.WriteLine("=========================================");
            Console.WriteLine();
            Console.WriteLine("1. Get employee by ID");
            Console.WriteLine("2. Search employee by name");
            Console.WriteLine("3. Get leave balance");
            Console.WriteLine("4. Get employee projects");
            Console.WriteLine("5. List employees by department");
            Console.WriteLine("0. Return to main menu");
            Console.WriteLine();

            Console.Write("Select an option: ");
            string choice = Console.ReadLine() ?? string.Empty;

            try
            {
                switch (choice)
                {
                    case "1":
                        await ShowEmployeeAsync();
                        break;

                    case "2":
                        await SearchEmployeesAsync();
                        break;

                    case "3":
                        await ShowLeaveBalanceAsync();
                        break;

                    case "4":
                        await ShowProjectsAsync();
                        break;

                    case "5":
                        await ShowDepartmentEmployeesAsync();
                        break;

                    case "0":
                        return;

                    default:
                        Console.WriteLine("Invalid selection.");
                        break;
                }
            }
            catch (HttpRequestException exception)
            {
                Console.WriteLine();
                Console.WriteLine(
                    $"Unable to call Employee Function: " +
                    $"{exception.Message}");
            }

            Pause();
        }
    }

    private async Task ShowEmployeeAsync()
    {
        int? employeeId = ReadEmployeeId();

        if (employeeId is null)
        {
            return;
        }

        Employee? employee =
            await _employeeClient.GetEmployeeAsync(
                employeeId.Value);

        if (employee is null)
        {
            Console.WriteLine(
                $"Employee {employeeId} was not found.");

            return;
        }

        PrintEmployee(employee);
    }

    private async Task SearchEmployeesAsync()
    {
        Console.Write("Enter employee name: ");
        string name = Console.ReadLine() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Name is required.");
            return;
        }

        IReadOnlyList<Employee> employees =
            await _employeeClient.SearchEmployeesAsync(name);

        PrintEmployees(employees);
    }

    private async Task ShowLeaveBalanceAsync()
    {
        int? employeeId = ReadEmployeeId();

        if (employeeId is null)
        {
            return;
        }

        LeaveBalanceResult? result =
            await _employeeClient.GetLeaveBalanceAsync(
                employeeId.Value);

        if (result is null)
        {
            Console.WriteLine(
                $"Employee {employeeId} was not found.");

            return;
        }

        Console.WriteLine();
        Console.WriteLine(
            $"Employee {result.EmployeeId} has " +
            $"{result.LeaveBalance} leave days remaining.");
    }

    private async Task ShowProjectsAsync()
    {
        int? employeeId = ReadEmployeeId();

        if (employeeId is null)
        {
            return;
        }

        EmployeeProjectsResult? result =
            await _employeeClient.GetProjectsAsync(
                employeeId.Value);

        if (result is null)
        {
            Console.WriteLine(
                $"Employee {employeeId} was not found.");

            return;
        }

        Console.WriteLine();
        Console.WriteLine(
            $"Projects for employee {result.EmployeeId}:");

        foreach (string project in result.Projects)
        {
            Console.WriteLine($"- {project}");
        }
    }

    private async Task ShowDepartmentEmployeesAsync()
    {
        Console.Write("Enter department: ");

        string department =
            Console.ReadLine() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(department))
        {
            Console.WriteLine("Department is required.");
            return;
        }

        IReadOnlyList<Employee> employees =
            await _employeeClient
                .GetEmployeesByDepartmentAsync(department);

        PrintEmployees(employees);
    }

    private static int? ReadEmployeeId()
    {
        Console.Write("Enter employee ID: ");

        if (!int.TryParse(
                Console.ReadLine(),
                out int employeeId))
        {
            Console.WriteLine("Invalid employee ID.");
            return null;
        }

        return employeeId;
    }

    private static void PrintEmployees(
        IReadOnlyList<Employee> employees)
    {
        if (employees.Count == 0)
        {
            Console.WriteLine("No employees were found.");
            return;
        }

        foreach (Employee employee in employees)
        {
            PrintEmployee(employee);
            Console.WriteLine();
        }
    }

    private static void PrintEmployee(Employee employee)
    {
        Console.WriteLine();
        Console.WriteLine($"ID:         {employee.Id}");
        Console.WriteLine($"Name:       {employee.Name}");
        Console.WriteLine($"Job title:  {employee.JobTitle}");
        Console.WriteLine($"Department: {employee.Department}");
        Console.WriteLine($"Location:   {employee.Location}");
        Console.WriteLine($"Email:      {employee.Email}");
        Console.WriteLine(
            $"Leave:      {employee.LeaveBalance} days");

        Console.WriteLine("Projects:");

        foreach (string project in employee.Projects)
        {
            Console.WriteLine($"- {project}");
        }
    }

    private static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine(
            "Press any key to continue...");

        Console.ReadKey();
    }
}