using System.Collections.Generic;
using AET.ModVerify.Steps;
using PG.StarWarsGame.Engine.Database;

namespace AET.ModVerify;

public interface IVerificationProvider
{
    IEnumerable<GameVerificationStep> GetAllDefaultVerifiers(IGameDatabase database, VerificationSettings settings);
}