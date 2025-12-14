using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.App.Settings;

internal sealed record GameInstallationsSettings
{
    public bool Interactive => string.IsNullOrEmpty(AutoPath) && ModPaths.Count == 0 && string.IsNullOrEmpty(GamePath);

    [MemberNotNullWhen(true, nameof(AutoPath))]
    public bool UseAutoDetection => !string.IsNullOrEmpty(AutoPath);

    [MemberNotNullWhen(true, nameof(GamePath))]
    public bool ManualSetup => !string.IsNullOrEmpty(GamePath);

    public string? AutoPath { get; init; }

    public IReadOnlyList<string> ModPaths { get; init; } = [];

    public string? GamePath { get; init; }

    public string? FallbackGamePath { get; init; }

    public IReadOnlyList<string> AdditionalFallbackPaths { get; init; } = [];

    public GameEngineType? Engine { get; init; }
}