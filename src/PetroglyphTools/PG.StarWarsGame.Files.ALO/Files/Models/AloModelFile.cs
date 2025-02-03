using System;
using PG.StarWarsGame.Files.ALO.Data;

namespace PG.StarWarsGame.Files.ALO.Files.Models;

public sealed class AloModelFile(
    AlamoModel model,
    AloFileInformation fileInformation,
    IServiceProvider serviceProvider)
    : PetroglyphFileHolder<AlamoModel, AloFileInformation>(model, fileInformation, serviceProvider), IAloModelFile;