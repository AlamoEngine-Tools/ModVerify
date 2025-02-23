using System;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Engine.ErrorReporting;
using PG.StarWarsGame.Engine.IO.Repositories;
using PG.StarWarsGame.Files.ALO.Services;

namespace PG.StarWarsGame.Engine.Rendering;

internal class PGRender(GameRepository gameRepository, GameErrorReporterWrapper errorReporter, IServiceProvider serviceProvider)
{
    private readonly IAloFileService _aloFileService = serviceProvider.GetRequiredService<IAloFileService>();

    public ModelClass? LoadModelAndAnimations(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            errorReporter.Assert(EngineAssert.CreateCapture("Model path is null or empty."));
        }

        return new ModelClass(null!);
    }
}