using EmployeeAssistant.ConsoleApp;
using EmployeeAssistant.ConsoleApp.Agent;

Console.WriteLine("Hello, World!");
var config = AppConfig.Instance;
var functionClient = new EmployeeFunctionClient(
    new HttpClient(),
    functionBaseUrl: config.EmployeeFunctionBaseUrl,
    functionKey: "");

var agentService = new EmployeeAgentService(
    // Use the project endpoint (up to /api/projects/{projectName})
    projectEndpoint: "https://suku-ai-103-learning-resource.services.ai.azure.com/api/projects/ai-103-learning",
    modelDeploymentName: config.DeploymentName,
    functionClient);

string answer = await agentService.AskAsync(
    "Show employee 1001");

Console.WriteLine(answer);

answer = await agentService.AskAsync(
    "Show employee 1002");

Console.WriteLine(answer);

while (true)
{
    string? messenge = Console.ReadLine();

    answer = await agentService.AskAsync(messenge);

    Console.WriteLine(answer);
}