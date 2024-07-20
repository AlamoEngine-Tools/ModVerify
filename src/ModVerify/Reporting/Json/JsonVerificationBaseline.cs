using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace AET.ModVerify.Reporting.Json;

internal class JsonVerificationBaseline
{
    [JsonPropertyName("errors")]
    public IEnumerable<JsonVerificationError> Errors { get; }

    public JsonVerificationBaseline(VerificationBaseline baseline)
    {
        Errors = baseline.Select(x => new JsonVerificationError(x));
    }

    [JsonConstructor]
    private JsonVerificationBaseline(IEnumerable<JsonVerificationError> errors)
    {
        Errors = errors;
    }
}