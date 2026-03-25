namespace AET.ModVerify.Reporting.Reporters;
public record JsonReporterSettings : FileBasedReporterSettings
{
    public bool AggregateResults { get; init; }
}