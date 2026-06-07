using System.Linq;
using Diagnostics = AET.ModVerify.Reporting.Diagnostics;
using PG.StarWarsGame.Engine.GameObjects;

namespace AET.ModVerify.Verifiers.GameObjects;

public sealed partial class GameObjectTypeVerifier
{
    private void VerifyXRefs(GameObjectType gameObjectType, string[] context)
    {
        if (!string.IsNullOrEmpty(gameObjectType.VariantOfExistingTypeName) && gameObjectType.VariantOfExistingType is null)
        {
            AddError(Diagnostics.GameObjects.MissingBaseType(this, gameObjectType.VariantOfExistingTypeName,
                gameObjectType.Name, [..context, "VariantOfExistingType"]));
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
                AddError(Diagnostics.GameObjects.MissingCompanyUnit(this, companyUnit,
                    gameObjectType.Name, [..context, "CompanyUnits"]));
            }
        }
    }
}