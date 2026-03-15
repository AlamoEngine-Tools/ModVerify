using System;
using System.Threading;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify.Verifiers;

public partial class CommandBarVerifier(IStarWarsGameEngine gameEngine, GameVerifySettings settings, IServiceProvider serviceProvider)
    : GameVerifier(null, gameEngine, settings, serviceProvider)
{
    public const string CommandBarNoShellsGroup = "CMDBAR00";
    public const string CommandBarManyShellsGroup = "CMDBAR01";
    public const string CommandBarNoShellsComponentInShellGroup = "CMDBAR02";
    public const string CommandBarDuplicateComponent = "CMDBAR03";
    public const string CommandBarUnsupportedComponent = "CMDBAR04";
    public const string CommandBarShellNoModel = "CMDBAR05";

    public override string FriendlyName => "CommandBar";

    public override void Verify(CancellationToken token)
    {
        VerifyCommandBarShellsGroups();
        VerifyCommandBarComponents();
    }
}