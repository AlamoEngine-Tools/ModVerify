using System;
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
    : GameVerifier(null, gameDatabase, settings, serviceProvider)
{
    public const string CommandBarNoShellsGroup = "CMDBAR00";
    public const string CommandBarManyShellsGroup = "CMDBAR01";
    public const string CommandBarNoShellsComponentInShellGroup = "CMDBAR02";
    public const string CommandBarDuplicateComponent = "CMDBAR03";
    public const string CommandBarUnsupportedComponent = "CMDBAR04";
    public const string CommandBarShellNoModel = "CMDBAR05";

    public override string FriendlyName => "CommandBar Verifiers";

    public override void Verify(CancellationToken token)
    {
        VerifyCommandBarShellsGroups();
        VerifyCommandBarComponents();
    }

    private void VerifySingleComponent(CommandBarBaseComponent component)
    {
        VerifyCommandBarModel(component);
        VerifyComponentBone(component);
    }
}

partial class CommandBarVerifier
{
    private void VerifyCommandBarModel(CommandBarBaseComponent component)
    {
        if (component is not CommandBarShellComponent shellComponent)
            return;

        if (shellComponent.ModelPath is null)
        {
            AddError(VerificationError.Create(VerifierChain,
                CommandBarShellNoModel, $"The CommandBarShellComponent '{component.Name}' has no model specified.",
                VerificationSeverity.Error, shellComponent.Name));
            return;
        }

        var model = Database.PGRender.LoadModelAndAnimations(shellComponent.ModelPath.AsSpan(), null);
        if (model is null)
        {
            AddError(VerificationError.Create(VerifierChain,
                CommandBarShellNoModel, $"Could not find model '{shellComponent.ModelPath}' for CommandBarShellComponent '{component.Name}'.",
                VerificationSeverity.Error, [shellComponent.Name], shellComponent.ModelPath));
            return;
        }
    }

    private void VerifyComponentBone(CommandBarBaseComponent component)
    {
        if (component is CommandBarShellComponent)
            return;

        if (component.Bone == -1)
        {
            AddError(VerificationError.Create(VerifierChain,
                CommandBarShellNoModel, $"The CommandBar component '{component.Name}' is not connected to a shell component.",
                VerificationSeverity.Warning, component.Name));
        }
    }
}

partial class CommandBarVerifier
{
    private void VerifyCommandBarComponents()
    {
        var occupiedComponentIds = SupportedCommandBarComponentData.GetComponentIdsForEngine(Repository.EngineType).Keys
            .ToDictionary(value => value, _ => false);

        foreach (var component in Database.CommandBar.Components)
        {
            if (!occupiedComponentIds.TryGetValue(component.Id, out var alreadyOccupied))
            {
                AddError(VerificationError.Create(
                    VerifierChain,
                    CommandBarUnsupportedComponent,
                    $"The CommandBar component '{component.Name}' is not supported by the game.",
                    VerificationSeverity.Information,
                    component.Name));
            }
            else
            {
                occupiedComponentIds[component.Id] = true;
            }

            if (alreadyOccupied)
            {
                AddError(VerificationError.Create(VerifierChain,
                    CommandBarDuplicateComponent,
                    $"The CommandBar component '{component.Name}' with ID '{component.Id}' already exists.",
                    VerificationSeverity.Warning,
                    component.Name));
            }

            VerifySingleComponent(component);
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
            AddError(VerificationError.Create(VerifierChain,
                CommandBarNoShellsGroup, 
                $"No CommandBarGroup '{CommandBarConstants.ShellGroupName}' found.", 
                VerificationSeverity.Error, 
                "GameCommandBar"));

        if (shellGroups.Count > 1) 
            AddError(VerificationError.Create(VerifierChain, 
                CommandBarManyShellsGroup, 
                $"Found more than one Shells CommandBarGroup. Mind that group names are case-sensitive. Correct name is '{CommandBarConstants.ShellGroupName}'",
                VerificationSeverity.Warning, 
                shellGroups, "GameCommandBar"));
    }

    private void VerifyShellGroup(CommandBarComponentGroup shellGroup)
    {
        foreach (var component in shellGroup.Components)
        {
            var shellComponent = component as CommandBarShellComponent;
            if (shellComponent?.Type is not CommandBarComponentType.Shell)
            {
                AddError(VerificationError.Create(VerifierChain,
                    CommandBarNoShellsComponentInShellGroup, 
                    $"The CommandBar component '{component.Name}' is not a shell component, but part of the '{CommandBarConstants.ShellGroupName}' group.", 
                    VerificationSeverity.Warning, component.Name));
            }
        }
    }
}