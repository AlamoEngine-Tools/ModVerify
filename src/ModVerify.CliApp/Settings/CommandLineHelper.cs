using System;
using System.Linq;
using System.Reflection;
using CommandLine;

namespace AET.ModVerify.App.Settings;

internal static class CommandLineHelper
{
    public static string GetOptionName(this Type type, string optionPropertyName)
    {
        var property = type.GetProperties().FirstOrDefault(p => p.Name.Equals(optionPropertyName));
        var optionAttribute = property?.GetCustomAttribute<OptionAttribute>();
        return optionAttribute is null
            ? throw new InvalidOperationException($"Unable to get option data for {type}:{optionAttribute}")
            : $"--{optionAttribute.LongName}";
    }
}