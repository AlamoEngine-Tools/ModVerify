using Microsoft.Extensions.DependencyInjection;

namespace AET.ModVerify;

public static class ModVerifyServiceContribution
{
    public static void ContributeServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IVerificationProvider>(sp => new VerificationProvider(sp));
    }
}