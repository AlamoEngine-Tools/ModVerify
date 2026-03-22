using System;
using System.Threading;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers.Commons;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Engine.GameObjects;

namespace AET.ModVerify.Verifiers.GameObjects;

// TODO: Add GameObjectTypeVerifier and check that LandModelTerrainOverride is correct (all keys correct, no dups)
public sealed partial class GameObjectTypeVerifier : NamedGameEntityVerifier<GameObject>
{
    private readonly SingleModelVerifier _singleModelVerifier;

    public override string FriendlyName => "GameObjectType Verifier";

    public override IGameManager<GameObject> GameManager => GameEngine.GameObjectTypeManager;

    public override string EntityTypeName => "GameObjectType";

    public GameObjectTypeVerifier(
        IStarWarsGameEngine gameEngine,
        GameVerifySettings settings,
        IServiceProvider serviceProvider) 
        : base(gameEngine, settings, serviceProvider)
    {
        _singleModelVerifier = new SingleModelVerifier(this);
    }

    protected override void VerifyEntity(GameObject entity, string[] context, double progress, CancellationToken token)
    {
        if (entity.Name.Length >= PGConstants.MaxGameObjectTypeName)
        {
            AddError(VerificationError.Create(this, VerifierErrorCodes.NameTooLong,
                $"The GameObjectType name '{entity.Name}' is too long. Maximum length is {PGConstants.MaxGameObjectTypeName}.", 
                VerificationSeverity.Critical, entity.Name));
        }
        VerifyXRefs(entity, context);
        VerifyModels(entity, context, token);
        VerifyIcons(entity, context);
    }

    protected override void PostEntityVerify(CancellationToken token)
    {
        foreach (var modelError in _singleModelVerifier.VerifyErrors)
            AddError(modelError);
    }
}