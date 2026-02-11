using System.IO;

namespace AET.ModVerify.App.TargetSelectors;

internal class TargetNotFoundException(string path) 
    : DirectoryNotFoundException($"The target path '{path}' does not exist");