using AET.ModVerify.Reporting.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AET.ModVerify.Reporting.Baseline.Json;

internal class JsonVerificationBaseline
{
    [JsonPropertyName("version")]
    public Version? Version { get; }

    [JsonPropertyName("target")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonVerificationTarget? Target { get; }

    [JsonPropertyName("minSeverity")]
    [JsonConverter(typeof(JsonStringEnumConverter<VerificationSeverity>))]
    public VerificationSeverity MinimumSeverity { get; }

    [JsonPropertyName("errors")]
    public IEnumerable<JsonVerificationError> Errors { get; }

    public JsonVerificationBaseline(VerificationBaseline baseline)
    {
        Errors = baseline
            .OrderByDescending(x => x.Severity)
            .ThenBy(x => x.Id, StringComparer.Ordinal)
            .ThenBy(x => x.Asset, StringComparer.Ordinal)
            .ThenBy(x => string.Join("\n", x.ContextEntries), StringComparer.Ordinal)
            .Select(x => new JsonVerificationError(x))
            .ToList();
        Version = baseline.Version;
        MinimumSeverity = baseline.MinimumSeverity;
        Target = baseline.Target is not null ? new JsonVerificationTarget(baseline.Target) : null;
    }

    [JsonConstructor]
    private JsonVerificationBaseline(
        JsonVerificationTarget target,
        Version version,
        VerificationSeverity minimumSeverity,
        IEnumerable<JsonVerificationError> errors)
    {
        Target = target;
        Errors = errors;
        Version = version;
        MinimumSeverity = minimumSeverity;
    }
}