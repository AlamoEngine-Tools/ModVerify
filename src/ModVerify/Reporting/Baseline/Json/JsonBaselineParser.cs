using System;
using System.IO;
using System.Text.Json;

namespace AET.ModVerify.Reporting.Baseline.Json;

internal static class JsonBaselineParser
{
    public static VerificationBaseline Parse(Stream dataStream)
    {
        if (dataStream == null) 
            throw new ArgumentNullException(nameof(dataStream));
        try
        {
            var jsonNode = JsonDocument.Parse(dataStream);
            var jsonBaseline = EvaluateAndDeserialize(jsonNode);

            if (jsonBaseline is null)
                throw new InvalidBaselineException($"Unable to parse input from stream to {nameof(VerificationBaseline)}. Unknown Error!");

            return new VerificationBaseline(jsonBaseline);
        }
        catch (JsonException cause)
        {
            throw new InvalidBaselineException(cause.Message, cause);
        }
    }

    private static JsonVerificationBaseline? EvaluateAndDeserialize(JsonDocument? json)
    {
        if (json is null)
            return null;
        JsonBaselineSchema.Evaluate(json.RootElement);
        return json.Deserialize<JsonVerificationBaseline>();
    }
}