using AET.ModVerify.Reporting;
using PG.StarWarsGame.Engine.GameObjects;

namespace AET.ModVerify.Verifiers.GameObjects;

public sealed partial class GameObjectTypeVerifier
{
    private void VerifyXRefs(GameObject gameObject, string[] context)
    {
        if (!string.IsNullOrEmpty(gameObject.VariantOfExistingTypeName) && gameObject.VariantOfExistingType is null)
        {
            AddError(VerificationError.Create(
                VerifierChain, 
                VerifierErrorCodes.MissingXRef,
                $"Missing base type '{gameObject.VariantOfExistingTypeName}' for GameObject '{gameObject.Name}'",
                VerificationSeverity.Critical, 
                context,
                gameObject.VariantOfExistingTypeName));
        }
    }
}