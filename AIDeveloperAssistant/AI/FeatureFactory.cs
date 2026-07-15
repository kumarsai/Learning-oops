using AIDeveloperAssistant.AI;
using AIDeveloperAssistant.Core;
using AIDeveloperAssistant.Features.CodeInterpreter;
using AIDeveloperAssistant.Features.EmployeeAssistant;
using AIDeveloperAssistant.Features.FileSearch;
using AIDeveloperAssistant.Features.FunctionCalling;
using AIDeveloperAssistant.Features.InterviewCoach;
using AIDeveloperAssistant.Features.WebSearch;
using AIDeveloperAssistant.Services;

namespace AIDeveloperAssistant.Features;

internal class FeatureFactory
{
    private readonly ResponsesService _responsesService;
    //private readonly EmployeeFunctionClient _employeeClient = new EmployeeFunctionClient();
    //private readonly EmployeeAssistantService _employeeAssistantService = new EmployeeAssistantService();
    public FeatureFactory(ResponsesService responsesService)
    {
        _responsesService = responsesService;
    }

    public IFeature Create(MenuOption option)
    {
        AppConfig config = AppConfig.Instance;
        var employeeFunctionClient =
    new EmployeeFunctionClient();

        var employeeToolExecutor =
            new EmployeeToolExecutor(
                employeeFunctionClient);

        var employeeAssistantService =
            new EmployeeAssistantService(
                _responsesService._client,
                config.DeploymentName,
                employeeToolExecutor);

        return option switch
        {
            MenuOption.InterviewCoach => new InterviewCoachFeature(_responsesService),
            MenuOption.CodeInterpreter => new CodeInterpreterFeature(_responsesService),
            MenuOption.WebSearch => new WebSearchFeature(_responsesService),
            MenuOption.FileSearch => new FileSearchFeature(_responsesService),
            MenuOption.SmartAssistant => new FunctionCallingFeature(_responsesService),
            MenuOption.EmployeeAssistant => new EmployeeAssistantFeature1(employeeAssistantService),

            _ => throw new NotImplementedException($"{option} is not implemented.")
        };
    }
}