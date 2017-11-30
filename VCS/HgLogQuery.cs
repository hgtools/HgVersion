using Mercurial;
using VCSVersion.VCS;

// ReSharper disable MemberCanBePrivate.Global

namespace HgVersion.VCS
{
    /// <inheritdoc />
    public sealed class HgLogQuery : ILogQuery
    {
        /// <summary>
        /// Specifies a set of revisions.
        /// </summary>
        public RevSpec Revision { get; }

        /// <summary>
        /// Create an instance of <see cref="HgLogQuery"/>
        /// </summary>
        /// <param name="revision">Specifies a set of revisions</param>
        public HgLogQuery(RevSpec revision)
        {
            Revision = revision;
        }

        /// <summary>
        /// Gets a <see cref="HgLogQuery"/> that selects the first "n" commits of the set.
        /// </summary>
        /// <param name="amount">The number of commits to select.</param>
        public HgLogQuery Limit(int amount)
        {
            return Revision.Limit(amount);
        }

        /// <summary>
        /// Gets a <see cref= "HgLogQuery" /> that selects all commits that is a parent
        /// commits of commits in this <see cref="HgLogQuery"/>.
        /// </summary>
        public HgLogQuery Parents()
        {
            return Revision.Parents;
        }

        /// <summary>
        /// Gets a <see cref="HgLogQuery"/> that selects commits witch does not belong to excluded branches.
        /// </summary>
        /// <param name="excludedBranches">Branches for exclusion from the result set.</param>
        /// <returns></returns>
        public HgLogQuery Except(params string[] excludedBranches)
        {
            var revision = new RevSpec(Revision);
            foreach (var excludedBranch in excludedBranches)
            {
                revision &= RevSpec.InBranch(excludedBranch);
            }

            return revision;
        }

        /// <summary>
        /// Converts a <see cref="RevSpec"/> into a <see cref="HgLogQuery"/> 
        /// </summary>
        /// <param name="revision">Specifies a set of revisions</param>
        public static implicit operator HgLogQuery(RevSpec revision) =>
            new HgLogQuery(revision);

        /// <inheritdoc />
        ILogQuery ILogQuery.Limit(int amount) =>
            Limit(amount);
    }
}