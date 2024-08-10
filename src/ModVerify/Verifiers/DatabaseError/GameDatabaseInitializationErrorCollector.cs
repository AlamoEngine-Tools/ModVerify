using System;
using System.Collections.Generic;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerify.Verifiers;

internal sealed class GameDatabaseInitializationErrorCollector(
    IDatabaseErrorCollection errorCollection,
    IGameDatabase gameDatabase,
    GameVerifySettings settings,
    IServiceProvider serviceProvider) : GameVerifierBase(gameDatabase, settings, serviceProvider)
{
    public override string FriendlyName => "Reporting Game Initialization Errors";

    protected override void RunVerification(CancellationToken token)
    { 
        AddErrors(new XmlParseErrorReporter(Repository, Services).GetErrors(errorCollection.XmlErrors));
    }

    private void AddErrors(IEnumerable<VerificationError> errors)
    {
        foreach (var error in errors) 
            AddError(error);
    }
}