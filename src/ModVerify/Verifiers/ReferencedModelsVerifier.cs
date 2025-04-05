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
        var models = GameEngine.GameObjectTypeManager.Entries
            .SelectMany(x => x.Models)
            .Concat(FocHardcodedConstants.HardcodedModels).ToList();

        if (models.Count == 0)
            return;

        var numModels = models.Count;
        var counter = 0;

        var inner = new SingleModelVerifier(this, GameEngine, Settings, Services);
        try
        {
            inner.Error += OnModelError;
            foreach (var model in models)
            {
                OnProgress((double)++counter / numModels, $"Model - '{model}'");
                inner.Verify(model, [], token);
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
