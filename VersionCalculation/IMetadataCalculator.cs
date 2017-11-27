using HgVersion.SemanticVersions;
using HgVersion.VCS;

namespace HgVersion.VersionCalculation
{
    public interface IMetadataCalculator
    {
        BuildMetadata CalculateMetadata(IVersionContext context, ICommit baseVersionSource);
    }
}