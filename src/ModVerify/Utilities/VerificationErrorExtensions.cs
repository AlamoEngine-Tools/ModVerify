using System;
using System.Collections.Generic;
using AET.ModVerify.Reporting;
using AET.ModVerify.Reporting.Baseline;
using AET.ModVerify.Reporting.Suppressions;

namespace AET.ModVerify.Utilities;

/// <summary>
/// Provides extension methods for applying baselines and suppressions to collections of <see cref="VerificationError"/> instances.
/// </summary>
public static class VerificationErrorExtensions
{
    extension(IEnumerable<VerificationError> errors)
    {
        /// <summary>
        /// Applies the specified <see cref="VerificationBaseline"/> to the collection of <see cref="VerificationError"/> instances.
        /// </summary>
        /// <param name="baseline">The verification baseline to apply.</param>
        /// <returns>An enumerable collection of verification errors that are not present in the baseline.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="errors"/> or <paramref name="baseline"/> is <see langword="null"/>.</exception>
        public IEnumerable<VerificationError> ApplyBaseline(VerificationBaseline baseline)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));
            if (baseline == null)
                throw new ArgumentNullException(nameof(baseline));
            return baseline.Apply(errors);
        }

        /// <summary>
        /// Applies the specified <see cref="BaselineCollection"/> to the collection of <see cref="VerificationError"/> instances.
        /// </summary>
        /// <param name="baselines">The baseline collection to apply.</param>
        /// <returns>An enumerable collection of verification errors that are not present in the baselines.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="errors"/> or <paramref name="baselines"/> is <see langword="null"/>.</exception>
        public IEnumerable<VerificationError> ApplyBaselines(BaselineCollection baselines)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));
            if (baselines == null)
                throw new ArgumentNullException(nameof(baselines));
            return baselines.Apply(errors);
        }

        /// <summary>
        /// Applies the specified <see cref="SuppressionList"/> to the collection of <see cref="VerificationError"/> instances.
        /// </summary>
        /// <param name="suppressions">The suppression list to apply.</param>
        /// <returns>An enumerable collection of verification errors that are not suppressed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="errors"/> or <paramref name="suppressions"/> is <see langword="null"/>.</exception>
        public IEnumerable<VerificationError> ApplySuppressions(SuppressionList suppressions)
        {
            if (errors == null)
                throw new ArgumentNullException(nameof(errors));
            if (suppressions == null)
                throw new ArgumentNullException(nameof(suppressions));
            return suppressions.Apply(errors);
        }
    }
}