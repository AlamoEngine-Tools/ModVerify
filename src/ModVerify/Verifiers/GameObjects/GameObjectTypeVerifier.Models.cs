using PG.StarWarsGame.Engine.GameObjects;
using System.Threading;

namespace AET.ModVerify.Verifiers.GameObjects;

public sealed partial class GameObjectTypeVerifier
{
    private void VerifyModels(GameObject gameObject, string[] context, CancellationToken token)
    {
        foreach (var model in GameEngine.GameObjectTypeManager.GetModels(gameObject))
            _singleModelVerifier.Verify(model, context, token);
    }
}