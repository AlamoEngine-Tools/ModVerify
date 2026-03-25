using AET.ModVerify.Reporting;
using PG.StarWarsGame.Engine.GameObjects;

namespace AET.ModVerify.Verifiers.GameObjects;

public sealed partial class GameObjectTypeVerifier
{
    private void VerifyIcons(GameObject gameObject, string[] context)
    {
        VerifyObjectIcon(gameObject, context);
    }

    private void VerifyObjectIcon(GameObject gameObject, string[] context)
    {
        if (string.IsNullOrEmpty(gameObject.IconName))
            return;

        /*
         * The engine loads game object icons with different strategies, depending on where the icon is displayed:
         * 1.   the game loads the texture from MTD and supports the faction prefixes e.g, r_ or e_
         *      Faction prefixes have higher priority than the non-prefix versions. The player's faction (not the object owner) is used.
         *      This applies to all command bar components (such as build buttons)
         * 2.   the game loads the texture form MTD and does NOT support faction prefix.
         *      If the texture is not found, the game searches the texture with forced .dds name in the Textures folder.
         *      This applies to the GUI dialogs (such as battle summary)
         * 3.   the game only loads the texture from MTD NOT supporting faction prefix nor textures folder fallback.
         *      This applies (only) to the neutralize hero dialog
         *
         * We only verify whether the icon exists in the MTD data, as this is the primary case to all strategies
         * (and it's what really should only be used for mods)
         * Faction-specific icons are not verified as they are statically not decidable.
         */

        if (!GameEngine.CommandBar.IconExists(gameObject))
        {
            AddError(VerificationError.Create(this, VerifierErrorCodes.FileNotFound, 
                $"Could not find icon '{gameObject.IconName}' for game object type '{gameObject.Name}'.",
                VerificationSeverity.Warning, context, gameObject.IconName!));
        }
    }
}