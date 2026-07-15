namespace AIDeveloperAssistant.Models;

internal sealed class EmployeeProjectsResult
{
    public int EmployeeId { get; set; }

    public List<string> Projects { get; set; } = [];
}