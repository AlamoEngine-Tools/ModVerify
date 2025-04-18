﻿using AET.ModVerify.Reporting.Reporters.JSON;
using AET.ModVerify.Reporting.Reporters.Text;
using AET.ModVerify.Reporting.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace AET.ModVerify.Reporting.Reporters;

public static class VerificationReportersExtensions
{
    public static IServiceCollection RegisterJsonReporter(this IServiceCollection serviceCollection)
    {
        return RegisterJsonReporter(serviceCollection, new JsonReporterSettings
        {
            OutputDirectory = "."
        });
    }

    public static IServiceCollection RegisterTextFileReporter(this IServiceCollection serviceCollection)
    {
        return RegisterTextFileReporter(serviceCollection, new TextFileReporterSettings
        {
            OutputDirectory = "."
        });
    }

    public static IServiceCollection RegisterConsoleReporter(this IServiceCollection serviceCollection, bool summaryOnly = false)
    {
        return RegisterConsoleReporter(serviceCollection, new VerifyReportSettings
        {
            MinimumReportSeverity = VerificationSeverity.Error
        }, summaryOnly);
    }

    public static IServiceCollection RegisterJsonReporter(
        this IServiceCollection serviceCollection, 
        JsonReporterSettings settings)
    {
        return serviceCollection.AddSingleton<IVerificationReporter>(sp => new JsonReporter(settings, sp));
    }

    public static IServiceCollection RegisterTextFileReporter(
        this IServiceCollection serviceCollection, 
        TextFileReporterSettings settings)
    {
        return serviceCollection.AddSingleton<IVerificationReporter>(sp => new TextFileReporter(settings, sp));
    }

    public static IServiceCollection RegisterConsoleReporter(
        this IServiceCollection serviceCollection, 
        VerifyReportSettings settings,
        bool summaryOnly = false)
    {
        return serviceCollection.AddSingleton<IVerificationReporter>(sp => new ConsoleReporter(settings, summaryOnly, sp));
    }
}