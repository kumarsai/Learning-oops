using Microsoft.Extensions.Configuration;

namespace EmployeeAssistant.ConsoleApp;

internal sealed class AppConfig
{
    private static readonly Lazy<AppConfig> _instance = new(() => new AppConfig());

    public static AppConfig Instance => _instance.Value;

    public string Endpoint { get; }
    public string DeploymentName { get; }

    public string EmployeeFunctionBaseUrl { get; }
    public string? EmployeeFunctionKey { get; }

    private AppConfig()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        Endpoint = config["AzureOpenAI:Endpoint"]
            ?? throw new Exception("AzureOpenAI:Endpoint is missing");

        DeploymentName = config["AzureOpenAI:DeploymentName"]
            ?? throw new Exception("AzureOpenAI:DeploymentName is missing");

        EmployeeFunctionBaseUrl =
          config["EmployeeFunction:BaseUrl"]
          ?? throw new Exception("Employee Function BaseUrl is missing.");

        EmployeeFunctionKey =
            Environment.GetEnvironmentVariable("EMPLOYEE_FUNCTION_KEY")
            ?? config["EmployeeFunction:FunctionKey"];
    }
}