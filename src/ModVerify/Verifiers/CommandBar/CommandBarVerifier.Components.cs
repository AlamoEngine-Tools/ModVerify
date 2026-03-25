using AET.ModVerify.Reporting;
using PG.StarWarsGame.Engine.CommandBar;
using System.Linq;
using System.Threading;

namespace AET.ModVerify.Verifiers.CommandBar;

partial class CommandBarVerifier
{
    private void VerifyCommandBarComponents(CancellationToken token, double startProgress)
    {
        var occupiedComponentIds = SupportedCommandBarComponentData
            .GetComponentIdsForEngine(Repository.EngineType).Keys
            .ToDictionary(value => value, _ => false);

        var counter = 0;
        var numEntities = GameEngine.CommandBar.Components.Count;
        var num = 1 - startProgress;
       
        foreach (var component in GameEngine.CommandBar.Components)
        {
            var progress = num + (++counter / (double)numEntities) * startProgress;
            OnProgress(progress, $"CommandBarComponent - '{component.Name}'");

            if (!occupiedComponentIds.TryGetValue(component.Id, out var alreadyOccupied))
            {
                AddError(VerificationError.Create(
                    this,
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
                AddError(VerificationError.Create(this,
                    VerifierErrorCodes.Duplicate,
                    $"The CommandBar component '{component.Name}' with ID '{component.Id}' already exists.",
                    VerificationSeverity.Warning,
                    component.Name));
            }

            VerifySingleComponent(component, token);
        }
    }
}