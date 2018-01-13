using System;
using System.Collections.Generic;
using System.Linq;
using VCSVersion;
using VCSVersion.Configuration;
using VCSVersion.SemanticVersions;
using VCSVersion.VCS;

namespace HgVersion.VCS
{
    /// <inheritdoc />
    public sealed class HgRepositoryMetadataProvider : IRepositoryMetadataProvider
    {
        private Dictionary<Tuple<IBranchHead, IBranchHead>, MergeBaseData> _mergeBaseCache;
        private Dictionary<IBranchHead, List<SemanticVersion>> _semanticVersionTagsOnBranchCache;
        private IHgRepository _repository;
        private Config _configuration;

        public HgRepositoryMetadataProvider(IHgRepository repository, Config configuration)
        {
            _mergeBaseCache = new Dictionary<Tuple<IBranchHead, IBranchHead>, MergeBaseData>();
            _semanticVersionTagsOnBranchCache = new Dictionary<IBranchHead, List<SemanticVersion>>();
            _repository = repository;
            _configuration = configuration;
        }

        /// <inheritdoc />
        public IEnumerable<SemanticVersion> GetVersionTagsOnBranch(IBranchHead branch, string tagPrefixRegex)
        {
            if (_semanticVersionTagsOnBranchCache.ContainsKey(branch))
            {
                Logger.WriteDebug($"Cache hit for version tags on branch '{branch.Name}");
                return _semanticVersionTagsOnBranchCache[branch];
            }

            using (Logger.IndentLog($"Getting version tags from branch '{branch.Name}'."))
            {
                var builder = new HgLogQueryBuilder();
                var tags = _repository
                    .Log(builder.TaggedBranchCommits(branch.Name))
                    .SelectMany(commit => commit.Tags)
                    .ToList();

                var versionTags = tags
                    .SelectMany(tag =>
                    {
                        if (SemanticVersion.TryParse(tag, tagPrefixRegex, out var semver))
                            return new[] { semver };

                        return Enumerable.Empty<SemanticVersion>();
                    })
                    .ToList();

                _semanticVersionTagsOnBranchCache.Add(branch, versionTags);
                return versionTags;
            }
        }

        /// <inheritdoc />
        public IEnumerable<IBranchHead> GetBranchesContainingCommit(ICommit commit, IList<IBranchHead> branches, bool onlyTrackedBranches)
        {
            if (commit == null)
                throw new ArgumentNullException(nameof(commit));

            using (Logger.IndentLog($"Getting branches containing the commit '{commit.Hash}'."))
            {
                Logger.WriteInfo("Trying to find direct branches.");

                return branches
                    .Where(branch => Equals(branch.Commit, commit));
            }
        }

        /// <inheritdoc />
        public ICommit FindMergeBase(IBranchHead branch, IBranchHead otherBranch)
        {
            var key = Tuple.Create(branch, otherBranch);

            if (_mergeBaseCache.ContainsKey(key))
            {
                Logger.WriteDebug($"Cache hit for merge base between '{branch.Name}' and '{otherBranch.Name}'.");
                return _mergeBaseCache[key].MergeBase;
            }

            using (Logger.IndentLog($"Finding merge base between '{branch.Name}' and '{otherBranch.Name}'."))
            {
                var select = new HgLogQueryBuilder();
                var findMergeBase = _repository
                    .Log(select.CommonAncestorOf(
                        select.ByBranch(branch.Name),
                        select.ByBranch(branch.Name)))
                    .FirstOrDefault();
                
                // Store in cache.
                _mergeBaseCache.Add(key, new MergeBaseData(branch, otherBranch, _repository, findMergeBase));

                Logger.WriteInfo($"Merge base of {branch.Name}' and '{otherBranch.Name} is {findMergeBase}");
                return findMergeBase;
            }
        }

        /// <inheritdoc />
        public ICommit FindCommitWasBranchedFrom(IBranchHead branch, params IBranchHead[] excludedBranches)
        {
            if (branch == null)
                throw new ArgumentNullException(nameof(branch));

            using (Logger.IndentLog($"Finding branch source of '{branch.Name}'"))
            {
                var builder = new HgLogQueryBuilder();
                var possibleBranches = _repository
                    .Log(builder.ByBranch(branch.Name)
                        .Limit(1)
                        .Parents()
                        .Except(excludedBranches.Select(b => b.Name).ToArray()))
                    .ToList();

                if (possibleBranches.Count > 1)
                {
                    var first = possibleBranches.First();
                    Logger.WriteInfo(
                        $"Multiple source branches have been found, picking the first one ({first.Branch}).\n" +
                        "This may result in incorrect commit counting.\nOptions were:\n " +
                        string.Join(", ", possibleBranches.Select(b => b.Branch)));

                    return first;
                }

                return possibleBranches.SingleOrDefault();
            }
        }

        /// <inheritdoc />
        public IMergeMessage ParseMergeMessage(string message)
        {
            return new HgMergeMessage(message, _configuration);
        }

        private class MergeBaseData
        {
            public IBranchHead Branch { get; }
            public IBranchHead OtherBranch { get; }
            public IRepository Repository { get; }
            public ICommit MergeBase { get; }

            public MergeBaseData(IBranchHead branch, IBranchHead otherBranch, IRepository repository, ICommit mergeBase)
            {
                Branch = branch;
                OtherBranch = otherBranch;
                Repository = repository;
                MergeBase = mergeBase;
            }
        }
    }
}
