using HgVersion.Configuration;
using HgVersionTests.Helpers;
using NUnit.Framework;
using Shouldly;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using VCSVersion;
using VCSVersion.AssemblyVersioning;
using VCSVersion.Configuration;
using VCSVersion.Helpers;
using YamlDotNet.Serialization;

namespace HgVersionTests.Configuration
{
    [TestFixture, Parallelizable(ParallelScope.Self)]
    public class HgConfigurationProviderTests
    {
        private string repoPath;
        private IFileSystem fileSystem;

        [SetUp]
        public void Setup()
        {
            fileSystem = new TestFileSystem();
            repoPath = "c:\\MyHgRepo";
        }
        
        [Test]
        public void OverwritesDefaultsWithProvidedConfig()
        {
            var defaultConfig = HgConfigurationProvider.Provide(repoPath, fileSystem);
            const string text = @"
                next-version: 2.0.0
                branches:
                    develop:
                        mode: ContinuousDeployment
                        tag: dev";

            SetupConfigFileContent(text);
            var config = HgConfigurationProvider.Provide(repoPath, fileSystem);

            config.NextVersion.ShouldBe("2.0.0");
            config.Branches["develop"].Increment.ShouldBe(defaultConfig.Branches["develop"].Increment);
            config.Branches["develop"].VersioningMode.ShouldBe(defaultConfig.Branches["develop"].VersioningMode);
            config.Branches["develop"].Tag.ShouldBe("dev");
        }

