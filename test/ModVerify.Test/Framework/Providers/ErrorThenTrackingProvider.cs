using System;
using System.Collections.Generic;
using AET.ModVerify;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers;
using ModVerify.Test.Framework.Verifiers;
using PG.StarWarsGame.Engine;

namespace ModVerify.Test.Framework.Providers;

internal sealed class ErrorThenTrackingProvider(
    Func<IStarWarsGameEngine, GameVerifySettings, IServiceProvider, StaticErrorVerifier> errorFactory)
    : IGameVerifiersProvider
{
    public TrackingVerifier? Tracker { get; private set; }

    public IEnumerable<GameVerifier> GetVerifiers(
        IStarWarsGameEngine engine, GameVerifySettings settings, IServiceProvider sp)
    {
        yield return errorFactory(engine, settings, sp);
        Tracker = new TrackingVerifier(engine, settings, sp);
        yield return Tracker;
    }

    public static ErrorThenTrackingProvider Create(
        string id,
        string asset,
        string[] context,
        VerificationSeverity severity,
        string message = "static error")
    {
        var spec = new StaticErrorSpec(id, asset, context, severity, message);
        return new ErrorThenTrackingProvider((engine, settings, sp) => new StaticErrorVerifier([spec], engine, settings, sp));
    }
}
