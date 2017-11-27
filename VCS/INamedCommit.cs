namespace HgVersion.VCS
{
    /// <summary>
    /// Abstraction for named commits, like tags, heads and bookmarks.
    /// </summary>
    public interface INamedCommit
    {
        /// <summary>
        /// Gets the name of the commit.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the commit itself.
        /// </summary>
        ICommit Commit { get; }
    }
}