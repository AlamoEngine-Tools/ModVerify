using PG.StarWarsGame.Engine.CommandBar;
using System.Linq;
using System.Threading;
using AET.ModVerify.Reporting.Diagnostics;

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
                AddError(CommandBarErrors.UnsupportedComponent(this, component.Name, []));
            }
            else
            {
                occupiedComponentIds[component.Id] = true;
            }

            if (alreadyOccupied)
            {
                AddError(CommandBarErrors.DuplicateComponent(this, component.Name, component.Id, []));
            }

            VerifySingleComponent(component, token);
        }
    }
}