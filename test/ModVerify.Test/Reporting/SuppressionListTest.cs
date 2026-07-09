using System.Collections.Generic;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Suppressions;
using AET.ModVerify.Verifiers;
using Xunit;

namespace ModVerify.Test.Reporting;

public class SuppressionListTest
{
    [Fact]
    public void Suppresses_ErrorMatchedOnlyByDisjointFilter_IsStillSuppressed()
    {
        var list = new SuppressionList(
        [
            new SuppressionFilter(id: "E1", verifier: null, asset: null),
            new SuppressionFilter(id: null, verifier: "V", asset: "a"),
        ]);

        // Matches only the second filter (Verifier "V" + Asset "a"); the first (Id "E1") does not apply.
        var error = CreateError(id: "OTHER", verifier: "V", asset: "a");

        Assert.True(list.Suppresses(error));
    }

    [Fact]
    public void Suppresses_ErrorMatchedByBroadFilter_IsSuppressed()
    {
        var list = new SuppressionList(
        [
            new SuppressionFilter(id: "E1", verifier: null, asset: null),
            new SuppressionFilter(id: null, verifier: "V", asset: "a"),
        ]);

        var error = CreateError(id: "E1", verifier: "AnyVerifier", asset: "any-asset");

        Assert.True(list.Suppresses(error));
    }

    private static VerificationError CreateError(string id, string verifier, string asset)
    {
        return new VerificationError(id, "message", new FakeVerifierInfo(verifier), ["ctx"], asset, VerificationSeverity.Warning);
    }

    private sealed class FakeVerifierInfo(string name) : IGameVerifierInfo
    {
        public IGameVerifierInfo? Parent => null;
        public IReadOnlyList<IGameVerifierInfo> VerifierChain => [this];
        public string Name { get; } = name;
        public string FriendlyName => Name;
    }
}
