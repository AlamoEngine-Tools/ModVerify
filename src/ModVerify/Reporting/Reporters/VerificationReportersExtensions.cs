using AET.ModVerify.Reporting.Reporters.JSON;
using AET.ModVerify.Reporting.Reporters.Text;
using AET.ModVerify.Reporting.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace AET.ModVerify.Reporting.Reporters;

public static class VerificationReportersExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection RegisterJsonReporter()
        {
            return serviceCollection.RegisterJsonReporter(new JsonReporterSettings
            {
                OutputDirectory = "."
            });
        }

        public IServiceCollection RegisterTextFileReporter()
        {
            return serviceCollection.RegisterTextFileReporter(new TextFileReporterSettings
            {
                OutputDirectory = "."
            });
        }

        public IServiceCollection RegisterConsoleReporter(bool summaryOnly = false)
        {
            return serviceCollection.RegisterConsoleReporter(new ReporterSettings
            {
                MinimumReportSeverity = VerificationSeverity.Error
            }, summaryOnly);
        }

        public IServiceCollection RegisterJsonReporter(JsonReporterSettings settings)
        {
            return serviceCollection.AddSingleton<IVerificationReporter>(sp => new JsonReporter(settings, sp));
        }

        public IServiceCollection RegisterTextFileReporter(TextFileReporterSettings settings)
        {
            return serviceCollection.AddSingleton<IVerificationReporter>(sp => new TextFileReporter(settings, sp));
        }

        public IServiceCollection RegisterConsoleReporter(ReporterSettings settings,
            bool summaryOnly = false)
        {
            return serviceCollection.AddSingleton<IVerificationReporter>(sp => new ConsoleReporter(settings, summaryOnly, sp));
        }
    }
}