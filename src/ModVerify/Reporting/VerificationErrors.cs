using System;
using System.Collections.Generic;

namespace AET.ModVerify.Reporting;

public sealed record VerificationErrors
{
    public static readonly VerificationErrors Empty = new(
        [],
        new Dictionary<string, IReadOnlyList<VerificationError>>(),
        new Dictionary<string, IReadOnlyList<VerificationError>>());

    public IReadOnlyList<VerificationError> NewErrors { get; }

    public IReadOnlyDictionary<string, IReadOnlyList<VerificationError>> ExistingErrors { get; }

    public IReadOnlyDictionary<string, IReadOnlyList<VerificationError>> ResolvedErrors { get; }

    public VerificationErrors(
        IReadOnlyList<VerificationError> newErrors,
        IReadOnlyDictionary<string, IReadOnlyList<VerificationError>> existingErrors,
        IReadOnlyDictionary<string, IReadOnlyList<VerificationError>> resolvedErrors)
    {
        NewErrors = newErrors ?? throw new ArgumentNullException(nameof(newErrors));
        ExistingErrors = existingErrors ?? throw new ArgumentNullException(nameof(existingErrors));
        ResolvedErrors = resolvedErrors ?? throw new ArgumentNullException(nameof(resolvedErrors));
    }
}
