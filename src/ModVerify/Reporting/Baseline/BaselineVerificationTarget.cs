using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Reporting.Baseline;

public sealed record BaselineVerificationTarget
{ 
    public required GameEngineType Engine { get; init; }
    public required string Name { get; init; }
    public GameLocations? Location { get; init; } // Optional compared to Verification Target
    public string? Version { get; init; }
    public bool IsGame { get; init; }
}