namespace HgVersion.VersionCalculation.BaseVersionCalculation
{
    /// <summary>
    /// Calculates the <see cref="BaseVersion"/> value.
    /// </summary>
    public interface IBaseVersionCalculator
    {
        /// <summary>
        /// Calculates the <see cref="BaseVersion"/> value for the given <paramref name="context"/>.
        /// </summary>
        /// <param name="context">
        /// The context for calculating the <see cref="BaseVersion"/>.
        /// </param>
        /// <returns>
        /// An <see cref="BaseVersion"/> of the base version value found by the strategy.
        /// </returns>
        BaseVersion CalculateVersion(IVersionContext context);
    }
}
