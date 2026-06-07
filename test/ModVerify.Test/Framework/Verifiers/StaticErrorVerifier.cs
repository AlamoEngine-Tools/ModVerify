using System;
using System.Collections.Generic;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers;
using PG.StarWarsGame.Engine;

namespace ModVerify.Test.Framework.Verifiers;

/// <summary>Emits a fixed set of errors on each <see cref="Verify"/> call.</summary>
internal sealed class StaticErrorVerifier(
    IReadOnlyList<StaticErrorSpec> errors,
    IStarWarsGameEngine gameEngine,
    GameVerifySettings settings,
    IServiceProvider sp)
    : GameVerifier(gameEngine, settings, sp)
{
    public override void Verify(CancellationToken token)
    {
        foreach (var spec in errors)
            AddError(new VerificationError(spec.Id, spec.Message, this, spec.Context, spec.Asset, spec.Severity));
    }
}

internal sealed record StaticErrorSpec(
    string Id,
    string Asset,
    string[] Context,
    VerificationSeverity Severity = VerificationSeverity.Warning,
    string Message = "static error");
