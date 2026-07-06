using System.Collections.Generic;
using System.Text.Json.Serialization;
using AnakinRaW.CommonUtilities.Collections;

namespace AET.ModVerify.Reporting.Json;

internal class JsonVerificationReport
{
    [JsonPropertyName("metadata")]
    public required JsonVerificationReportMetadata Metadata { get; init; }

    [JsonPropertyName("errors")]
    public required IEnumerable<JsonVerificationErrorBase> Errors { get; init; }
    
    [JsonPropertyName("resolved")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonConverter(typeof(ValueListDictionaryJsonConverter))]
    public ReadOnlyValueListDictionary<string, JsonVerificationErrorBase>? Resolved { get; init; }
}