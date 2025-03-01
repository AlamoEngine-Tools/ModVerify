using System;
using System.Collections.Generic;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerify.Verifiers;

internal sealed class GameEngineErrorCollector(
    IDatabaseErrorCollection errorCollection,
    IGameDatabase gameDatabase,
    GameVerifySettings settings,
    IServiceProvider serviceProvider) : GameVerifierBase(gameDatabase, settings, serviceProvider)
{
    public override string FriendlyName => "Reporting Game Engine Errors";

    protected override void RunVerification(CancellationToken token)
    { 
        AddErrors(new InitializationErrorReporter(Repository, Services).GetErrors(errorCollection.InitializationErrors));
        AddErrors(new XmlParseErrorReporter(Repository, Services).GetErrors(errorCollection.XmlErrors));
        if (Settings.GlobalReportSettings.ReportAsserts) 
            AddErrors(new GameAssertErrorReporter(Repository, Services).GetErrors(errorCollection.Asserts));
    }

    private void AddErrors(IEnumerable<VerificationError> errors)
    {
        foreach (var error in errors) 
            AddError(error);
    }
}