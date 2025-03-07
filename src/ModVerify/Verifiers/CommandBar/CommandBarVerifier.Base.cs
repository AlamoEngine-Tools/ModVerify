using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AnakinRaW.CommonUtilities.Collections;
using PG.StarWarsGame.Engine.CommandBar;
using PG.StarWarsGame.Engine.CommandBar.Components;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerify.Verifiers;

public partial class CommandBarVerifier(IGameDatabase gameDatabase, GameVerifySettings settings, IServiceProvider serviceProvider)
    : GameVerifierBase(gameDatabase, settings, serviceProvider)
{
    public const string CommandBarNoShellsGroup = "CMDBAR00";
    public const string CommandBarManyShellsGroup = "CMDBAR01";
    public const string CommandBarNoShellsComponentInShellGroup = "CMDBAR02";
    public const string CommandBarDuplicateComponent = "CMDBAR03";
    public const string CommandBarUnsupportedComponent = "CMDBAR04";

    public override string FriendlyName => "CommandBar Verifiers";

    protected override void RunVerification(CancellationToken token)
    {
        VerifyCommandBarShellsGroups();
        VerifyCommandBarComponents();
        VerifyCommandBarModels();
    }
}

partial class CommandBarVerifier
{
    private void VerifyCommandBarModels()
    {
        foreach (var component in Database.CommandBar.Components)
        {
        }
    }
}

partial class CommandBarVerifier
{
    private void VerifyCommandBarComponents()
    {
        var occupiedComponentIds = Enum.GetValues(typeof(CommandBarComponentId))
            .Cast<CommandBarComponentId>()
            .ToDictionary(value => value, _ => false);

        foreach (var component in Database.CommandBar.Components)
        {
            if (occupiedComponentIds[component.Id])
            {
                AddError(VerificationError.Create(this, 
                    CommandBarDuplicateComponent,
                    $"The CommandBar component '{component.Name}' with ID '{component.Id}' already exists.",
                    VerificationSeverity.Warning, 
                    component.Name));
            }


            // TODO: Foc supports more types. The verifier should be aware of that.
            if (component.Id is CommandBarComponentId.None or CommandBarComponentId.Count)
            {
                AddError(VerificationError.Create(this,
                    CommandBarUnsupportedComponent,
                    $"The CommandBar component '{component.Name}' is not supported by the game.",
                    VerificationSeverity.Information, 
                    component.Name));
            }
        }

        // TODO: Foc supports more types. The verifier should be aware of that.
        var missingComponents = occupiedComponentIds
            .Where(x => x is { Value: false, Key: not CommandBarComponentId.None and not CommandBarComponentId.Count })
            .Select(x => x.Key);

        foreach (var componentId in missingComponents)
        {
            AddError(VerificationError.Create(this,
                CommandBarUnsupportedComponent,
                $"The CommandBar is missing the required component id, which is named '{componentId}' in XML.",
                VerificationSeverity.Error,
                componentId.ToString()));
        }
    }
}

partial class CommandBarVerifier
{
    private void VerifyCommandBarShellsGroups()
    {
        var shellGroups = new FrugalList<string>();
        foreach (var groupPair in Database.CommandBar.Groups)
        {
            if (groupPair.Key == CommandBarConstants.ShellGroupName)
            {
                shellGroups.Add(groupPair.Key);
                VerifyShellGroup(groupPair.Value);
            }
            else if (groupPair.Key.Equals(CommandBarConstants.ShellGroupName, StringComparison.OrdinalIgnoreCase))
            {
                shellGroups.Add(groupPair.Key);
            }
        }

        if (shellGroups.Count == 0) 
            AddError(VerificationError.Create(this,
                CommandBarNoShellsGroup, 
                $"No CommandBarGroup '{CommandBarConstants.ShellGroupName}' found.", 
                VerificationSeverity.Error, 
                "GameCommandBar"));

        if (shellGroups.Count >= 1) 
            AddError(VerificationError.Create(this, 
                CommandBarManyShellsGroup, 
                $"Found more than one Shells CommandBarGroup. Mind that group names are case-sensitive. Correct name is '{CommandBarConstants.ShellGroupName}'",
                VerificationSeverity.Warning, 
                shellGroups.Concat(["GameCommandBar"])));
    }

    private void VerifyShellGroup(CommandBarComponentGroup shellGroup)
    {
        foreach (var shellComponent in shellGroup.Components)
        {
            if (shellComponent is not CommandBarShellComponent || shellComponent.Type is not CommandBarComponentType.Shell)
                AddError(VerificationError.Create(this,
                    CommandBarNoShellsComponentInShellGroup, 
                    $"The CommandBar component '{shellComponent.Name}' is not a shell component, but part of the '{CommandBarConstants.ShellGroupName}' group.", 
                    VerificationSeverity.Warning));
        }
    }
}