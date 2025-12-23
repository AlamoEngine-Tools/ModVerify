using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.App.Settings;

internal sealed record VerificationTargetSettings
{
    public bool Interactive => string.IsNullOrEmpty(TargetPath) && ModPaths.Count == 0 && string.IsNullOrEmpty(GamePath) && string.IsNullOrEmpty(FallbackGamePath);

    [MemberNotNullWhen(true, nameof(TargetPath))]
    public bool UseAutoDetection => !string.IsNullOrEmpty(TargetPath) && ModPaths.Count == 0 && string.IsNullOrEmpty(GamePath) && string.IsNullOrEmpty(FallbackGamePath);

    [MemberNotNullWhen(true, nameof(GamePath))]
    public bool ManualSetup => !string.IsNullOrEmpty(GamePath);

    public string? TargetPath { get; init; }

    public IReadOnlyList<string> ModPaths { get; init; } = [];

    public string? GamePath { get; init; }

    public string? FallbackGamePath { get; init; }

    public IReadOnlyList<string> AdditionalFallbackPaths { get; init; } = [];

    public GameEngineType? Engine { get; init; }
}