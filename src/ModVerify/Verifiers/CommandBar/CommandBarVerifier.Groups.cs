using System;
using AET.ModVerify.Reporting;
using AnakinRaW.CommonUtilities.Collections;
using PG.StarWarsGame.Engine.CommandBar;
using PG.StarWarsGame.Engine.CommandBar.Components;

namespace AET.ModVerify.Verifiers.CommandBar;

partial class CommandBarVerifier
{
    private void VerifyCommandBarShellsGroups()
    {
        var shellGroups = new FrugalList<string>();
        foreach (var groupPair in GameEngine.CommandBar.Groups)
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

        if (shellGroups.Count > 1) 
            AddError(VerificationError.Create(this, 
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
                AddError(VerificationError.Create(this,
                    CommandBarNoShellsComponentInShellGroup, 
                    $"The CommandBar component '{component.Name}' is not a shell component, but part of the '{CommandBarConstants.ShellGroupName}' group.", 
                    VerificationSeverity.Warning, component.Name));
            }
        }
    }
}