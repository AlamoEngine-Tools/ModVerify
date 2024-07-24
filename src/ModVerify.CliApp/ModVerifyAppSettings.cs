using System;
using AET.ModVerify.Settings;
using System.Diagnostics.CodeAnalysis;

namespace ModVerify.CliApp;

public record ModVerifyAppSettings
{
    public GameVerifySettings GameVerifySettigns { get; init; }

    public string? PathToVerify { get; init; } = null;

    public string Output { get; init; } = Environment.CurrentDirectory;

    public string? AdditionalFallbackPath { get; init; } = null;

    [MemberNotNullWhen(true, nameof(NewBaselinePath))]
    public bool CreateNewBaseline => !string.IsNullOrEmpty(NewBaselinePath);

    public string? NewBaselinePath { get; init; }
}