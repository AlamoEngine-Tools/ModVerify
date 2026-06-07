using System.Threading;
using Diagnostics = AET.ModVerify.Reporting.Diagnostics;
using AET.ModVerify.Verifiers.Commons;
using AET.ModVerify.Verifiers.Utilities;
using PG.StarWarsGame.Engine.CommandBar;

namespace AET.ModVerify.Verifiers.CommandBar;

partial class CommandBarVerifier
{
    private void VerifyMegaTexture(CancellationToken token)
    {
        if (CommandBar.MtdFile is null)
        {
            AddError(Diagnostics.CommandBar.MegaTextureDirectoryNotFound(this, CommandBarConstants.MegaTextureBaseName, []));
        }
        else
        {
            var dupVerifier = new DuplicateVerifier(this);
            dupVerifier.Verify(IDuplicateVerificationContext.CreateForMtd(CommandBar.MtdFile), [], token);

            foreach (var duplicateError in dupVerifier.VerifyErrors)
                AddError(duplicateError);
        }

        if (CommandBar.MegaTextureFileName is null)
        {
            AddError(Diagnostics.CommandBar.MegaTextureNotFound(this, CommandBarConstants.MegaTextureBaseName, []));
        }
        else if (!GameEngine.GameRepository.TextureRepository.FileExists(CommandBar.MegaTextureFileName))
        {
            AddError(Diagnostics.CommandBar.MegaTextureNotFound(this, CommandBarConstants.MegaTextureBaseName, []));
        }
    }
}