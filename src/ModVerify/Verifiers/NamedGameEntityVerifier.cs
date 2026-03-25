using System;
using System.Threading;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers.Commons;
using AET.ModVerify.Verifiers.Utilities;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Files.XML.Data;

namespace AET.ModVerify.Verifiers;

public abstract partial class NamedGameEntityVerifier<T>(
    IStarWarsGameEngine gameEngine,
    GameVerifySettings settings,
    IServiceProvider serviceProvider) 
    : GameVerifier(null, gameEngine, settings, serviceProvider)
    where T : NamedXmlObject
{
    public abstract IGameManager<T> GameManager { get; }

    public abstract string EntityTypeName { get; }

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

    protected virtual void PostEntityVerify(CancellationToken token)
    {
    }

    protected abstract void VerifyEntity(T entity, string[] context, double progress, CancellationToken token);

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