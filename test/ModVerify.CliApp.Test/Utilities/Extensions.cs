using System;

namespace ModVerify.CliApp.Test.Utilities;

internal static class Extensions
{
#if NETFRAMEWORK
    public static string[] Split(this string str, char separator, StringSplitOptions options)
    {
        return str.Split([separator], options);
    }


    extension(Enum)
    {
        public static T Parse<T>(string value) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), value);
        }
    }
    
#endif
}