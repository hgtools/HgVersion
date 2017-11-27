namespace HgVersion.Output
{
    public sealed class VersionVariables
    {
        public string Major { get; internal set; }
        public string Minor { get; internal set; }
        public string Patch { get; internal set; }
        public string PreReleaseTag { get; internal set; }
        public string PreReleaseTagWithDash { get; internal set; }
        public string PreReleaseLabel { get; internal set; }
        public string PreReleaseNumber { get; internal set; }
        public string BuildMetadata { get; internal set; }
        public string BuildMetadataPadded { get; internal set; }
        public string FullBuildMetadata { get; internal set; }
        public string MajorMinorPatch { get; internal set; }
        public string SemVer { get; internal set; }
        public string AssemblySemVer { get; internal set; }
        public string AssemblyFileSemVer { get; internal set; }
        public string FullSemVer { get; internal set; }
        public string InformationalVersion { get; internal set; }
        public string BranchName { get; internal set; }
        public string Sha { get; internal set; }
        public string CommitDate { get; internal set; }
        public string NuGetVersion { get; internal set; }
        public string NuGetPreReleaseTag { get; internal set; }
        public string CommitsSinceVersionSource { get; internal set; }
    }
}