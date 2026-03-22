using System.Linq;
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
                this, 
                VerifierErrorCodes.MissingXRef,
                $"Missing base type '{gameObject.VariantOfExistingTypeName}' for GameObject '{gameObject.Name}'",
                VerificationSeverity.Critical,
                [..context, "VariantOfExistingType"],
                gameObject.VariantOfExistingTypeName));
        }

        VerifyCompanyUnits(gameObject, context);
    }

    private void VerifyCompanyUnits(GameObject gameObject, string[] context)
    {
        if (gameObject.GroundCompanyUnits.Count == 0)
            return;

        var uniqueCompanyUnits = gameObject.GroundCompanyUnits
            .Select(x => x.ToUpperInvariant())
            .Distinct();

        foreach (var companyUnit in uniqueCompanyUnits)
        {
            if (GameEngine.GameObjectTypeManager.FindObjectType(companyUnit) is null)
            {
                AddError(VerificationError.Create(this, VerifierErrorCodes.MissingXRef,
                    $"Missing company unit '{companyUnit}' for GameObject '{gameObject.Name}'",
                    VerificationSeverity.Critical, [..context, "CompanyUnits"], companyUnit));
            }
        }
    }
}