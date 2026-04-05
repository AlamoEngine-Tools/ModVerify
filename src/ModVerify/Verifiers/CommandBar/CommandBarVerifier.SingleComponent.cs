using AET.ModVerify.Reporting;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.CommandBar.Components;
using System.Threading;

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
            AddError(VerificationError.Create(this, VerifierErrorCodes.NameTooLong, 
                // Deliberately not reporting the buffer length as max, as it's considered to be internal data
                $"The CommandBarShellComponent name '{component.Name}' is too long. Maximum length is {PGConstants.MaxCommandBarComponentName}.",
                VerificationSeverity.Critical, [], component.Name));
        }
    }

    private void VerifyCommandBarModel(CommandBarBaseComponent component, CancellationToken token)
    {
        if (component is not CommandBarShellComponent shellComponent)
            return;

        if (shellComponent.ModelName is null)
        {
            AddError(VerificationError.Create(this,
                CommandBarShellNoModel, $"The CommandBarShellComponent '{component.Name}' has no model specified.",
                VerificationSeverity.Error, [shellComponent.Name], shellComponent.Name));
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
            AddError(VerificationError.Create(this,
                CommandBarShellNoModel, $"The CommandBar component '{component.Name}' is not connected to a shell component.",
                VerificationSeverity.Warning, component.Name));
        }
    }
}