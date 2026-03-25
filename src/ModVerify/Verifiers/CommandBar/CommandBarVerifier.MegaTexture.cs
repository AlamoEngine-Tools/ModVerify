using System.Threading;
using AET.ModVerify.Reporting;
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
            AddError(VerificationError.Create(this, VerifierErrorCodes.FileNotFound,
                $"Cannot find CommandBar MegaTextureDirectory '{CommandBarConstants.MegaTextureBaseName}.mtd'", 
                VerificationSeverity.Critical, $"{CommandBarConstants.MegaTextureBaseName}.mtd"));
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
            AddError(VerificationError.Create(this, VerifierErrorCodes.FileNotFound,
                $"Cannot find CommandBar MegaTexture '{CommandBarConstants.MegaTextureBaseName}.tga'",
                VerificationSeverity.Critical, $"{CommandBarConstants.MegaTextureBaseName}.tga"));
        }
        else if (!GameEngine.GameRepository.TextureRepository.FileExists(CommandBar.MegaTextureFileName))
        {
            AddError(VerificationError.Create(this, VerifierErrorCodes.FileNotFound,
                $"Cannot find CommandBar MegaTexture '{CommandBarConstants.MegaTextureBaseName}.tga'",
                VerificationSeverity.Critical, $"{CommandBarConstants.MegaTextureBaseName}.tga"));
        }
    }
}