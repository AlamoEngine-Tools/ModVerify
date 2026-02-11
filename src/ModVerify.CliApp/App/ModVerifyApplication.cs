using AET.ModVerify.App.Settings;
using AnakinRaW.ApplicationBase;
using AnakinRaW.ApplicationBase.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace AET.ModVerify.App;

internal sealed class ModVerifyApplication(AppSettingsBase settings, IServiceProvider services)
{
    private readonly ILogger? _logger = services.GetService<ILoggerFactory>()?.CreateLogger(typeof(ModVerifyApplication));

    public async Task<int> RunAsync()
    {
        using (new UnhandledExceptionHandler(services))
        using (new UnobservedTaskExceptionHandler(services))
            return await RunCoreAsync().ConfigureAwait(false);
    }

    private async Task<int> RunCoreAsync()
    {
        _logger?.LogDebug("Raw command line: {CommandLine}", Environment.CommandLine);
        
        try
        {
            var action = CreateAppAction();
            return await action.ExecuteAsync().ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger?.LogCritical(e, e.Message);
            ConsoleUtilities.WriteApplicationFatalError(ModVerifyConstants.AppNameString, e);
            return e.HResult;
        }
        finally
        {
#if NET
            await Log.CloseAndFlushAsync();
#else
            Log.CloseAndFlush();
#endif
            if (settings is AppVerifySettings { IsInteractive: true })
            {
                Console.WriteLine();
                ConsoleUtilities.WriteHorizontalLine('-');
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
            }
        }
    }

    private IModVerifyAppAction CreateAppAction()
    {
        switch (settings)
        {
            case AppVerifySettings verifySettings:
                return new VerifyAction(verifySettings, services);
            case AppBaselineSettings baselineSettings:
                return new CreateBaselineAction(baselineSettings, services);
            default:
                throw new InvalidOperationException("Unknown settings");
        }
    }
}