using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PG.StarWarsGame.Engine;

namespace AET.ModVerifyTool.Options;

internal record GameInstallationsSettings
{
    public bool Interactive => string.IsNullOrEmpty(AutoPath) && ModPaths.Count == 0 && string.IsNullOrEmpty(GamePath);

    [MemberNotNullWhen(true, nameof(AutoPath))]
    public bool UseAutoDetection => !string.IsNullOrEmpty(AutoPath);

    [MemberNotNullWhen(true, nameof(GamePath))]
    public bool ManualSetup => !string.IsNullOrEmpty(GamePath);

    public string? AutoPath { get; init; }

    public IList<string> ModPaths { get; init; } = Array.Empty<string>();

    public string? GamePath { get; init; }

    public string? FallbackGamePath { get; init; }

    public IList<string> AdditionalFallbackPaths { get; init; } = Array.Empty<string>();

    public GameEngineType? EngineType { get; init; }
}