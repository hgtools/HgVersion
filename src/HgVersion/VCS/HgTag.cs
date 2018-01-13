using System;
using VCSVersion.VCS;

namespace HgVersion.VCS
{
    /// <inheritdoc cref="HgNamedCommit" />
    public sealed class HgTag : HgNamedCommit, ITag
    {
        /// <summary>
        /// Creates an instance of <see cref="HgTag"/>.
        /// </summary>
        /// <param name="name">Commit name</param>
        /// <param name="commit">Commit itself</param>
        public HgTag(string name, ICommit commit) : base(name, commit)
        { }
        
        /// <summary>
        /// Creates an instance of <see cref="HgTag"/>
        /// </summary>
        /// <param name="name">Commit name</param>
        /// <param name="commitLookup">Commit lookup function</param>
        public HgTag(string name, Func<ICommit> commitLookup) : base(name, commitLookup)
        { }
    }
}