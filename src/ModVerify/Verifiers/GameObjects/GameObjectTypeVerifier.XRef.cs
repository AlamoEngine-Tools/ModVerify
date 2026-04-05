using System.Linq;
using AET.ModVerify.Reporting;
using PG.StarWarsGame.Engine.GameObjects;

namespace AET.ModVerify.Verifiers.GameObjects;

public sealed partial class GameObjectTypeVerifier
{
    private void VerifyXRefs(GameObjectType gameObjectType, string[] context)
    {
        if (!string.IsNullOrEmpty(gameObjectType.VariantOfExistingTypeName) && gameObjectType.VariantOfExistingType is null)
        {
            AddError(VerificationError.Create(
                this, 
                VerifierErrorCodes.MissingXRef,
                $"Missing base type '{gameObjectType.VariantOfExistingTypeName}' for GameObject '{gameObjectType.Name}'",
                VerificationSeverity.Critical,
                [..context, "VariantOfExistingType"],
                gameObjectType.VariantOfExistingTypeName));
        }

        VerifyCompanyUnits(gameObjectType, context);
    }

    private void VerifyCompanyUnits(GameObjectType gameObjectType, string[] context)
    {
        if (gameObjectType.GroundCompanyUnits.Count == 0)
            return;

        var uniqueCompanyUnits = gameObjectType.GroundCompanyUnits
            .Select(x => x.ToUpperInvariant())
            .Distinct();

        foreach (var companyUnit in uniqueCompanyUnits)
        {
            if (GameEngine.GameObjectTypeManager.FindObjectType(companyUnit) is null)
            {
                AddError(VerificationError.Create(this, VerifierErrorCodes.MissingXRef,
                    $"Missing company unit '{companyUnit}' for GameObject '{gameObjectType.Name}'",
                    VerificationSeverity.Critical, [..context, "CompanyUnits"], companyUnit));
            }
        }
    }
}