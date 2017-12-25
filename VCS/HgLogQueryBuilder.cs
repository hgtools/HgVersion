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
            
            return RevSpec.AncestorsOf(query.Revision);
        }

        /// <summary>
        /// Creates a <see cref="HgLogQuery"/> that finds commits that belong to the named branch.
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
            
            return RevSpec.CommonAncestorOf(
                query1.Revision,
                query2.Revision);
        }

        /// <summary>
        /// Create a <see cref="HgLogQuery" /> that includes a range
        /// of commits between <paramref name="fromHash"/> and <paramref name="toHash"/>.
        /// </summary>
        /// <param name="fromHash">Hash of first commit to include.</param>
        /// <param name="toHash">Hash of last commit to include.</param>
        public HgLogQuery Range(string fromHash, string toHash)
        {
            return RevSpec.Range(
                RevSpec.Single(fromHash), 
                RevSpec.Single(toHash));
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
            AncestorsOf(hash).ExceptTaggingCommits();

        /// <inheritdoc />
        ILogQuery ILogQueryBuilder.Single(string hash) =>
            Single(hash).ExceptTaggingCommits();

        /// <inheritdoc />
        ILogQuery ILogQueryBuilder.ByBranch(string name) =>
            ByBranch(name).ExceptTaggingCommits();
        
        /// <inheritdoc />
        ILogQuery ILogQueryBuilder.Range(string fromHash, string toHash) =>
            Range(fromHash, toHash).ExceptTaggingCommits();
        
    }
}