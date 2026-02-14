using AET.ModVerify.Verifiers;
using Microsoft.Extensions.DependencyInjection;

namespace AET.ModVerify;

public static class ModVerifyServiceExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddModVerify()
        {
            return serviceCollection.AddSingleton<IGameVerifierService>(sp => new GameVerifierService(sp));
        }

        public IServiceCollection RegisterVerifierCache()
        {
            return serviceCollection.AddSingleton<IAlreadyVerifiedCache>(sp => new AlreadyVerifiedCache(sp));
        }
    }
}