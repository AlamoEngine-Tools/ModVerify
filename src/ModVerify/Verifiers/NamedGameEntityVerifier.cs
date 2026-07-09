using System;
using System.Threading;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers.Commons;
using AET.ModVerify.Verifiers.Utilities;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Files.XML.Data;

namespace AET.ModVerify.Verifiers;

/// <summary>Provides the base class for verifiers that iterate the named entities of a game manager, checking each entity and its duplicates.</summary>
/// <typeparam name="T">The type of named XML entity to verify.</typeparam>
/// <param name="gameEngine">The game engine to verify against.</param>
/// <param name="settings">The settings that control the verification run.</param>
/// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
public abstract partial class NamedGameEntityVerifier<T>(
    IStarWarsGameEngine gameEngine,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    : GameVerifier(null, gameEngine, settings, serviceProvider)
    where T : NamedXmlObject
{
    /// <summary>Gets the game manager that provides the entities to verify.</summary>
    public abstract IGameManager<T> GameManager { get; }

    /// <summary>Gets the display name of the entity type being verified.</summary>
    public abstract string EntityTypeName { get; }

    /// <inheritdoc />
    public sealed override void Verify(CancellationToken token)
    {
        OnProgress(0.0, $"Verifying GameManager for '{EntityTypeName}'");
        PreEntityVerify(token);
        OnProgress(0.5, null);

        var numEntities = GameEngine.GameObjectTypeManager.Entries.Count;
        double counter = 0;
        var context = new string[1];
        foreach (var gameEntity in GameManager.Entries)
        {
            LogVerifyingEntityTypeName(Logger, EntityTypeName, gameEntity.Name);
            var progress = 0.5 + ++counter / numEntities * 0.5;
            OnProgress(progress, $"{EntityTypeName} - '{gameEntity.Name}'");
            context[0] = gameEntity.Name;
            VerifyEntity(gameEntity, context, progress, token);
        }

        PostEntityVerify(token);
    }

    /// <summary>Performs verification after all entities have been verified.</summary>
    /// <param name="token">The token to monitor for cancellation requests.</param>
    /// <remarks>The default implementation does nothing.</remarks>
    protected virtual void PostEntityVerify(CancellationToken token)
    {
    }

    /// <summary>Verifies a single entity.</summary>
    /// <param name="entity">The entity to verify.</param>
    /// <param name="context">The context entries describing the entity, used for error reporting.</param>
    /// <param name="progress">The current progress value, between 0.0 and 1.0.</param>
    /// <param name="token">The token to monitor for cancellation requests.</param>
    protected abstract void VerifyEntity(T entity, string[] context, double progress, CancellationToken token);

    /// <summary>Performs verification before any entity is verified.</summary>
    /// <param name="token">The token to monitor for cancellation requests.</param>
    /// <remarks>The default implementation checks the game manager for duplicate entries.</remarks>
    protected virtual void PreEntityVerify(CancellationToken token)
    {
        VerifyDuplicates(token);
    }

    private void VerifyDuplicates(CancellationToken token)
    {
        LogCheckingEntityTypeForDuplicateEntries(Logger, EntityTypeName);
        var context = IDuplicateVerificationContext.CreateForNamedXmlObjects(GameManager, EntityTypeName);
        var verifier = new DuplicateVerifier(this, GameEngine, Settings, Services);
        verifier.Verify(context, [], token);
        foreach (var error in verifier.VerifyErrors)
            AddError(error);
    }

    [LoggerMessage(LogLevel.Trace, "Verifying {entityType} - '{name}'")]
    static partial void LogVerifyingEntityTypeName(ILogger? logger, string entityType, string name);

    [LoggerMessage(LogLevel.Debug, "Checking {entityType} for duplicate entries")]
    static partial void LogCheckingEntityTypeForDuplicateEntries(ILogger logger, string entityType);
}