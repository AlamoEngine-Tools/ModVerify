using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers;
using AET.ModVerify.Verifiers.CommandBar;
using AET.ModVerify.Verifiers.Engine;
using AET.ModVerify.Verifiers.GameObjects;
using AET.ModVerify.Verifiers.GuiDialogs;
using AET.ModVerify.Verifiers.SfxEvents;
using PG.StarWarsGame.Engine;
using System;
using System.Collections.Generic;

namespace AET.ModVerify;

/// <summary>
/// Default implementation of <see cref="IGameVerifiersProvider"/> that provides
/// the default set of verifiers for verifying a game installation.
/// </summary>
public sealed class DefaultGameVerifiersProvider : IGameVerifiersProvider
{
    /// <summary>
    /// Returns the default set of verifiers to use for verifying a game installation.
    /// </summary>
    /// <inheritdoc/>
    public IEnumerable<GameVerifier> GetVerifiers(
        IStarWarsGameEngine gameEngine, 
        GameVerifySettings settings, 
        IServiceProvider serviceProvider)
    {
        //yield break;
        yield return new SfxEventVerifier(gameEngine, settings, serviceProvider);
        yield return new HardcodedAssetsVerifier(gameEngine, settings, serviceProvider);
        yield return new GuiDialogsVerifier(gameEngine, settings, serviceProvider);
        yield return new GameObjectTypeVerifier(gameEngine, settings, serviceProvider);
        yield return new CommandBarVerifier(gameEngine, settings, serviceProvider);
    }
}