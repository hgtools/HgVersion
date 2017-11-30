using System;
using VCSVersion.VCS;

namespace HgVersion.VCS
{
    /// <inheritdoc cref="HgNamedCommit" />
    public sealed class HgBranchHead : HgNamedCommit, IBranchHead, IEquatable<HgBranchHead>
    {
        /// <summary>
        /// Creates an instance of <see cref="HgBranchHead"/>.
        /// </summary>
        /// <param name="name">Commit name</param>
        /// <param name="commit">Commit itself</param>
        public HgBranchHead(string name, ICommit commit) : base(name, commit)
        { }
        
        /// <summary>
        /// Creates an instance of <see cref="HgBranchHead"/>
        /// </summary>
        /// <param name="name">Commit name</param>
        /// <param name="commitLookup">Commit lookup function</param>
        public HgBranchHead(string name, Func<ICommit> commitLookup) : base(name, commitLookup)
        { }

        public bool Equals(HgBranchHead other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && Equals(Commit, other.Commit);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is HgBranchHead && Equals((HgBranchHead) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 0;
                hash ^= (Name?.GetHashCode() ?? 0) * 397;
                hash ^= (Commit?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public static bool operator ==(HgBranchHead left, HgBranchHead right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(HgBranchHead left, HgBranchHead right)
        {
            return !Equals(left, right);
        }
    }
}