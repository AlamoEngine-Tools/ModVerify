using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers.Commons;
using PG.StarWarsGame.Engine;
using System;
using System.Linq;
using System.Threading;

namespace AET.ModVerify.Verifiers;

public sealed class ReferencedModelsVerifier(
    IStarWarsGameEngine engine,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    : GameVerifier(null, engine, settings, serviceProvider)
{
    public override string FriendlyName => "Referenced Models";

    public override void Verify(CancellationToken token)
    {
        var gameObjectEntries = GameEngine.GameObjectTypeManager.Entries.ToList();
        var hardcodedModels = FocHardcodedConstants.HardcodedModels.ToList();

        var totalModelsCount =
            gameObjectEntries
                .Sum(x => GameEngine.GameObjectTypeManager.GetModels(x).Count())
            + hardcodedModels.Count;

        if (totalModelsCount == 0)
            return;

        var counter = 0;

        var inner = new SingleModelVerifier(this, GameEngine, Settings, Services);
        try
        {
            inner.Error += OnModelError;

            var context = new string[1];
            foreach (var gameObject in gameObjectEntries)
            {
                context[0] = $"GameObject: {gameObject.Name}";
                foreach (var model in GameEngine.GameObjectTypeManager.GetModels(gameObject))
                {
                    OnProgress((double)++counter / totalModelsCount, $"Model - '{model}'");
                    inner.Verify(model, context, token);
                }
            }

            context[0] = "Hardcoded Model";
            foreach (var hardcodedModel in hardcodedModels)
            {
                OnProgress((double)++counter / totalModelsCount, $"Model - '{hardcodedModel}'");
                inner.Verify(hardcodedModel, context, token);
            }
        }
        finally
        {
            inner.Error -= OnModelError;
        }
    }

    private void OnModelError(object sender, VerificationErrorEventArgs e)
    {
        AddError(e.Error);
    }
}