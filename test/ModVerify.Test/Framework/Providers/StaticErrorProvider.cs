using AET.ModVerify.Reporting;
using ModVerify.Test.Framework.Verifiers;

namespace ModVerify.Test.Framework.Providers;

internal static class StaticErrorProvider
{
    public static SingleVerifierProvider<StaticErrorVerifier> Create(
        string id,
        string asset,
        string[] context,
        string message = "static error",
        VerificationSeverity severity = VerificationSeverity.Warning)
    {
        return Create(new StaticErrorSpec(id, asset, context, severity, message));
    }

    public static SingleVerifierProvider<StaticErrorVerifier> Create(params StaticErrorSpec[] errors)
    {
        return new SingleVerifierProvider<StaticErrorVerifier>(
            (engine, settings, sp) => new StaticErrorVerifier(errors, engine, settings, sp));
    }
}
