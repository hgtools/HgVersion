using HgVersion.SemanticVersions;
using HgVersion.VCS;

namespace HgVersion.VersionCalculation
{
    public sealed class MetadataCalculator : IMetadataCalculator
    {
        public BuildMetadata CalculateMetadata(IVersionContext context, ICommit baseVersionSource)
        {
//            var qf = new CommitFilter
//            {
//                IncludeReachableFrom = context.CurrentCommit,
//                ExcludeReachableFrom = baseVersionSource,
//                SortBy = CommitSortStrategies.Topological | CommitSortStrategies.Time
//            };
//
//            var commitLog = context.Repository.Commits.QueryBy(qf);
//            var commitsSinceTag = commitLog.Count();

            var repository = context.Repository;
            
            return new BuildMetadata(
                1, // TODO: implement commitsSinceTag
                repository.Branch(),
                repository.Tip().Hash,
                repository.Tip().Timestamp);
        }
    }
}