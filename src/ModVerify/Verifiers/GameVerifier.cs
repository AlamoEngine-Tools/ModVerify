using System;
using System.Collections.Generic;
using System.Threading;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Verifiers;

public abstract class GameVerifier<T>(
    IGameVerifierInfo? parent,
    IStarWarsGameEngine gameEngine,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    : GameVerifierBase(parent, gameEngine, settings, serviceProvider) where T : notnull
{
    public abstract void Verify(T toVerify, IReadOnlyCollection<string> contextInfo, CancellationToken token);
}

public abstract class GameVerifier(
    IGameVerifierInfo? parent,
    IStarWarsGameEngine gameEngine,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    : GameVerifierBase(parent, gameEngine, settings, serviceProvider)
{
    public abstract void Verify(CancellationToken token);
}