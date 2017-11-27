namespace HgVersion.SemanticVersions
{
    /// <summary>
    /// <see cref="SemanticVersion"/> comparation mode
    /// </summary>
    public enum SemanticVersionComparation
    {
        /// <summary>
        /// Full comparation
        /// </summary>
        Full,
        
        /// <summary>
        /// Compare only major, minor and patch fields 
        /// </summary>
        MajorMinorPatch
    }
}