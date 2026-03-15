using System;
using System.Threading;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers.Commons;
using AET.ModVerify.Verifiers.Utilities;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Verifiers.GameObjects;

public sealed class GameObjectTypeVerifier(
    IGameVerifierInfo? parent,
    IStarWarsGameEngine gameEngine,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    : GameVerifier(parent, gameEngine, settings, serviceProvider)
{
    public override string FriendlyName => "GameObjectType Verifier";

    public override void Verify(CancellationToken token)
    {
        var context = IDuplicateVerificationContext.CreateForNamedXmlObjects(GameEngine.GameObjectTypeManager, "GameObjectType");
        var verifier = new DuplicateVerifier(this, GameEngine, Settings, Services);
        verifier.Verify(context, [], token);
        foreach (var error in verifier.VerifyErrors)
            AddError(error);
    }
}
