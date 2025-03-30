using System;
using System.Collections.Generic;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers;
using AET.ModVerify.Verifiers.GuiDialogs;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerify.Pipeline;

public sealed class DefaultGameVerifiersProvider : IGameVerifiersProvider
{
    public IEnumerable<GameVerifier> GetVerifiers(
        IGameDatabase database, 
        GameVerifySettings settings, 
        IServiceProvider serviceProvider)
    {
        yield return new ReferencedModelsVerifier(database, settings, serviceProvider);
        yield return new DuplicateNameFinder(database, settings, serviceProvider);
        yield return new AudioFilesVerifier(database, settings, serviceProvider);
        yield return new GuiDialogsVerifier(database, settings, serviceProvider);
        yield return new CommandBarVerifier(database, settings, serviceProvider);
    }
}