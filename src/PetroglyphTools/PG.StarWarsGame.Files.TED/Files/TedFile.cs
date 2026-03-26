using System;
using PG.StarWarsGame.Files.TED.Data;

namespace PG.StarWarsGame.Files.TED.Files;

public sealed class TedFile(IMapData data, TedFileInformation fileInformation, IServiceProvider serviceProvider) 
    : PetroglyphFileHolder<IMapData, TedFileInformation>(data, fileInformation, serviceProvider), ITedFile;