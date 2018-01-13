using System;
using System.Collections.Generic;
using Mercurial;
using VCSVersion.VCS;

namespace HgVersion.VCS
{
    /// <inheritdoc cref="ICommit" />
    public sealed class HgCommit : ICommit, IEquatable<HgCommit>
    {   
        private readonly Changeset _changeset;

        /// <inheritdoc />
        public DateTime When => _changeset.Timestamp;

        /// <inheritdoc />
        public string AuthorName => _changeset.AuthorName;

        /// <inheritdoc />
        public string AuthorEmailAddress => _changeset.AuthorEmailAddress;

        /// <inheritdoc />
        public string CommitMessage => _changeset.CommitMessage;

        /// <inheritdoc />
        public string Branch => _changeset.Branch;
        
        /// <inheritdoc />
        public string Hash => _changeset.Hash;

        /// <inheritdoc />
        public IEnumerable<string> Tags => _changeset.Tags;

        /// <summary>
        /// Creates an instance of <see cref="HgCommit"/>
        /// </summary>
        /// <param name="changeset">Mercurial.Net <see cref="Changeset"/></param>
        public HgCommit(Changeset changeset)
        {
            _changeset = changeset;
        }
        
        /// <summary>
        /// Covert <see cref="Changeset"/> into <see cref="HgCommit"/>
        /// </summary>
        /// <param name="changeset">Changeset from Mercurial.Net to convert</param>
        public static implicit operator HgCommit(Changeset changeset) =>
            new HgCommit(changeset);

        /// <summary>
        /// Covert <see cref="HgCommit"/> into <see cref="Changeset"/>
        /// </summary>
        /// <param name="commit">Commit to convert</param>
        public static implicit operator Changeset(HgCommit commit) =>
            commit._changeset;

        /// <summary>
        /// Covert <see cref="HgCommit"/> into <see cref="RevSpec"/>
        /// </summary>
        /// <param name="commit">Commit to convert</param>
        public static implicit operator RevSpec(HgCommit commit) =>
            RevSpec.Single(commit.Hash);

        /// <inheritdoc />
        public bool Equals(HgCommit other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(_changeset, other._changeset);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is HgCommit && Equals((HgCommit) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _changeset?.GetHashCode() ?? 0;
        }
        
        public static bool operator ==(HgCommit left, HgCommit right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(HgCommit left, HgCommit right)
        {
            return !Equals(left, right);
        }
    }
}
