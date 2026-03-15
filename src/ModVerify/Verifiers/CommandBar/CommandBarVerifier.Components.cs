using AET.ModVerify.Reporting;
using PG.StarWarsGame.Engine.CommandBar;
using PG.StarWarsGame.Engine.CommandBar.Components;
using System;
using System.Linq;

namespace AET.ModVerify.Verifiers;

partial class CommandBarVerifier
{
    private void VerifyCommandBarComponents()
    {
        var occupiedComponentIds = SupportedCommandBarComponentData.GetComponentIdsForEngine(Repository.EngineType).Keys
            .ToDictionary(value => value, _ => false);

        foreach (var component in GameEngine.CommandBar.Components)
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

    private void VerifySingleComponent(CommandBarBaseComponent component)
    {
        VerifyCommandBarModel(component);
        VerifyComponentBone(component);
    }

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

        var model = GameEngine.PGRender.LoadModelAndAnimations(shellComponent.ModelPath.AsSpan(), null);
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