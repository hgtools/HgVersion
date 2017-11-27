using System.Collections.Generic;

namespace HgVersion.VersionCalculation.BaseVersionCalculation
{
    /// <summary>
    /// Calculates specific <see cref="BaseVersion"/> values.
    /// </summary>
    public interface IBaseVersionStrategy
    {
        /// <summary>
        /// Calculates specific <see cref="BaseVersion"/> values for the given <paramref name="context"/>.
        /// </summary>
        /// <param name="context">
        /// The context for calculating the <see cref="BaseVersion"/>.
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{BaseVersion}"/> of the base version values found by the strategy.
        /// </returns>
        IEnumerable<BaseVersion> GetVersions(IVersionContext context);
    }
}