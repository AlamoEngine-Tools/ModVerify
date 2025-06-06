using AET.ModVerify.App.Settings;
using PG.StarWarsGame.Engine;
using PG.StarWarsGame.Infrastructure;

namespace AET.ModVerify.App.ModSelectors;

internal interface IModSelector
{
    GameLocations? Select(
        GameInstallationsSettings settings,
        out IPhysicalPlayableObject? targetObject, 
        out GameEngineType? actualEngineType);
}