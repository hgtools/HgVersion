using Mercurial;
using VCSVersion.VCS;

namespace HgVersion.VCS
{
    /// <summary>
    /// Mercurial repository
    /// </summary>
    public interface IHgRepository : IRepository
    {
        /// <summary>
        /// Get a commit by revision number 
        /// </summary>
        /// <param name="revisionNumber">Revision number</param>
        ICommit GetCommit(int revisionNumber);

        /// <summary>
        /// Get a commit by revision specification
        /// </summary>
        /// <param name="revision">Revision specification</param>
        ICommit GetCommit(RevSpec revision);

        /// <summary>
        /// Get a branch head
        /// </summary>
        /// <param name="branchName">Branch name</param>
        ICommit GetBranchHead(string branchName);
        
        /// <summary>
        /// Create a branch with name <paramref name="branch"/>
        /// </summary>
        /// <param name="branch">Branch name</param>
        void Branch(string branch);

        /// <summary>
        /// Update working directory to a revision
        /// </summary>
        /// <param name="rev">Revision to update</param>
        void Update(RevSpec rev);
    }
}