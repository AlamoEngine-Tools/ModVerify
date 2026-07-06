using System;

namespace AET.ModVerify.Reporting.Reporters;

/// <summary>Provides factory methods for creating verification reporters.</summary>
public static class ExtensionMethods
{
    extension(IVerificationReporter)
    {
        /// <summary>Creates a JSON reporter with default settings.</summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        /// <returns>A new JSON reporter.</returns>
        public static IVerificationReporter CreateJson(IServiceProvider serviceProvider)
        {
            return IVerificationReporter.CreateJson(new JsonReporterSettings(), serviceProvider);
        }

        /// <summary>Creates a JSON reporter with the specified settings.</summary>
        /// <param name="settings">The settings that control the reporter's behavior.</param>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        /// <returns>A new JSON reporter.</returns>
        public static IVerificationReporter CreateJson(JsonReporterSettings settings, IServiceProvider serviceProvider)
        {
            return new JsonReporter(settings, serviceProvider);
        }

        /// <summary>Creates a text-file reporter with default settings.</summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        /// <returns>A new text-file reporter.</returns>
        public static IVerificationReporter CreateText(IServiceProvider serviceProvider)
        {
            return IVerificationReporter.CreateText(new TextFileReporterSettings(), serviceProvider);
        }

        /// <summary>Creates a text-file reporter with the specified settings.</summary>
        /// <param name="settings">The settings that control the reporter's behavior.</param>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        /// <returns>A new text-file reporter.</returns>
        public static IVerificationReporter CreateText(TextFileReporterSettings settings, IServiceProvider serviceProvider)
        {
            return new TextFileReporter(settings, serviceProvider);
        }

        /// <summary>Creates a console reporter that reports findings with a severity of <see cref="VerificationSeverity.Error"/> or higher.</summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        /// <param name="summaryOnly"><see langword="true"/> to write only a summary; otherwise, <see langword="false"/> to write individual findings.</param>
        /// <returns>A new console reporter.</returns>
        public static IVerificationReporter CreateConsole(IServiceProvider serviceProvider, bool summaryOnly = false)
        {
            var settings = new ConsoleReporterSettings
            {
                MinimumReportSeverity = VerificationSeverity.Error,
                SummaryOnly = summaryOnly
            };
            return IVerificationReporter.CreateConsole(settings, serviceProvider);
        }

        /// <summary>Creates a console reporter with the specified settings.</summary>
        /// <param name="settings">The settings that control the reporter's behavior.</param>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        /// <returns>A new console reporter.</returns>
        public static IVerificationReporter CreateConsole(ConsoleReporterSettings settings, IServiceProvider serviceProvider)
        {
            return new ConsoleReporter(settings, serviceProvider);
        }
    }
}