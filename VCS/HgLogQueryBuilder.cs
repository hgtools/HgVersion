using System;
using Mercurial;
using VCSVersion.VCS;

namespace HgVersion.VCS
{
    /// <inheritdoc />
    public sealed class HgLogQueryBuilder : ILogQueryBuilder
    {
        /// <summary>
        /// Returns a <see cref="HgLogQuery"/> that selects a commit based on its unique hash number.
        /// </summary>
        /// <param name="hash">The commit unique hash.</param>
        /// <exception cref="ArgumentNullException">
        /// <para><paramref name="hash"/> is <c>null</c> or empty.</para>
        /// </exception>
        public HgLogQuery Single(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                throw new ArgumentNullException(nameof(hash));

            return RevSpec.Single(hash);
        }

        /// <summary>
        /// Create a <see cref="HgLogQuery"/> that includes the commit
        /// specified and all ancestor commits.
        /// </summary>
        /// <param name="hash">The commit hash to end with.</param>
        public HgLogQuery AncestorsOf(string hash)
        {
            return AncestorsOf(Single(hash));
        }

        /// <summary>
        /// Create a <see cref="HgLogQuery"/> that includes the commit
        /// specified and all ancestor commits.
        /// </summary>
        /// <param name="query">The <see cref="HgLogQuery"/> to end with.</param>
        public HgLogQuery AncestorsOf(HgLogQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            
            if (!(query is HgLogQuery hgQuery))
                throw new InvalidOperationException($"{query.GetType()} is not supported.");

            return RevSpec.AncestorsOf(hgQuery.Revision);
        }

        /// <summary>
        /// Creates a <see cref="HgLogQuery"/> that finds tipmost commit that belongs to the named branch.
        /// </summary>
        /// <param name="name">Branch name.</param>
        public HgLogQuery ByBranch(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            return RevSpec.ByBranch(name);
        }

        /// <summary>
        /// Creates a <see cref="HgLogQuery"/> that finds the common ancestor of the two <see cref="HgLogQuery"/>
        /// </summary>
        /// <param name="query1">The first <see cref="HgLogQuery"/> of which to find the ancestor of.</param>
        /// <param name="query2">The second <see cref="HgLogQuery"/> of which to find the ancestor of.</param>
        public HgLogQuery CommonAncestorOf(HgLogQuery query1, HgLogQuery query2)
        {
            if (query1 == null)
                throw new ArgumentNullException(nameof(query1));

            if (query2 == null)
                throw new ArgumentNullException(nameof(query2));

            if (!(query1 is HgLogQuery hgQuery1))
                throw new InvalidOperationException($"{query1.GetType()} is not supported.");

            if (!(query2 is HgLogQuery hgQuery2))
                throw new InvalidOperationException($"{query2.GetType()} is not supported.");

            return RevSpec.CommonAncestorOf(
                hgQuery1.Revision,
                hgQuery2.Revision);
        }

        /// <summary>
        /// Creates a <see cref="HgLogQuery"/> that finds tagged commits that belongs to the named branch.
        /// </summary>
        /// <param name="name">Branch name.</param>
        public HgLogQuery TaggedBranchCommits(string name)
        {
            return RevSpec.ByBranch(name) & RevSpec.Tagged();
        }

        /// <inheritdoc />
        ILogQuery ILogQueryBuilder.AncestorsOf(string hash) =>
            AncestorsOf(hash);

        /// <inheritdoc />
        ILogQuery ILogQueryBuilder.Single(string hash) =>
            Single(hash);
    }
}