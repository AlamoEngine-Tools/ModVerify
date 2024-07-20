using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AET.ModVerify.Reporting.Json;

internal class JsonVerificationReport(IEnumerable<JsonVerificationError> errors)
{
    [JsonPropertyName("errors")]
    public IEnumerable<JsonVerificationError> Errors { get; } = errors;
}