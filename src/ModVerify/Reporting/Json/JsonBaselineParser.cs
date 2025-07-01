using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AET.ModVerify.Reporting.Json;

public static class JsonBaselineParser
{
    public static VerificationBaseline Parse(Stream dataStream)
    {
        if (dataStream == null) 
            throw new ArgumentNullException(nameof(dataStream));
        try
        {
            var jsonNode = JsonNode.Parse(dataStream);
            var jsonBaseline = ParseCore(jsonNode);

            if (jsonBaseline is null)
                throw new InvalidBaselineException($"Unable to parse input from stream to {nameof(VerificationBaseline)}. Unknown Error!");

            if (jsonBaseline.Version != VerificationBaseline.LatestVersion)
                throw new InvalidBaselineException("The parsed baseline version does not match the version used by this version of ModVerify.");

            return new VerificationBaseline(jsonBaseline);
        }
        catch (JsonException cause)
        {
            throw new InvalidBaselineException(cause.Message, cause);
        }
    }

    private static JsonVerificationBaseline? ParseCore(JsonNode? jsonData)
    {
        if (jsonData is null)
            return null;

        JsonBaselineSchema.Evaluate(jsonData);
        return jsonData.Deserialize<JsonVerificationBaseline>();
    }
}