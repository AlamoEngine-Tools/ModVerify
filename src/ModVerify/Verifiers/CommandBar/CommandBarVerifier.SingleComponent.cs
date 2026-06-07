using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.CommandBar.Components;
using System.Threading;
using Diagnostics = AET.ModVerify.Reporting.Diagnostics;

namespace AET.ModVerify.Verifiers.CommandBar;

partial class CommandBarVerifier
{ 
    private void VerifySingleComponent(CommandBarBaseComponent component, CancellationToken token)
    {
        VerifyName(component);
        VerifyCommandBarModel(component, token);
        VerifyComponentBone(component);

        // TODO: Textures
    }

    private void VerifyName(CommandBarBaseComponent component)
    {
        if (component.Name.Length > PGConstants.MaxCommandBarComponentNameBuffer)
        {
            // Deliberately not reporting the buffer length as max, as it's considered to be internal data
            AddError(Diagnostics.CommandBar.ComponentNameTooLong(this, component.Name, PGConstants.MaxCommandBarComponentName, []));
        }
    }

    private void VerifyCommandBarModel(CommandBarBaseComponent component, CancellationToken token)
    {
        if (component is not CommandBarShellComponent shellComponent)
            return;

        if (shellComponent.ModelName is null)
        {
            AddError(Diagnostics.CommandBar.ShellNoModel(this, shellComponent.Name, [shellComponent.Name]));
            return;
        }
        
        _modelVerifier.VerifyModel(shellComponent.ModelName, [shellComponent.Name], token);
    }

    private void VerifyComponentBone(CommandBarBaseComponent component)
    {
        if (component is CommandBarShellComponent)
            return;

        if (component.Bone == -1)
        {
            AddError(Diagnostics.CommandBar.ComponentNotConnected(this, component.Name, []));
        }
    }
}