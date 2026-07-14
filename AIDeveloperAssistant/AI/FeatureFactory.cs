using AIDeveloperAssistant.AI;
using AIDeveloperAssistant.Core;
using AIDeveloperAssistant.Features.CodeInterpreter;
using AIDeveloperAssistant.Features.FileSearch;
using AIDeveloperAssistant.Features.InterviewCoach;
using AIDeveloperAssistant.Features.WebSearch;

namespace AIDeveloperAssistant.Features;

internal class FeatureFactory
{
    private readonly ResponsesService _responsesService;

    public FeatureFactory(ResponsesService responsesService)
    {
        _responsesService = responsesService;
    }

    public IFeature Create(MenuOption option)
    {
        return option switch
        {
            MenuOption.InterviewCoach => new InterviewCoachFeature(_responsesService),
            MenuOption.CodeInterpreter => new CodeInterpreterFeature(_responsesService),
            MenuOption.WebSearch => new WebSearchFeature(_responsesService),
            MenuOption.FileSearch => new FileSearchFeature(_responsesService),
            _ => throw new NotImplementedException($"{option} is not implemented.")
        };
    }
}