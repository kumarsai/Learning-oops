using AIDeveloperAssistant.AI;
using OpenAI.Responses;

namespace AIDeveloperAssistant.Features;

internal abstract class BaseFeature
{
    protected ResponsesService _responsesService {  get; set; }

#pragma warning disable OPENAI001
    //protected ResponsesClient _client { get; set; }
    //protected string _deploymentName { get; set; }
    protected string _instructions { get; set; }
    protected string? _previousResponseId {  get; set; }
}
