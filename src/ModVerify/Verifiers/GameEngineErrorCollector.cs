using System;
using System.Collections.Generic;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Reporters.Engine;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Verifiers;

public sealed class GameEngineErrorCollector(
    IGameEngineErrorCollection errorCollection,
    IStarWarsGameEngine gameEngine,
    GameVerifySettings settings,
    IServiceProvider serviceProvider) : GameVerifier(null, gameEngine, settings, serviceProvider)
{
    public override string FriendlyName => "Game Engine Initialization";

    public override void Verify(CancellationToken token)
    {
        AddErrors(new InitializationErrorReporter(Repository, Services).GetErrors(errorCollection.InitializationErrors));
        AddErrors(new XmlParseErrorReporter(Repository, Services).GetErrors(errorCollection.XmlErrors));
        if (!Settings.IgnoreAsserts)
            AddErrors(new GameAssertErrorReporter(Repository, Services).GetErrors(errorCollection.Asserts));
    }

    private void AddErrors(IEnumerable<VerificationError> errors)
    {
        foreach (var error in errors)
            AddError(error);
    }
}