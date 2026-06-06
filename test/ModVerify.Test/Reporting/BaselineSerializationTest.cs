using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Baseline;
using AET.ModVerify.Verifiers;
using Xunit;

namespace ModVerify.Test.Reporting;

/// <summary>
/// Baselines are version-controlled, so their JSON serialization must be deterministic. The errors
/// are stored in a <see cref="HashSet{T}"/> and context entries in a <see cref="HashSet{T}"/> of
/// strings; their enumeration order is randomized per process (System.HashCode seeds randomly at
/// startup). These tests pin the canonical ordering that the serializer imposes.
/// </summary>
public class BaselineSerializationTest
{
    [Fact]
    public void ToJson_ErrorInsertionOrderDoesNotAffectOutput()
    {
        var a = Error("TEST01", "asset-a", VerificationSeverity.Warning, ["b", "a"]);
        var b = Error("TEST02", "asset-b", VerificationSeverity.Error, ["x"]);
        var c = Error("TEST01", "asset-a", VerificationSeverity.Warning, ["c"]);

        var first = Serialize([a, b, c]);
        var second = Serialize([c, a, b]);
        var third = Serialize([b, c, a]);

        Assert.Equal(first, second);
        Assert.Equal(first, third);
    }

    [Fact]
    public void ToJson_ErrorsAreSortedBySeverityThenIdThenAsset()
    {
        var json = Serialize(
        [
            Error("TEST02", "asset-b", VerificationSeverity.Warning, []),
            Error("TEST01", "asset-z", VerificationSeverity.Error, []),
            Error("TEST01", "asset-a", VerificationSeverity.Error, []),
            Error("TEST00", "asset-a", VerificationSeverity.Warning, []),
        ]);

        var errors = ParseErrors(json)
            .Select(e => (Id: e.GetProperty("id").GetString(), Asset: e.GetProperty("asset").GetString()))
            .ToList();

        Assert.Equal(
        [
            ("TEST01", "asset-a"),
            ("TEST01", "asset-z"),
            ("TEST00", "asset-a"),
            ("TEST02", "asset-b"),
        ], errors);
    }

    [Fact]
    public void ToJson_ContextEntriesPreserveInsertionOrder()
    {
        // Context is a semantic sequence (parser -> file -> element), not a set to alphabetize.
        string[] context = ["Parser: SomeParser", "File: DATA\\XML\\FOO.XML", "element-z"];
        var json = Serialize([Error("TEST01", "asset-a", VerificationSeverity.Warning, context)]);

        var serializedContext = ParseErrors(json).Single()
            .GetProperty("context")
            .EnumerateArray()
            .Select(x => x.GetString())
            .ToList();

        Assert.Equal(context, serializedContext);
    }

    private static string Serialize(IEnumerable<VerificationError> errors)
    {
        var baseline = new VerificationBaseline(VerificationSeverity.Information, errors, target: null);
        using var stream = new MemoryStream();
        baseline.ToJson(stream);
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static IReadOnlyList<JsonElement> ParseErrors(string json)
    {
        using var doc = JsonDocument.Parse(json);
        // Clone so the elements survive the document's disposal.
        return doc.RootElement.GetProperty("errors").EnumerateArray().Select(e => e.Clone()).ToList();
    }

    private static VerificationError Error(string id, string asset, VerificationSeverity severity, string[] context)
    {
        return new VerificationError(id, "message", StubVerifierInfo.Instance, context, asset, severity);
    }

    private sealed class StubVerifierInfo : IGameVerifierInfo
    {
        public static readonly StubVerifierInfo Instance = new();
        public IGameVerifierInfo? Parent => null;
        public IReadOnlyList<IGameVerifierInfo> VerifierChain => [this];
        public string Name => "stub";
        public string FriendlyName => Name;
    }
}
