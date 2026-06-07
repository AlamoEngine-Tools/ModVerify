using System;
using Diagnostics = AET.ModVerify.Reporting.Diagnostics;
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
            AddError(Diagnostics.CommandBar.NoShellsGroup(this, CommandBarConstants.ShellGroupName, []));

        if (shellGroups.Count > 1) 
            AddError(Diagnostics.CommandBar.ManyShellsGroups(this, CommandBarConstants.ShellGroupName, shellGroups));
    }

    private void VerifyShellGroup(CommandBarComponentGroup shellGroup)
    {
        foreach (var component in shellGroup.Components)
        {
            var shellComponent = component as CommandBarShellComponent;
            if (shellComponent?.Type is not CommandBarComponentType.Shell)
            {
                AddError(Diagnostics.CommandBar.NonShellInShellGroup(this, component.Name, CommandBarConstants.ShellGroupName, []));
            }
        }
    }
}