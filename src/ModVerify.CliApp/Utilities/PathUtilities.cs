using System;
using System.Runtime.InteropServices;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;

namespace AET.ModVerify.App.Utilities;

internal static class PathUtilities
{
    private static readonly string HomeVariable;
    private static readonly string HomePath;
    private static readonly StringComparison StringComparer;
    
    static PathUtilities()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            HomeVariable = "%USERPROFILE%";
            StringComparer = StringComparison.OrdinalIgnoreCase;
        }
        else
        {
            HomeVariable = "$HOME";
            StringComparer = StringComparison.Ordinal;
        }
        
        HomePath = PathNormalizer.Normalize(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            PathNormalizeOptions.EnsureTrailingSeparator);
    }

    internal static string MaskUsername(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        var index = path.IndexOf(HomePath, StringComparer);
        return index >= 0 ? path.Remove(index, HomePath.Length).Insert(index, HomeVariable) : path;
    }
}