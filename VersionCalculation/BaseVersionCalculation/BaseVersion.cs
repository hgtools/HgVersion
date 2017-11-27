using HgVersion.SemanticVersions;
using HgVersion.VCS;

namespace HgVersion.VersionCalculation.BaseVersionCalculation
 {
    /// <summary>
    /// TODO: add summary
    /// </summary>
    public sealed class BaseVersion
    {
        /// <summary>
        /// Base version type
        /// </summary>
        public string Type { get; }
        
        /// <summary>
        /// Current semantic version
        /// </summary>
        public SemanticVersion Version { get; }
        
        /// <summary>
        /// Base version source
        /// </summary>
        public ICommit Source { get; }
        
        /// <summary>
        /// Should increment current version
        /// </summary>
        public bool ShouldIncrement { get; }
        
        /// <summary>
        /// Create an instance of <see cref="BaseVersion"/>
        /// </summary>
        public BaseVersion(string type, SemanticVersion version, ICommit source, bool shouldIncrement)
        {
            Type = type;
            Version = version;
            Source = source;
            ShouldIncrement = shouldIncrement;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Type}: {Version.ToString("f")} with commit count "
                + $"source {(Source == null ? "External Source" : Source.Hash)} "
                + $"({(ShouldIncrement ? "Should increment" : "None")})";
        }
    }
}
