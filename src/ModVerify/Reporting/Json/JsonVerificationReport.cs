using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AET.ModVerify.Reporting.Json;

internal class JsonVerificationReport
{
    [JsonPropertyName("metadata")]
    public required JsonVerificationReportMetadata Metadata { get; init; }

    [JsonPropertyName("errors")]
    public required IEnumerable<JsonVerificationErrorBase> Errors { get; init; }
}