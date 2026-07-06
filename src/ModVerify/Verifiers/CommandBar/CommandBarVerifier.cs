using System;
using System.Linq;
using System.Threading;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers.Commons;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.CommandBar;

namespace AET.ModVerify.Verifiers.CommandBar;

public partial class CommandBarVerifier : GameVerifier
{ 
    private readonly SingleModelVerifier _modelVerifier;
    private readonly TextureVerifier _textureVerifier;

    public override string FriendlyName => "CommandBar";

    public ICommandBarGameManager CommandBar { get; }

    public CommandBarVerifier(IStarWarsGameEngine gameEngine, GameVerifySettings settings, IServiceProvider serviceProvider)
        : base(gameEngine, settings, serviceProvider)
    {
        CommandBar = gameEngine.CommandBar;
        _modelVerifier = new SingleModelVerifier(this);
        _textureVerifier = new TextureVerifier(this);
    }

    public override void Verify(CancellationToken token)
    {
        var progress = 0.0d;
        OnProgress(progress, "Verifying MegaTexture");
        VerifyMegaTexture(token);
        progress = 1 / 3.0;
        OnProgress(progress, "Verifying CommandBar Shell");
        VerifyCommandBarShellsGroups();
        progress = 2 / 3.0;
        OnProgress(progress, "Verifying CommandBar components");
        VerifyCommandBarComponents(token, progress);

        foreach (var subError in _modelVerifier.VerifyErrors.Concat(_textureVerifier.VerifyErrors))
            AddError(subError);

        progress = 1.0;
        OnProgress(progress, null);
    }
}