using System;

namespace HgVersion.VCS
{
    /// <summary>
    /// Abstraction for a repository log query builder.
    /// </summary>
    public interface ILogQueryBuilder
    {
        /// <summary>
        /// Returns a <see cref="ILogQuery"/> that selects a commit based on its unique hash number.
        /// </summary>
        /// <param name="hash">The commit unique hash.</param>
        /// <returns>
        /// The revision specification for a revision selected by
        /// its locally unique revision number.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="hash"/> is <c>null</c> or empty.</para>
        /// </exception>
        ILogQuery Single(string hash);
        
        /// <summary>
        /// Create a <see cref="ILogQuery"/> that includes the commit
        /// specified and all ancestor commits.
        /// </summary>
        /// <param name="query">The <see cref="ILogQuery"/> to end with.</param>
        /// <returns>A <see cref="ILogQuery"/> with the specified range.</returns>
        ILogQuery AncestorsOf(ILogQuery query);
    }
}