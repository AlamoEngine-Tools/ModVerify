using AET.ModVerify.Verifiers;
using Microsoft.Extensions.DependencyInjection;

namespace AET.ModVerify;

public static class ModVerifyServiceExtensions
{
    public static IServiceCollection RegisterVerifierCache(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<IAlreadyVerifiedCache>(sp => new AlreadyVerifiedCache(sp));
    }
}