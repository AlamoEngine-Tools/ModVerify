using System.Threading;
using System.Threading.Tasks;
using AET.ModVerify.Progress;
using AET.ModVerify.Reporting;
using AET.ModVerify.Settings;
using PG.StarWarsGame.Engine;

namespace AET.ModVerify;

public interface IGameVerifierService
{
    Task<VerificationResult> VerifyAsync(
        VerificationTarget verificationTarget,
        VerifierServiceSettings settings,
        VerificationBaseline baseline,
        SuppressionList suppressions,
        IVerifyProgressReporter? progressReporter,
        IGameEngineInitializationReporter? engineInitializationReporter,
        CancellationToken token = default);
}