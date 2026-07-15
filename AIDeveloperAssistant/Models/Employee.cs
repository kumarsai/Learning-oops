namespace AIDeveloperAssistant.Models;

internal sealed class Employee
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Department { get; set; } = string.Empty;

    public string JobTitle { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public int LeaveBalance { get; set; }

    public List<string> Projects { get; set; } = [];
}