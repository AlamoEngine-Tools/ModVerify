using System;
using System.Collections.Generic;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify;

/// <summary>
/// Defines a provider for game verifiers.
/// </summary>
public interface IGameVerifiersProvider
{
    /// <summary>
    /// Returns an enumerable collection of game verifiers for the specified game engine, settings, and service provider.
    /// </summary>
    /// <param name="gameEngine">The game engine for which to provide verifiers.</param>
    /// <param name="settings">The verification settings.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>A <see cref="IEnumerable{GameVerifier}"/> of game verifiers.</returns>
    IEnumerable<GameVerifier> GetVerifiers(
        IStarWarsGameEngine gameEngine, 
        GameVerifySettings settings,
        IServiceProvider serviceProvider);
}