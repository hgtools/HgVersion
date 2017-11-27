using HgVersion.SemanticVersions;

namespace HgVersion.VersionCalculation.BaseVersionCalculation
{
    /// <summary>
    /// <see cref="BaseVersion"/> extension methods.
    /// </summary>
    public static class BaseVersionExtensions
    {
        /// <summary>
        /// Attempt to increment <see cref="BaseVersion"/> version.
        /// </summary>
        /// <param name="version">The base version found by <see cref="IBaseVersionCalculator"/>.</param>
        /// <param name="context">The context for calculating the <see cref="BaseVersion"/>.</param>
        /// <returns></returns>
        public static SemanticVersion MaybeIncrement(this BaseVersion version, IVersionContext context)
        {
            var strategy = IncrementStrategyFactory.GetStrategy(context, version);
            var semVersion = version.Version;
            return strategy.IncrementVersion(semVersion);
        }
    }
}