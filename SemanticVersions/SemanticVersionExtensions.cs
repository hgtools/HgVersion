using HgVersion.Output;

namespace HgVersion.SemanticVersions
{
    /// <summary>
    /// <see cref="SemanticVersion"/> extension methods
    /// </summary>
    public static class SemanticVersionExtensions
    {
        /// <summary>
        /// Converts a <see cref="SemanticVersion"/> into <see cref="VersionVariables"/>
        /// </summary>
        /// <param name="semVersion">Semantic version</param>
        /// <returns></returns>
        public static VersionVariables ToVersionVariables(this SemanticVersion semVersion)
        {
            var builder = new VersionVariablesBuilder(semVersion);
            return builder.Build();
        }
    }
}