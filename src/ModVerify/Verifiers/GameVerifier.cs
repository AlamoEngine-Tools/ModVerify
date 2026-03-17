using System;
using System.Collections.Generic;
using System.Threading;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Verifiers;

public abstract class GameVerifier<T> : GameVerifierBase where T : notnull
{
    protected GameVerifier(
        IGameVerifierInfo? parent,
        IStarWarsGameEngine gameEngine,
        GameVerifySettings settings,
        IServiceProvider serviceProvider) : base(parent, gameEngine, settings, serviceProvider)
    {
    }
    protected GameVerifier(GameVerifierBase parent) : base(parent)
    {
    }

    protected GameVerifier(
        IStarWarsGameEngine gameEngine,
        GameVerifySettings settings,
        IServiceProvider serviceProvider) : base(gameEngine, settings, serviceProvider)
    {
    }

    public abstract void Verify(T toVerify, IReadOnlyCollection<string> contextInfo, CancellationToken token);
}

public abstract class GameVerifier : GameVerifierBase
{
    protected GameVerifier(
        IGameVerifierInfo? parent,
        IStarWarsGameEngine gameEngine,
        GameVerifySettings settings,
        IServiceProvider serviceProvider) 
        : base(parent, gameEngine, settings, serviceProvider)
    {
    }
    protected GameVerifier(GameVerifierBase parent) : base(parent)
    {
    }

    protected GameVerifier(
        IStarWarsGameEngine gameEngine,
        GameVerifySettings settings,
        IServiceProvider serviceProvider)
        : base(gameEngine, settings, serviceProvider)
    {
    }

    public abstract void Verify(CancellationToken token);
}