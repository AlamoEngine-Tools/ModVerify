using System;

namespace AET.ModVerify.Reporting.Reporters;

public static class ExtensionMethods
{
    extension(IVerificationReporter)
    {
        public static IVerificationReporter CreateJson(IServiceProvider serviceProvider)
        {
            return IVerificationReporter.CreateJson(new JsonReporterSettings(), serviceProvider);
        }

        public static IVerificationReporter CreateJson(JsonReporterSettings settings, IServiceProvider serviceProvider)
        {
            return new JsonReporter(settings, serviceProvider);
        }

        public static IVerificationReporter CreateText(IServiceProvider serviceProvider)
        {
            return IVerificationReporter.CreateText(new TextFileReporterSettings(), serviceProvider);
        }

        public static IVerificationReporter CreateText(TextFileReporterSettings settings, IServiceProvider serviceProvider)
        {
            return new TextFileReporter(settings, serviceProvider);
        }

        public static IVerificationReporter CreateConsole(IServiceProvider serviceProvider, bool summaryOnly = false)
        {
            var settings = new ConsoleReporterSettings
            {
                MinimumReportSeverity = VerificationSeverity.Error,
                SummaryOnly = summaryOnly
            };
            return IVerificationReporter.CreateConsole(settings, serviceProvider);
        }

        public static IVerificationReporter CreateConsole(ConsoleReporterSettings settings, IServiceProvider serviceProvider)
        {
            return new ConsoleReporter(settings, serviceProvider);
        }
    }
}