using System;
using AET.ModVerify.Settings;
using System.Diagnostics.CodeAnalysis;

namespace ModVerify.CliApp.Options;

internal record ModVerifyAppSettings
{
    public required GameVerifySettings GameVerifySettings { get; init; }

    public required GameInstallationsSettings GameInstallationsSettings { get; init; }

    public string Output { get; init; } = Environment.CurrentDirectory;

    [MemberNotNullWhen(true, nameof(NewBaselinePath))]
    public bool CreateNewBaseline => !string.IsNullOrEmpty(NewBaselinePath);

    public string? NewBaselinePath { get; init; }
}