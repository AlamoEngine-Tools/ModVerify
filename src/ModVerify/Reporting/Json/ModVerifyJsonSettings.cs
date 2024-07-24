using System.Text.Json;

namespace AET.ModVerify.Reporting.Json;

internal static class ModVerifyJsonSettings
{
    public static readonly JsonSerializerOptions JsonSettings = new()
    {
        WriteIndented = true
    };
}