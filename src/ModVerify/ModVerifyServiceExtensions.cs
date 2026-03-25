using AET.ModVerify.Verifiers.Caching;
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
            return serviceCollection.AddSingleton<IAlreadyVerifiedCache>(new AlreadyVerifiedCache());
        }
    }
}