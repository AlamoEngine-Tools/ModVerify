using AnakinRaW.CommonUtilities.SimplePipeline.Progress;

namespace AET.ModVerify.Progress;

/// <summary>
/// Defines a progress reporter for the verification process.
/// </summary>
public interface IVerifyProgressReporter : IProgressReporter<VerifyProgressInfo>;