using System;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Files.ALO.Services;

namespace PG.StarWarsGame.Engine.Rendering;

internal class PGRender(GameRepository gameRepository, GameErrorReporterWrapper errorReporter, IServiceProvider serviceProvider)
{
    private readonly IAloFileService _aloFileService = serviceProvider.GetRequiredService<IAloFileService>();
    private readonly IRepository _modelRepository = gameRepository.ModelRepository;

    public ModelClass? LoadModelAndAnimations(string path, bool metadataOnly = true)
    {
        if (string.IsNullOrEmpty(path)) 
            errorReporter.Assert(EngineAssert.FromNullOrEmpty(null, "Model path is null or empty."));

        using var aloStream = _modelRepository.TryOpenFile(path);

        return null;
    }
}