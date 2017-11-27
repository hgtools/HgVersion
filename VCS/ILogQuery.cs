namespace HgVersion.VCS
{
    /// <summary>
    /// Abstraction for a repository log query.
    /// </summary>
    public interface ILogQuery
    {
        /// <summary>
        /// Returns a <see cref="ILogQuery"/> that selects the first "n" commits of the set.
        /// </summary>
        /// <param name="amount">The number of commits to select.</param>
        /// <returns>A <see cref="ILogQuery"/> that selects the first N commits of result set.</returns>
        ILogQuery Limit(int amount);
    }
}