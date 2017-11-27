using System;
using System.Collections.Generic;
using System.Linq;
using HgVersion.Exceptions;
using HgVersion.SemanticVersions;
using HgVersion.VCS;

namespace HgVersion.VersionCalculation.BaseVersionCalculation
{
    /// <summary>
    /// Version is 0.1.0.
    /// <see cref="BaseVersion.Source"/> is the "root" commit reachable from the current commit.
    /// Does not increment.
    /// </summary>
    public sealed class FallbackBaseVersionStrategy : IBaseVersionStrategy
    {
        public IEnumerable<BaseVersion> GetVersions(IVersionContext context)
        {
            var repository = context.Repository;
            var tip = repository.Tip();
            var root = GetRootCommit(repository, tip);
            var semVersion = new SemanticVersion(minor: 1);
            
            yield return new BaseVersion("Fallback base version", semVersion, root, shouldIncrement: false);
        }

        private static ICommit GetRootCommit(IRepository repository, ICommit tip)
        {
            try
            {
                return repository
                    .Log(select => select
                        .AncestorsOf(select.Single(tip.Hash))
                        .Limit(1))
                    .Single();
            }
            catch (InvalidOperationException exception)
            {
                // todo: ensure error message
                throw new BaseVerisonException(
                    $"Can't find commit {tip.Hash}. Please ensure that the repository is an unshallow clone with `git fetch --unshallow`.",
                    exception);
            }
        }
    }
}