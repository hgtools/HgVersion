using System;
using System.Threading;
using VCSVersion.VCS;

namespace HgVersion.VCS
{
    /// <inheritdoc />
    public abstract class HgNamedCommit : INamedCommit
    {
        private string _name;
        private ICommit _commit;
        private Func<ICommit> _commitFactory;

        /// <inheritdoc />
        public string Name => _name;

        /// <inheritdoc />
        public ICommit Commit => LazyInitializer.EnsureInitialized(ref _commit, _commitFactory);
        
        /// <summary>
        /// Creates an instance of <see cref="HgNamedCommit"/>
        /// </summary>
        /// <param name="name">Commit name</param>
        /// <param name="commit">Commit itself</param>
        protected HgNamedCommit(string name, ICommit commit)
        {
            _name = name;
            _commit = commit;
            _commitFactory = () => _commit;
        }
        
        /// <summary>
        /// Creates an instance of <see cref="HgNamedCommit"/>
        /// </summary>
        /// <param name="name">Commit name</param>
        /// <param name="commitLookup">Commit lookup function</param>
        protected HgNamedCommit(string name, Func<ICommit> commitLookup)
        {
            _name = name;
            _commitFactory = commitLookup;
        } 
    }
}