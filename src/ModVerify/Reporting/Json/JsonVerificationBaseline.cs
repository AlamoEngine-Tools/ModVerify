using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AET.ModVerify.Reporting.Json;

internal class JsonVerificationBaseline
{
    [JsonPropertyName("version")]
    public Version? Version { get; }

    [JsonPropertyName("minSeverity")]
    [JsonConverter(typeof(JsonStringEnumConverter<VerificationSeverity>))]
    public VerificationSeverity MinimumSeverity { get; }

    [JsonPropertyName("errors")]
    public IEnumerable<JsonVerificationError> Errors { get; }

    public JsonVerificationBaseline(VerificationBaseline baseline)
    {
        Errors = baseline.Select(x => new JsonVerificationError(x));
        Version = baseline.Version;
        MinimumSeverity = baseline.MinimumSeverity;
    }

    [JsonConstructor]
    private JsonVerificationBaseline(Version version, VerificationSeverity minimumSeverity, IEnumerable<JsonVerificationError> errors)
    {
        Errors = errors;
        Version = version;
        MinimumSeverity = minimumSeverity;
    }
}