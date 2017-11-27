using HgVersion.SemanticVersions;

namespace HgVersion.VersionCalculation
{
    public interface IIncrementStrategy
    {
        SemanticVersion IncrementVersion(SemanticVersion semver);
    }
}