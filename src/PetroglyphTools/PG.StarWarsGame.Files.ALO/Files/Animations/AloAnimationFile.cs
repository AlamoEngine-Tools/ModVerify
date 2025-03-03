using System;
using PG.StarWarsGame.Files.ALO.Data;

namespace PG.StarWarsGame.Files.ALO.Files.Animations;

public sealed class AloAnimationFile(
    AlamoAnimation animation,
    AloFileInformation fileInformation,
    IServiceProvider serviceProvider)
    : PetroglyphFileHolder<AlamoAnimation, AloFileInformation>(animation, fileInformation, serviceProvider), IAloAnimationFile;