using System.Text;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Reporting;

public sealed class BaselineVerificationTarget
{ 
    public required GameEngineType Engine { get; init; }
    public required string Name { get; init; }
    public GameLocations? Location { get; init; } // Optional compared to Verification Target
    public string? Version { get; init; }
    
    public override string ToString()
    {
        var sb = new StringBuilder($"[Name={Name};EngineType={Engine};");
        if (!string.IsNullOrEmpty(Version)) sb.Append($"Version={Version};");
        if (Location is not null) 
            sb.Append($"Location={Location};");
        sb.Append(']');
        return sb.ToString();
    }
}