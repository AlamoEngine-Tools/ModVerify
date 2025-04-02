using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers.Commons;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Engine;
using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace AET.ModVerify.Verifiers;

public sealed class ReferencedModelsVerifier(
    IStarWarsGameEngine engine,
    GameVerifySettings settings,
    IServiceProvider serviceProvider)
    : GameVerifier(null, engine, settings, serviceProvider)
{
    public override string FriendlyName => "Referenced Models";

    private readonly ILogger? _logger =
        serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(ReferencedModelsVerifier));

    public override void Verify(CancellationToken token)
    {
        var models = GameEngine.GameObjectTypeManager.Entries
            .SelectMany(x => x.Models)
            .Concat(FocHardcodedConstants.HardcodedModels);


        var inner = new SingleModelVerifier(this, GameEngine, Settings, Services);
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
