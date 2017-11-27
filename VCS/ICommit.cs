using System;
using System.Collections.Generic;

namespace HgVersion.VCS
{
    /// <summary>
    /// Abstraction for a commit.
    /// </summary>
    public interface ICommit
    {
        /// <summary>
        /// Gets the timestamp of this <see cref="ICommit"/>.
        /// </summary>
        DateTime Timestamp { get; }

        /// <summary>
        /// Gets the name of the author of this <see cref="ICommit"/>.
        /// </summary>
        string AuthorName { get; }

        /// <summary>
        /// Gets the email address of the author of this <see cref="ICommit"/>.
        /// </summary>
        string AuthorEmailAddress { get; }

        /// <summary>
        /// Gets the commit message of this <see cref="ICommit"/>.
        /// </summary>
        string CommitMessage { get; }

        /// <summary>
        /// Gets the branch this <see cref="ICommit"/> is on.
        /// </summary>
        string Branch { get; }
        
        /// <summary>
        /// Gets the unique hash of this <see cref="ICommit" />.
        /// </summary>
        string Hash { get; }

        /// <summary>
        /// Gets the collection of tags for this <see cref="ICommit"/>, or an empty collection if this commit has no tags.
        /// </summary>
        IEnumerable<string> Tags { get; }
    }
}
