using HgVersion.SemanticVersions;

namespace HgVersion.VersionCalculation
{
    public interface IVersionCalculator
    {
        SemanticVersion CalculateVersion(IVersionContext context);
    }
}
