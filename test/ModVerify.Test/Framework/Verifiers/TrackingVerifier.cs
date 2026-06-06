using System;
using System.Threading;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers;
using PG.StarWarsGame.Engine;

namespace ModVerify.Test.Framework.Verifiers;

/// <summary>A test verifier that records whether <see cref="Verify"/> has been called.</summary>
internal sealed class TrackingVerifier(IStarWarsGameEngine engine, GameVerifySettings settings, IServiceProvider sp)
    : GameVerifier(engine, settings, sp)
{
    public bool WasInvoked { get; private set; }

    public override void Verify(CancellationToken token)
    {
        WasInvoked = true;
    }
}
