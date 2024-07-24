using System.Collections.Generic;
using AET.ModVerify.Settings;
using AET.ModVerify.Verifiers;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerify;

public interface IVerificationProvider
{
    IEnumerable<GameVerifierBase> GetAllDefaultVerifiers(IGameDatabase database, GameVerifySettings settings);
}