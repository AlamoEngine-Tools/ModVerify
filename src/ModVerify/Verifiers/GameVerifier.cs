using System;
using System.Collections.Generic;
using System.Threading;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Verifiers;

/// <summary>Provides the base class for verifiers that verify individual entities of a given type.</summary>
/// <typeparam name="T">The type of entity to verify.</typeparam>
public abstract class GameVerifier<T> : GameVerifierBase where T : notnull
{
    /// <summary>Initializes a new instance of the <see cref="GameVerifier{T}"/> class.</summary>
    /// <param name="parent">The parent verifier, or <see langword="null"/> if this is a root verifier.</param>
    /// <param name="gameEngine">The game engine to verify against.</param>
    /// <param name="settings">The settings that control the verification run.</param>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    protected GameVerifier(
        IGameVerifierInfo? parent,
        IStarWarsGameEngine gameEngine,
        GameVerifySettings settings,
        IServiceProvider serviceProvider) : base(parent, gameEngine, settings, serviceProvider)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="GameVerifier{T}"/> class as a child of the specified parent verifier.</summary>
    /// <param name="parent">The parent verifier whose engine, settings, and services are inherited.</param>
    protected GameVerifier(GameVerifierBase parent) : base(parent)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="GameVerifier{T}"/> class as a root verifier.</summary>
    /// <param name="gameEngine">The game engine to verify against.</param>
    /// <param name="settings">The settings that control the verification run.</param>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    protected GameVerifier(
        IStarWarsGameEngine gameEngine,
        GameVerifySettings settings,
        IServiceProvider serviceProvider) : base(gameEngine, settings, serviceProvider)
    {
    }

    /// <summary>Verifies the specified entity.</summary>
    /// <param name="toVerify">The entity to verify.</param>
    /// <param name="contextInfo">Context entries describing where the entity was found, used for error reporting.</param>
    /// <param name="token">The token to monitor for cancellation requests.</param>
    public abstract void Verify(T toVerify, IReadOnlyCollection<string> contextInfo, CancellationToken token);
}

/// <summary>Provides the base class for verifiers that perform a single verification pass.</summary>
public abstract class GameVerifier : GameVerifierBase
{
    /// <summary>Initializes a new instance of the <see cref="GameVerifier"/> class.</summary>
    /// <param name="parent">The parent verifier, or <see langword="null"/> if this is a root verifier.</param>
    /// <param name="gameEngine">The game engine to verify against.</param>
    /// <param name="settings">The settings that control the verification run.</param>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    protected GameVerifier(
        IGameVerifierInfo? parent,
        IStarWarsGameEngine gameEngine,
        GameVerifySettings settings,
        IServiceProvider serviceProvider)
        : base(parent, gameEngine, settings, serviceProvider)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="GameVerifier"/> class as a child of the specified parent verifier.</summary>
    /// <param name="parent">The parent verifier whose engine, settings, and services are inherited.</param>
    protected GameVerifier(GameVerifierBase parent) : base(parent)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="GameVerifier"/> class as a root verifier.</summary>
    /// <param name="gameEngine">The game engine to verify against.</param>
    /// <param name="settings">The settings that control the verification run.</param>
    /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
    protected GameVerifier(
        IStarWarsGameEngine gameEngine,
        GameVerifySettings settings,
        IServiceProvider serviceProvider)
        : base(gameEngine, settings, serviceProvider)
    {
    }

    /// <summary>Runs the verification.</summary>
    /// <param name="token">The token to monitor for cancellation requests.</param>
    public abstract void Verify(CancellationToken token);
}