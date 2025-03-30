using System;
using System.Collections.Generic;
using System.Threading;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerify.Verifiers;

public abstract class GameVerifier<T>(
    IGameVerifierInfo? parent,
    IGameDatabase gameDatabase,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    : GameVerifierBase(parent, gameDatabase, settings, serviceProvider) where T : notnull
{
    public abstract void Verify(T toVerify, IReadOnlyCollection<string> contextInfo, CancellationToken token);
}

public abstract class GameVerifier(
    IGameVerifierInfo? parent,
    IGameDatabase gameDatabase,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    : GameVerifierBase(parent, gameDatabase, settings, serviceProvider)
{
    public abstract void Verify(CancellationToken token);
}