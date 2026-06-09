using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AET.ModVerify;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Baseline;
using AET.ModVerify.Reporting.Reporters;
using AET.ModVerify.Reporting.Suppressions;
using AET.ModVerify.Verifiers;
using AnakinRaW.CommonUtilities.Collections;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine;
using Testably.Abstractions.Testing;
using Xunit;

namespace ModVerify.Test.Reporting;

public class JsonReporterTest
{
    private const string OutputDirectory = "verify-out";

    [Fact]
    public async Task ReportAsync_WritesMetadata_WithTargetStatusDurationAndVerifiers()
    {
        var errors = Errors(newErrors: [], resolved: []);

        var root = await Report(
            errors,
            status: VerificationCompletionStatus.Completed,
            duration: TimeSpan.FromSeconds(5),
            version: "1.2.3",
            verifiers: [StubVerifierInfo.Instance]);

        var metadata = root.GetProperty("metadata");
        Assert.Equal("Completed", metadata.GetProperty("status").GetString());
        Assert.Equal("0:00:05", metadata.GetProperty("duration").GetString());
        Assert.False(string.IsNullOrEmpty(metadata.GetProperty("time").GetString()));

        var verifiers = metadata.GetProperty("verifiers").EnumerateArray().Select(x => x.GetString()).ToList();
        Assert.Equal(["stub"], verifiers);

        var target = metadata.GetProperty("target");
        Assert.Equal("test-target", target.GetProperty("name").GetString());
        Assert.Equal("Foc", target.GetProperty("engine").GetString());
        Assert.Equal("1.2.3", target.GetProperty("version").GetString());
        Assert.True(target.GetProperty("isGame").GetBoolean());

        var location = target.GetProperty("location");
        Assert.Equal("game", location.GetProperty("gamePath").GetString());
        Assert.Empty(location.GetProperty("modPaths").EnumerateArray());
        Assert.Equal(["fallback"], location.GetProperty("fallbackPaths").EnumerateArray().Select(x => x.GetString()));
    }

    [Fact]
    public async Task ReportAsync_OmitsVersion_WhenTargetHasNoVersion()
    {
        var errors = Errors(newErrors: [], resolved: []);

        var root = await Report(errors, version: null);

        var target = root.GetProperty("metadata").GetProperty("target");
        Assert.False(target.TryGetProperty("version", out _));
    }

    [Fact]
    public async Task ReportAsync_WritesErrors_WithAllProperties()
    {
        var errors = Errors(
            newErrors: [Error("FILE00", "foo.alo")],
            resolved: []);

        var root = await Report(errors);

        var error = root.GetProperty("errors").EnumerateArray().Single();
        Assert.Equal("FILE00", error.GetProperty("id").GetString());
        Assert.Equal("foo.alo", error.GetProperty("asset").GetString());
        Assert.Equal("message", error.GetProperty("message").GetString());
        Assert.Equal("Error", error.GetProperty("severity").GetString());
    }

    [Fact]
    public async Task ReportAsync_OrdersErrors_BySeverityThenId()
    {
        var errors = Errors(
            newErrors:
            [
                Error("TEX01", "bar.tga", severity: VerificationSeverity.Warning),
                Error("FILE02", "baz.alo", severity: VerificationSeverity.Error),
                Error("FILE00", "foo.alo", severity: VerificationSeverity.Error),
            ],
            resolved: []);

        var root = await Report(errors);

        var ids = root.GetProperty("errors").EnumerateArray()
            .Select(x => x.GetProperty("id").GetString())
            .ToList();
        Assert.Equal(["FILE00", "FILE02", "TEX01"], ids);
    }

    [Fact]
    public async Task ReportAsync_VerboseDisabled_OmitsVerifierChain()
    {
        var errors = Errors(newErrors: [Error("FILE00", "foo.alo")], resolved: []);

        var root = await Report(errors);

        var error = root.GetProperty("errors").EnumerateArray().Single();
        Assert.False(error.TryGetProperty("verifiers", out _));
    }

    [Fact]
    public async Task ReportAsync_VerboseEnabled_WritesVerifierChain()
    {
        var errors = Errors(newErrors: [Error("FILE00", "foo.alo")], resolved: []);

        var root = await Report(errors, verbose: true);

        var error = root.GetProperty("errors").EnumerateArray().Single();
        var chain = error.GetProperty("verifiers").EnumerateArray().Select(x => x.GetString()).ToList();
        Assert.Equal(["stub"], chain);
    }

    [Fact]
    public async Task ReportAsync_NoErrors_WritesEmptyErrorsArray()
    {
        var errors = Errors(newErrors: [], resolved: []);

        var root = await Report(errors);

        Assert.Equal(JsonValueKind.Array, root.GetProperty("errors").ValueKind);
        Assert.Empty(root.GetProperty("errors").EnumerateArray());
    }

    [Fact]
    public async Task ReportAsync_AggregateResults_CollapsesContextsForSameError()
    {
        var errors = Errors(
            newErrors:
            [
                Error("FILE00", "foo.alo", ["ctx-a"]),
                Error("FILE00", "foo.alo", ["ctx-b"]),
            ],
            resolved: []);

        var root = await Report(errors, aggregate: true);

        var error = root.GetProperty("errors").EnumerateArray().Single();
        var contexts = error.GetProperty("contexts").EnumerateArray()
            .Select(c => c.EnumerateArray().Single().GetString())
            .ToList();
        Assert.Equal(["ctx-a", "ctx-b"], contexts);
    }

