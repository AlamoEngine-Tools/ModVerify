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
            AddError(VerificationError.Create(this, spec.Id, spec.Message, spec.Severity, spec.Context, spec.Asset));
    }
}

internal sealed record StaticErrorSpec(
    string Id,
    string Asset,
    string[] Context,
    VerificationSeverity Severity = VerificationSeverity.Warning,
    string Message = "static error");
