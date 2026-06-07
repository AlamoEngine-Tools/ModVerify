using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers.Commons;
using PG.StarWarsGame.Engine;
using System;
using System.Threading;
using Diagnostics = AET.ModVerify.Reporting.Diagnostics;

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
        OnProgress(0.0d, "Verifying Hardcoded Shaders");
        VerifyShaders(token);
        OnProgress(0.5d, "Verifying Hardcoded Models");
        VerifyModels(token);
        OnProgress(1.0, null);
    }

    private void VerifyModels(CancellationToken token)
    {
        var models = HardcodedEngineAssets.GetHardcodedModelsAndParticles(GameEngine.EngineType);

        foreach (var model in models) 
            _modelVerifier.VerifyAlamoFile(model, [], token);

        foreach (var error in _modelVerifier.VerifyErrors) 
            AddError(error);
    }

    // TODO: Create a shader verifier that reports a warning if a shader is located at the game's root 
    //  as this can cause compatibility issues with mods and in general is not recommended.
    
    private void VerifyShaders(CancellationToken token)
    {
        var repo = GameEngine.GameRepository.EffectsRepository;
        // The engine loads the following shaders at startup
        foreach (var shadersName in HardcodedEngineAssets.HardcodedEngineShadersNames)
        {
            token.ThrowIfCancellationRequested();
            
            if (!repo.FileExists(shadersName))
                AddError(Diagnostics.HardcodedAssets.ShaderNotFound(this, shadersName, []));
        }

        // The engine loads the following shaders on terrain load. For simplicity, we try to find them once here
        foreach (var shadersName in HardcodedEngineAssets.HardcodedTerrainShadersNames)
        {
            token.ThrowIfCancellationRequested();

            if (!repo.FileExists(shadersName))
                AddError(Diagnostics.HardcodedAssets.TerrainShaderNotFound(this, shadersName, []));
        }
    }
}