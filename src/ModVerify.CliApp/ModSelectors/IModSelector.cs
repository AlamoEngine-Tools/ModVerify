using ModVerify.CliApp.Options;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure;

namespace ModVerify.CliApp.ModSelectors;

internal interface IModSelector
{
    GameLocations? Select(GameInstallationsSettings settings, out IPhysicalPlayableObject? targetObject, out GameEngineType? actualEngineType);
}