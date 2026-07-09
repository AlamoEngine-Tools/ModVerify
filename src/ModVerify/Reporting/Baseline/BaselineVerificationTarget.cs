using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Reporting.Baseline;

/// <summary>Represents the target that a verification baseline was created for.</summary>
public sealed record BaselineVerificationTarget
{
    /// <summary>Gets or sets the game engine type of the target.</summary>
    public required GameEngineType Engine { get; init; }

    /// <summary>Gets or sets the name of the target.</summary>
    public required string Name { get; init; }

    /// <summary>Gets or sets the game locations of the target, or <see langword="null"/> if not specified.</summary>
    /// <remarks>The location is optional for a baseline target, unlike for a verification target.</remarks>
    public GameLocations? Location { get; init; }

    /// <summary>Gets or sets the version of the target, or <see langword="null"/> if not specified.</summary>
    public string? Version { get; init; }

    /// <summary>Gets or sets a value that indicates whether the target is the base game rather than a mod.</summary>
    /// <value>
    /// <see langword="true"/> if the target is the base game; otherwise, <see langword="false"/>.
    /// The default is <see langword="false"/>.
    /// </value>
    public bool IsGame { get; init; }
}