    [Fact]
    public async Task ReportAsync_ResolvedErrors_AreGroupedByBaselineIdentifier()
    {
        var errors = Errors(
            newErrors: [],
            resolved:
            [
                ("foc-default", [Error("FILE00", "foo.alo")]),
                ("mod-baseline", [Error("TEX01", "bar.tga")]),
            ]);

        var root = await Report(errors);

        var resolved = root.GetProperty("resolved");
        Assert.Equal(JsonValueKind.Object, resolved.ValueKind);

        var defaultBaseline = resolved.GetProperty("foc-default").EnumerateArray().Single();
        Assert.Equal("FILE00", defaultBaseline.GetProperty("id").GetString());
        Assert.Equal("foo.alo", defaultBaseline.GetProperty("asset").GetString());

        var modBaseline = resolved.GetProperty("mod-baseline").EnumerateArray().Single();
        Assert.Equal("TEX01", modBaseline.GetProperty("id").GetString());
    }

    [Fact]
    public async Task ReportAsync_BaselineWithoutResolvedErrors_IsOmitted()
    {
        var errors = Errors(
            newErrors: [],
            resolved:
            [
                ("foc-default", [Error("FILE00", "foo.alo")]),
                ("empty-baseline", []),
            ]);

        var root = await Report(errors);

        var resolved = root.GetProperty("resolved");
        Assert.True(resolved.TryGetProperty("foc-default", out _));
        Assert.False(resolved.TryGetProperty("empty-baseline", out _));
    }

    [Fact]
    public async Task ReportAsync_NoResolvedErrors_OmitsResolvedSection()
    {
        var errors = Errors(
            newErrors: [Error("FILE00", "foo.alo")],
            resolved: [("foc-default", [])]);

        var root = await Report(errors);

        Assert.False(root.TryGetProperty("resolved", out _));
    }

    [Fact]
    public async Task ReportAsync_AggregateResults_CollapsesResolvedContextsForSameError()
    {
        var errors = Errors(
            newErrors: [],
            resolved:
            [
                ("foc-default",
                [
                    Error("FILE00", "foo.alo", ["ctx-a"]),
                    Error("FILE00", "foo.alo", ["ctx-b"]),
                ]),
            ]);

        var root = await Report(errors, aggregate: true);

        var entry = root.GetProperty("resolved").GetProperty("foc-default").EnumerateArray().Single();
        var contexts = entry.GetProperty("contexts").EnumerateArray()
            .Select(c => c.EnumerateArray().Single().GetString())
            .ToList();
        Assert.Equal(["ctx-a", "ctx-b"], contexts);
    }

    private static CategorizedVerificationErrors Errors(
        IReadOnlyList<VerificationError> newErrors,
        (string Baseline, VerificationError[] Errors)[] resolved)
    {
        var resolvedDictionary = new ValueListDictionary<string, VerificationError>();
        foreach (var (baseline, baselineErrors) in resolved)
            resolvedDictionary.AddRange(baseline, baselineErrors);

        return new CategorizedVerificationErrors(
            newErrors,
            ReadOnlyValueListDictionary<string, VerificationError>.Empty,
            new ReadOnlyValueListDictionary<string, VerificationError>(resolvedDictionary));
    }

    private static async Task<JsonElement> Report(
        CategorizedVerificationErrors errors,
        bool aggregate = false,
        bool verbose = false,
        VerificationCompletionStatus status = VerificationCompletionStatus.Completed,
        TimeSpan? duration = null,
        string? version = null,
        IReadOnlyCollection<IGameVerifierInfo>? verifiers = null)
    {
        var fileSystem = new MockFileSystem();
        var services = new ServiceCollection();
        services.AddSingleton<IFileSystem>(fileSystem);
        var serviceProvider = services.BuildServiceProvider();

        var result = new VerificationResult
        {
            Status = status,
            Errors = errors,
            UsedBaselines = BaselineCollection.Empty,
            UsedSuppressions = SuppressionList.Empty,
            Verifiers = verifiers ?? [],
            Target = new VerificationTarget
            {
                Engine = GameEngineType.Foc,
                Name = "test-target",
                Location = new GameLocations("game", "fallback"),
                Version = version,
            },
            Duration = duration ?? TimeSpan.Zero,
        };

        var reporter = IVerificationReporter.CreateJson(
            new JsonReporterSettings
            {
                OutputDirectory = OutputDirectory,
                AggregateResults = aggregate,
                Verbose = verbose,
            },
            serviceProvider);
        await reporter.ReportAsync(result);

        var path = fileSystem.Path.GetFullPath(
            fileSystem.Path.Combine(OutputDirectory, "VerificationResult_test-target.json"));
        var json = fileSystem.File.ReadAllText(path);

        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static VerificationError Error(
        string id,
        string asset,
        string[]? context = null,
        VerificationSeverity severity = VerificationSeverity.Error)
    {
        return new VerificationError(
            id, "message", StubVerifierInfo.Instance, context ?? [], asset, severity);
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
