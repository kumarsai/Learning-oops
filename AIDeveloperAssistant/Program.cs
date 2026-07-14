using AIDeveloperAssistant.AI;
using AIDeveloperAssistant.Core;
using AIDeveloperAssistant.Features;

var appConfig = AppConfig.Instance;

var clientFactory = new AIClientFactory();
var client = clientFactory.CreateClient(appConfig.Endpoint);

var responsesService = new ResponsesService(
    client,
    appConfig.DeploymentName);

var featureFactory = new FeatureFactory(responsesService);

while (true)
{
    MenuOption option = AppMenu.ShowMainMenu();

    if (option == MenuOption.Exit)
    {
        break;
    }

    IFeature feature = featureFactory.Create(option);
    await feature.RunAsync();

    Console.WriteLine();
    Console.WriteLine("Press any key to return to main menu...");
    Console.ReadKey();
}