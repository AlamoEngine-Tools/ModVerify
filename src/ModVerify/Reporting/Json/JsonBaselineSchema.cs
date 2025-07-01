using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using Json.Schema;

namespace AET.ModVerify.Reporting.Json;

public static class JsonBaselineSchema
{
    private static readonly JsonSchema Schema;
    private static readonly EvaluationOptions EvaluationOptions;
    
    static JsonBaselineSchema()
    {
        var evalvOptions = new EvaluationOptions
        {
            EvaluateAs = SpecVersion.Draft202012,
            OutputFormat = OutputFormat.Hierarchical,
            AllowReferencesIntoUnknownKeywords = false
        };

        Schema = GetCurrentSchema();
        EvaluationOptions = evalvOptions;
    }

    /// <summary>
    /// Evaluates a JSON node against the ModVerify Baseline JSON schema.
    /// </summary>
    /// <param name="json">The JSON node to evaluate.</param>
    /// <exception cref="InvalidBaselineException"><paramref name="json"/> is not valid against the baseline JSON schema.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static void Evaluate(JsonNode json)
    {
        if (json == null)
            throw new ArgumentNullException(nameof(json));
        var result = Schema.Evaluate(json, EvaluationOptions);
        ThrowOnValidationError(result);
    }

    private static void ThrowOnValidationError(EvaluationResults result)
    {
        if (!result.IsValid)
        {
            var error = GetFirstError(result);
            var errorMessage = "Baseline JSON not valid";

            if (error is null)
                errorMessage += ": Unknown Error";
            else
                errorMessage += $": {error}";

            throw new InvalidBaselineException(errorMessage);
        }
    }

    private static KeyValuePair<string, string>? GetFirstError(EvaluationResults result)
    {
        if (result.HasErrors)
            return result.Errors!.First();
        foreach (var child in result.Details)
        {
            var error = GetFirstError(child);
            if (error is not null)
                return error;
        }
        return null;
    }

    private static JsonSchema GetCurrentSchema()
    {
        using var resourceStream = typeof(JsonBaselineSchema)
            .Assembly.GetManifestResourceStream($"AET.ModVerify.Resources.Schemas.{GetVersionedPath()}.baseline.json");

        Debug.Assert(resourceStream is not null);
        var schema = JsonSchema.FromStream(resourceStream!).GetAwaiter().GetResult();

        var id = schema.GetId();
        if (id is null || !UriContainsVersion(id, VerificationBaseline.LatestVersionString))
            throw new InvalidOperationException("Internal error: The embedded schema version does not match the expected baseline version!");

        return schema;
    }

    private static bool UriContainsVersion(Uri id, string latestVersionString)
    {
        foreach (var segment in id.Segments)
        {
            var trimmed = segment.AsSpan().TrimEnd('/');
            if (trimmed.Equals(latestVersionString, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static string GetVersionedPath()
    {
        var version = VerificationBaseline.LatestVersion;
        var sb = new StringBuilder();

        AddVersionSegment(version.Major, ref sb);
        AddVersionSegment(version.Minor, ref sb);
        AddVersionSegment(version.Build, ref sb);
        AddVersionSegment(version.Revision, ref sb);

        // Remove the trailing dot
        sb.Length -= 1;

        return sb.ToString();

        static void AddVersionSegment(int segment, ref StringBuilder sb)
        {
            if (segment >= 0)
            {
                sb.Append('_');
                sb.Append(segment);
                sb.Append(".");
            }
        }
    }
}