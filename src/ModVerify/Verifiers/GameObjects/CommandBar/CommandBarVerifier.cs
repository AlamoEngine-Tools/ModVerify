using System;
using System.Threading;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Verifiers.CommandBar;

public partial class CommandBarVerifier(IStarWarsGameEngine gameEngine, GameVerifySettings settings, IServiceProvider serviceProvider)
    : GameVerifier(null, gameEngine, settings, serviceProvider)
{
    public const string CommandBarNoShellsGroup = "CMDBAR00";
    public const string CommandBarManyShellsGroup = "CMDBAR01";
    public const string CommandBarNoShellsComponentInShellGroup = "CMDBAR02";
    public const string CommandBarUnsupportedComponent = "CMDBAR03";
    public const string CommandBarShellNoModel = "CMDBAR04";

    public override string FriendlyName => "CommandBar";

    public override void Verify(CancellationToken token)
    {
        OnProgress(0.0, "Verifying CommandBar Shell");
        VerifyCommandBarShellsGroups();
        OnProgress(0.5d, "Verifying CommandBar components");
        VerifyCommandBarComponents();
        OnProgress(1.0d, null);
    }
}