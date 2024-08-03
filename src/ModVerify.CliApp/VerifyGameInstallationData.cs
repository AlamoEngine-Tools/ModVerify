using System.Text;
using PG.StarWarsGame.Engine;

namespace AET.ModVerifyTool;

internal class VerifyGameInstallationData
{
    public required string Name { get; init; }

    public required GameEngineType EngineType { get; init; }

    public required GameLocations GameLocations { get; init; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine($"ObjectToVerify={Name};EngineType={EngineType};Locations=[");
        if (GameLocations.ModPaths.Count > 0) 
            sb.AppendLine($"Mods=[{string.Join(";", GameLocations.ModPaths)}];");
        sb.AppendLine($"Game=[{GameLocations.GamePath}];");
        if (GameLocations.FallbackPaths.Count > 0)
            sb.AppendLine($"Fallbacks=[{string.Join(";", GameLocations.FallbackPaths)}];");
        sb.AppendLine("]");

        return sb.ToString();
    }
}