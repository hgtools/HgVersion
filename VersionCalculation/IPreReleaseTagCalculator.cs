using HgVersion.SemanticVersions;

namespace HgVersion.VersionCalculation
{
    public interface IPreReleaseTagCalculator
    {
        PreReleaseTag CalculateTag(IVersionContext context, SemanticVersion semVersion);
    }
}