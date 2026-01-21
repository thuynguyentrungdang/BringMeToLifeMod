using System.Reflection;
using RevivalModServer.Models;
using RevivalModServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Mod;
using Range = SemanticVersioning.Range;

namespace RevivalModServer;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.kobethuy.bringmetolife";
    public override string Name { get; init; } = "Bring Me To Life - Revival Mod (Server)";
    public override string Author { get; init; } = "Kobe Thuy";
    public override SemanticVersioning.Version Version { get; init; } = new("2.0.0");
    public override Range SptVersion { get; init; } = new("~4.0.0");
    public override string License { get; init; } = "MIT";
    public override bool? IsBundleMod { get; init; } = true;
    public override Dictionary<string, Range>? ModDependencies { get; init; }
    public override string? Url { get; init; }
    public override List<string>? Contributors { get; init; }
    public override List<string>? Incompatibilities { get; init; }
}

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class RevivalModServer (
    ModHelper modHelper,
    CustomStaticRouter customStaticRouter) : IOnLoad
{
    private ModConfig? _modConfig;
    
    public async Task OnLoad()
    {
        // Get your current assembly
        var assembly = Assembly.GetExecutingAssembly();
        var pathToMod = modHelper.GetAbsolutePathToModFolder(assembly);
        
        _modConfig = modHelper.GetJsonDataFromFile<ModConfig>(pathToMod, "config/config.json");
        
        customStaticRouter.PassConfig(_modConfig);
        await Task.CompletedTask;
    }

}