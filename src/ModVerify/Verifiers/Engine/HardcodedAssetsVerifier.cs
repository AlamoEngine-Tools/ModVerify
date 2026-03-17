using System;
using System.Threading;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers.Commons;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Verifiers.Engine;

public sealed class HardcodedAssetsVerifier : GameVerifier
{
    private readonly SingleModelVerifier _modelVerifier;

    public HardcodedAssetsVerifier(IStarWarsGameEngine gameEngine, GameVerifySettings settings, IServiceProvider serviceProvider) 
        : base(gameEngine, settings, serviceProvider)
    {
        _modelVerifier = new SingleModelVerifier(this);
    }

    public override void Verify(CancellationToken token)
    {
        OnProgress(0.0d, "Verifying Hardcoded Models");
        VerifyModels(token);
        OnProgress(1.0, null);
    }

    private void VerifyModels(CancellationToken token)
    {
        var models = HardcodedEngineAssets.GetHardcodedModels(GameEngine.EngineType);

        foreach (var model in models) 
            _modelVerifier.Verify(model, [], token);

        foreach (var error in _modelVerifier.VerifyErrors) 
            AddError(error);
    }
}