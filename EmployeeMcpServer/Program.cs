using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using System.ComponentModel;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

[McpServerToolType]
public static class EmployeeTools
{
    [McpServerTool]
    [Description("Gets employee information by employee ID.")]
    public static string GetEmployee(
        [Description("Employee ID")] int employeeId)
    {
        return employeeId switch
        {
            101 => """
                   {
                     "id": 101,
                     "name": "Sai",
                     "department": "Engineering"
                   }
                   """,

            _ => """{"error":"Employee not found"}"""
        };
    }
}