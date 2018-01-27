using HgVersion.Configuration;
using HgVersion.VCS;
using VCSVersion;
using VCSVersion.Configuration;
using VCSVersion.Helpers;
using VCSVersion.SemanticVersions;
using VCSVersion.VCS;
using VCSVersion.VersionCalculation.BaseVersionCalculation;
using System.Linq;

namespace HgVersion
{
    /// <inheritdoc />
    public class HgVersionContext : IVersionContext
    {
        /// <inheritdoc />
        public IRepository Repository { get; }

        /// <inheritdoc />
        public IFileSystem FileSystem { get; }
        
        /// <inheritdoc />
        public Config FullConfiguration { get; }

        /// <inheritdoc />
        public EffectiveConfiguration Configuration { get; }

        /// <inheritdoc />
        public IBranchHead CurrentBranch { get; }

        /// <inheritdoc />
        public ICommit CurrentCommit { get; }

        /// <inheritdoc />
        public IRepositoryMetadataProvider RepositoryMetadataProvider { get; }

        /// <inheritdoc />
        public bool IsCurrentCommitTagged { get; }

        /// <inheritdoc />
        public SemanticVersion CurrentCommitTaggedVersion { get; }

        /// <summary>
        /// Creates an instance of <see cref="HgVersionContext"/>
        /// </summary>
        /// <param name="repository">Mercurial retpository</param>
        public HgVersionContext(IHgRepository repository)
        {
            Repository = repository;
            CurrentBranch = repository.CurrentBranch();
            CurrentCommit = repository.CurrentCommit();
            FileSystem = new FileSystem();
            FullConfiguration = HgConfigurationProvider.Provide(repository, FileSystem);
            RepositoryMetadataProvider = new HgRepositoryMetadataProvider(repository, FullConfiguration);
            Configuration = CalculateEffectiveConfiguration();
            CurrentCommitTaggedVersion = CalculateCurrentCommitTaggedVersion();
            IsCurrentCommitTagged = CurrentCommitTaggedVersion != null;
        }
        
        private EffectiveConfiguration CalculateEffectiveConfiguration()
        {
            var currentBranchConfig = BranchConfigurationCalculator.GetBranchConfiguration(this, CurrentBranch);

            ThrowIfNull(currentBranchConfig.VersioningMode, nameof(currentBranchConfig.VersioningMode));
            ThrowIfNull(currentBranchConfig.Increment, nameof(currentBranchConfig.Increment));
            ThrowIfNull(currentBranchConfig.PreventIncrementOfMergedBranchVersion, nameof(currentBranchConfig.PreventIncrementOfMergedBranchVersion));
            ThrowIfNull(currentBranchConfig.TrackMergeTarget, nameof(currentBranchConfig.TrackMergeTarget));
            ThrowIfNull(currentBranchConfig.TracksReleaseBranches, nameof(currentBranchConfig.TracksReleaseBranches));
            ThrowIfNull(currentBranchConfig.IsReleaseBranch, nameof(currentBranchConfig.IsReleaseBranch));
            
            ThrowIfNull(FullConfiguration.AssemblyVersioningScheme, nameof(FullConfiguration.AssemblyVersioningScheme));
            ThrowIfNull(FullConfiguration.AssemblyFileVersioningScheme, nameof(FullConfiguration.AssemblyFileVersioningScheme));
            ThrowIfNull(FullConfiguration.CommitMessageIncrementing, nameof(FullConfiguration.CommitMessageIncrementing));
            ThrowIfNull(FullConfiguration.BuildMetaDataPadding, nameof(FullConfiguration.BuildMetaDataPadding));
            ThrowIfNull(FullConfiguration.CommitsSinceVersionSourcePadding, nameof(FullConfiguration.CommitsSinceVersionSourcePadding));
            ThrowIfNull(FullConfiguration.TaggedCommitsLimit, nameof(FullConfiguration.TaggedCommitsLimit));
            
            var versioningMode = currentBranchConfig.VersioningMode.GetValueOrDefault();
            var tag = currentBranchConfig.Tag;
            var tagNumberPattern = currentBranchConfig.TagNumberPattern;
            var incrementStrategy = currentBranchConfig.Increment.GetValueOrDefault();
            var preventIncrementForMergedBranchVersion = currentBranchConfig.PreventIncrementOfMergedBranchVersion.GetValueOrDefault();
            var trackMergeTarget = currentBranchConfig.TrackMergeTarget.GetValueOrDefault();

            var nextVersion = FullConfiguration.NextVersion;
            var assemblyVersioningScheme = FullConfiguration.AssemblyVersioningScheme.GetValueOrDefault();
            var assemblyFileVersioningScheme = FullConfiguration.AssemblyFileVersioningScheme.GetValueOrDefault();
            var assemblyInformationalFormat = FullConfiguration.AssemblyInformationalFormat;
            var tagPrefix = FullConfiguration.TagPrefix;
            var majorMessage = FullConfiguration.MajorVersionBumpMessage;
            var minorMessage = FullConfiguration.MinorVersionBumpMessage;
            var patchMessage = FullConfiguration.PatchVersionBumpMessage;
            var noBumpMessage = FullConfiguration.NoBumpMessage;
            var commitDateFormat = FullConfiguration.CommitDateFormat;
            var baseVersionStrategies = FullConfiguration.BaseVersionStrategies;
            var taggedCommitsLimit = FullConfiguration.TaggedCommitsLimit;
            var commitMessageVersionBump = currentBranchConfig.CommitMessageIncrementing ?? FullConfiguration.CommitMessageIncrementing.GetValueOrDefault();
            
            return new EffectiveConfiguration(
                assemblyVersioningScheme, assemblyFileVersioningScheme, assemblyInformationalFormat, versioningMode, tagPrefix,
                tag, nextVersion, incrementStrategy,
                currentBranchConfig.Regex,
                preventIncrementForMergedBranchVersion,
                tagNumberPattern, FullConfiguration.ContinuousDeploymentFallbackTag,
                trackMergeTarget,
                majorMessage, minorMessage, patchMessage, noBumpMessage,
                commitMessageVersionBump,
                FullConfiguration.BuildMetaDataPadding.GetValueOrDefault(),
                FullConfiguration.CommitsSinceVersionSourcePadding.GetValueOrDefault(),
                FullConfiguration.Ignore.ToFilters(),
                currentBranchConfig.TracksReleaseBranches.GetValueOrDefault(),
                currentBranchConfig.IsReleaseBranch.GetValueOrDefault(),
                commitDateFormat,
                ConfigHelper.GetEntities<IBaseVersionStrategy>(baseVersionStrategies),
                taggedCommitsLimit.GetValueOrDefault());
        }

        private SemanticVersion CalculateCurrentCommitTaggedVersion()
        {
            return CurrentCommit
                .Tags
                .SelectMany(tag =>
                {
                    if (SemanticVersion.TryParse(tag.Name, Configuration.TagPrefix, out var version))
                        return new[] { version };

                    return Enumerable.Empty<SemanticVersion>();
                })
                .Max();
        }

        private static void ThrowIfNull<T>(T? value, string name) where T : struct 
        {
            if (!value.HasValue)
                throw new HgConfigrationException($"Configuration value for '{name}' has no value. (this should not happen, please report an issue)");
        }
    }
}