        [Test]
        public void CanRemoveTag()
        {
            const string text = @"
                next-version: 2.0.0
                branches:
                    release:
                        tag: """"";

            SetupConfigFileContent(text);
            var config = HgConfigurationProvider.Provide(repoPath, fileSystem);

            config.NextVersion.ShouldBe("2.0.0");
            config.Branches["release"].Tag.ShouldBe(string.Empty);
        }

        [Test]
        public void RegexIsRequired()
        {
            const string text = @"
                next-version: 2.0.0
                branches:
                    bug:
                        tag: bugfix";

            SetupConfigFileContent(text);
            var ex = Should.Throw<ConfigurationException>(() => HgConfigurationProvider.Provide(repoPath, fileSystem));
            ex.Message.ShouldBe("Branch configuration 'bug' is missing required configuration 'regex'");
        }

        [Test]
        public void SourceBranchIsRequired()
        {
            const string text = @"
                next-version: 2.0.0
                branches:
                    bug:
                        regex: 'bug[/-]'
                        tag: bugfix";

            SetupConfigFileContent(text);
            var ex = Should.Throw<ConfigurationException>(() => HgConfigurationProvider.Provide(repoPath, fileSystem));
            ex.Message.ShouldBe("Branch configuration 'bug' is missing required configuration 'source-branches'");
        }

        [Test]
        public void CanProvideConfigForNewBranch()
        {
            const string text = @"
                next-version: 2.0.0
                branches:
                    bug:
                        regex: 'bug[/-]'
                        tag: bugfix
                        source-branches: []";

            SetupConfigFileContent(text);
            var config = HgConfigurationProvider.Provide(repoPath, fileSystem);

            config.Branches["bug"].Regex.ShouldBe("bug[/-]");
            config.Branches["bug"].Tag.ShouldBe("bugfix");
        }

        [Test]
        public void NextVersionCanBeInteger()
        {
            const string text = "next-version: 2";
            SetupConfigFileContent(text);
            var config = HgConfigurationProvider.Provide(repoPath, fileSystem);

            config.NextVersion.ShouldBe("2.0");
        }

        [Test]
        public void NextVersionCanHaveEnormousMinorVersion()
        {
            const string text = "next-version: 2.118998723";
            SetupConfigFileContent(text);
            var config = HgConfigurationProvider.Provide(repoPath, fileSystem);

            config.NextVersion.ShouldBe("2.118998723");
        }

        [Test]
        public void NextVersionCanHavePatch()
        {
            const string text = "next-version: 2.12.654651698";
            SetupConfigFileContent(text);
            var config = HgConfigurationProvider.Provide(repoPath, fileSystem);

            config.NextVersion.ShouldBe("2.12.654651698");
        }

        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void CanWriteOutEffectiveConfiguration()
        {
            var config = HgConfigurationProvider.GetEffectiveConfigAsString(repoPath, fileSystem);

            config.ShouldMatchApproved();
        }

        [Test]
        public void CanUpdateAssemblyInformationalVersioningScheme()
        {
            const string text = @"
                assembly-versioning-scheme: MajorMinor
                assembly-file-versioning-scheme: MajorMinorPatch
                assembly-informational-format: '{NugetVersion}'";

            SetupConfigFileContent(text);

            var config = HgConfigurationProvider.Provide(repoPath, fileSystem);
            config.AssemblyVersioningScheme.ShouldBe(AssemblyVersioningScheme.MajorMinor);
            config.AssemblyFileVersioningScheme.ShouldBe(AssemblyFileVersioningScheme.MajorMinorPatch);
            config.AssemblyInformationalFormat.ShouldBe("{NugetVersion}");
        }

        [Test]
        public void CanUpdateAssemblyInformationalVersioningSchemeWithMultipleVariables()
        {
            const string text = @"
                assembly-versioning-scheme: MajorMinor
                assembly-file-versioning-scheme: MajorMinorPatch
                assembly-informational-format: '{Major}.{Minor}.{Patch}'";

            SetupConfigFileContent(text);

            var config = HgConfigurationProvider.Provide(repoPath, fileSystem);
            config.AssemblyVersioningScheme.ShouldBe(AssemblyVersioningScheme.MajorMinor);
            config.AssemblyFileVersioningScheme.ShouldBe(AssemblyFileVersioningScheme.MajorMinorPatch);
            config.AssemblyInformationalFormat.ShouldBe("{Major}.{Minor}.{Patch}");
        }


        [Test]
        public void CanUpdateAssemblyInformationalVersioningSchemeWithFullSemVer()
        {
            const string text = @"
                assembly-versioning-scheme: MajorMinorPatch
                assembly-file-versioning-scheme: MajorMinorPatch
                assembly-informational-format: '{FullSemVer}'
                mode: ContinuousDelivery
                next-version: 5.3.0
                branches: {}";

            SetupConfigFileContent(text);

            var config = HgConfigurationProvider.Provide(repoPath, fileSystem);
            config.AssemblyVersioningScheme.ShouldBe(AssemblyVersioningScheme.MajorMinorPatch);
            config.AssemblyFileVersioningScheme.ShouldBe(AssemblyFileVersioningScheme.MajorMinorPatch);
            config.AssemblyInformationalFormat.ShouldBe("{FullSemVer}");
        }

        [Test]
        public void CanReadDefaultDocument()
        {
            const string text = "";
            SetupConfigFileContent(text);
            var config = HgConfigurationProvider.Provide(repoPath, fileSystem);
            config.AssemblyVersioningScheme.ShouldBe(AssemblyVersioningScheme.MajorMinorPatch);
            config.AssemblyFileVersioningScheme.ShouldBe(AssemblyFileVersioningScheme.MajorMinorPatch);
            config.AssemblyInformationalFormat.ShouldBe(null);
            config.Branches["develop"].Tag.ShouldBe("alpha");
            config.Branches["release"].Tag.ShouldBe("beta");
            config.TagPrefix.ShouldBe(HgConfigurationProvider.DefaultTagPrefix);
            config.NextVersion.ShouldBe(null);
        }

        [Test]
        public void VerifyAliases()
        {
            var config = typeof(Config);
            var propertiesMissingAlias = config.GetProperties()
                .Where(p => p.GetCustomAttribute<ObsoleteAttribute>() == null)
                .Where(p => p.GetCustomAttribute(typeof(YamlMemberAttribute)) == null)
                .Select(p => p.Name);

            propertiesMissingAlias.ShouldBeEmpty();
        }

        [Test]
        public void NoWarnOnHgVersionYmlFile()
        {
            SetupConfigFileContent(string.Empty);

            var s = string.Empty;
            Action<string> action = info => { s = info; };

            using (Logger.AddLoggersTemporarily(action, action, action, action))
            {
                HgConfigurationProvider.Provide(repoPath, fileSystem);
            }

            s.Length.ShouldBe(0);
        }

        private string SetupConfigFileContent(string text, string fileName = HgConfigurationProvider.DefaultConfigFileName)
        {
            return SetupConfigFileContent(text, fileName, repoPath);
        }

        private string SetupConfigFileContent(string text, string fileName, string path)
        {
            var fullPath = Path.Combine(path, fileName);
            fileSystem.WriteAllText(fullPath, text);

            return fullPath;
        }
    }
}
