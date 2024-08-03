using System;
using System.Diagnostics.CodeAnalysis;
using AET.ModVerify.Settings;

namespace AET.ModVerifyTool.Options;

internal record ModVerifyAppSettings
{
    public required GameVerifySettings GameVerifySettings { get; init; }

    public required GameInstallationsSettings GameInstallationsSettings { get; init; }

    public string Output { get; init; } = Environment.CurrentDirectory;

    [MemberNotNullWhen(true, nameof(NewBaselinePath))]
    public bool CreateNewBaseline => !string.IsNullOrEmpty(NewBaselinePath);

    public string? NewBaselinePath { get; init; }
}