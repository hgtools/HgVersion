using System.Collections.Generic;
using System.Linq;
using HgVersion.Exceptions;
using HgVersion.SemanticVersions;
using HgVersion.VCS;

namespace HgVersion.VersionCalculation.BaseVersionCalculation
{
    /// <inheritdoc />
    public sealed class BaseVersionCalculator : IBaseVersionCalculator
    {
        private readonly IBaseVersionStrategy[] _strategies;

        /// <summary>
        /// Create an instance of <see cref="BaseVersionCalculator"/>.
        /// </summary>
        /// <param name="strategies">Strategies to find specific <see cref="BaseVersion"/> values</param>
        public BaseVersionCalculator(params IBaseVersionStrategy[] strategies)
        {
            _strategies = strategies;
        }

        /// <inheritdoc />
        public BaseVersion CalculateVersion(IVersionContext context)
        {
            var baseVersions = GetBaseVersionsByStrategies(context);
            var resultVersion = HandleBaseVersions(baseVersions);
            
            if (resultVersion.Source == null)
                throw new BaseVerisonException("Base version should not be null");

            return resultVersion;
        }
        
        private List<BaseVersionTuple> GetBaseVersionsByStrategies(IVersionContext context)
        {
            return _strategies
                .SelectMany(s => s.GetVersions(context))
                .Select(v => new BaseVersionTuple(v.MaybeIncrement(context), v))
                .ToList();
        }

        private BaseVersion HandleBaseVersions(List<BaseVersionTuple> baseVersions)
        {
            var maxVersion = baseVersions.Aggregate((v1, v2) => 
                v1.IncrementedVersion > v2.IncrementedVersion ? v1 : v2);
            
            var matchingVersionsOnceIncremented = 
                baseVersions
                    .Where(v => v.Version.Source != null &&
                                v.IncrementedVersion == maxVersion.IncrementedVersion)
                    .ToList();

            if (matchingVersionsOnceIncremented.Any())
            {
                var oldest = matchingVersionsOnceIncremented.Aggregate((v1, v2) =>
                    v1.Version.Source.Timestamp < v2.Version.Source.Timestamp ? v1 : v2);
             
                return GetResultBaseVersion(oldest.Version, oldest.Version.Source);
            }
            
            var baseVersionWithOldestSource = baseVersions
                .Where(v => v.Version.Source != null)
                .OrderByDescending(v => v.IncrementedVersion)
                .ThenByDescending(v => v.Version.Source.Timestamp)
                .First();
            
            return GetResultBaseVersion(maxVersion.Version, baseVersionWithOldestSource.Version.Source);
        }

        private BaseVersion GetResultBaseVersion(BaseVersion maxVersion, ICommit oldestCommit)
        {
            return new BaseVersion(
                maxVersion.Type,
                maxVersion.Version,
                oldestCommit,
                maxVersion.ShouldIncrement);
        }
        
        private struct BaseVersionTuple
        {
            public readonly SemanticVersion IncrementedVersion;
            public readonly BaseVersion Version;
            
            public BaseVersionTuple(SemanticVersion incrementedVersion, BaseVersion version)
            {
                IncrementedVersion = incrementedVersion;
                Version = version;
            }
        }
    }
}