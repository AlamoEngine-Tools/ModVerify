using System;
using System.Linq;
using System.Threading;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.Database;
using AET.ModVerify.Verifiers.Commons;

namespace AET.ModVerify.Verifiers;

public sealed class ReferencedModelsVerifier(
    IGameDatabase database,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    : GameVerifier(null, database, settings, serviceProvider)
{
    public override string FriendlyName => "Referenced Models";

    public override void Verify(CancellationToken token)
    {
        var models = Database.GameObjectTypeManager.Entries
            .SelectMany(x => x.Models)
            .Concat(FocHardcodedConstants.HardcodedModels);


        var inner = new SingleModelVerifier(this, Database, Settings, Services);
        try
        {
            inner.Error += OnModelError;
            foreach (var model in models)
            {
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
