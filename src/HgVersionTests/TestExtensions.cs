using System;
using HgVersion.Configuration;
using HgVersion.VCS;
using HgVersionTests.VCS;
using Shouldly;
using VCSVersion;
using VCSVersion.Configuration;
using VCSVersion.Output;
using VCSVersion.SemanticVersions;
using VCSVersion.VCS;

namespace HgVersionTests
{
    public static class TestExtensions
    {
        public static Config ApplyDefaults(this Config config)
        {
            HgConfigurationProvider.ApplyDefaultsTo(config);
            return config;
        }
        
        public static VersionVariables GetVersion(this TestVesionContext context, Config configuration = null, IRepository repository = null, string commitId = null, bool isForTrackedBranchOnly = true, string targetBranch = null)
        {
            configuration = configuration ?? new Config().ApplyDefaults();

            if (!string.IsNullOrEmpty(commitId))
            {
                context.Update(commitId);
            }
            
            var finder = new VersionFinder();
            var version = finder.FindVersion(context);
            
            return version.ToVersionVariables(context.Configuration, context.IsCurrentCommitTagged);
        }
        
        public static void AssertFullSemver(this TestVesionContext context, string fullSemver, IRepository repository = null, string commitId = null, bool isForTrackedBranchOnly = true, string targetBranch = null)
        {
            context.AssertFullSemver(new Config(), fullSemver, repository, commitId, isForTrackedBranchOnly, targetBranch);
        }
        
        public static void AssertFullSemver(this TestVesionContext context, Config configuration, string fullSemver, IRepository repository = null, string commitId = null, bool isForTrackedBranchOnly = true, string targetBranch = null)
        {
            configuration.ApplyDefaults();

            var variables = context.GetVersion(configuration, repository, commitId, isForTrackedBranchOnly, targetBranch);
            variables.FullSemVer.ShouldBe(fullSemver);
        }

        public static IHgRepository WithLogger(this IHgRepository repository)
        {
            return new RepositoryLogger(repository);
        }
    }
}