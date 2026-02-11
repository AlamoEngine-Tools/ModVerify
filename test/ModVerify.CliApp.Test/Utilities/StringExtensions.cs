using System;

namespace ModVerify.CliApp.Test.Utilities;

internal static class StringExtensions
{
#if NETFRAMEWORK
    public static string[] Split(this string str, char separator, StringSplitOptions options)
    {
        return str.Split([separator], options);
    }
#endif
}