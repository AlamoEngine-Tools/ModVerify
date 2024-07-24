using AET.ModVerify.Reporting;

namespace AET.ModVerify.Settings;

public record VerificationAbortSettings
{
    public bool FailFast { get; init; } = false;

    public VerificationSeverity MinimumAbortSeverity { get; init; } = VerificationSeverity.Warning;

    public bool ThrowsGameVerificationException { get; init; } = false;
}