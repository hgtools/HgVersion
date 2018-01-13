using HgVersionTests.Configuration;
using HgVersionTests.Helpers;
using NUnit.Framework;
using Shouldly;
using System;
using System.IO;
using HgVersion.VersionInformationResources;
using VCSVersion.Output;
using VCSVersion.SemanticVersions;

namespace HgVersionTests.VersionInformationResources
{
    [TestFixture, Parallelizable(ParallelScope.All)]
    public class VersionInformationGeneratorTests
    {
        [Test]
        [TestCase("cs")]
        [TestCase("fs")]
        [TestCase("vb")]
        public void ShouldCreateFile(string fileExtension)
        {
            var fileSystem = new TestFileSystem();
            var directory = Path.GetTempPath();
            var fileName = "VersionInformationGenerator.g." + fileExtension;
            var fullPath = Path.Combine(directory, fileName);

            var version = new SemanticVersion
            (
                major: 1,
                minor: 2,
                patch: 3,
                preReleaseTag: "unstable4",
                buildMetadata: new BuildMetadata(5, "feature1", 
                    "commitSha", DateTimeOffset.Parse("2014-03-06 23:59:59Z"))
            );

            var variables = version.ToVersionVariables(new TestEffectiveConfiguration());
            var generator = new VersionInformationGenerator(fileName, directory, variables, fileSystem);
            generator.Generate();

            fileSystem.ReadAllText(fullPath)
                .ShouldMatchApproved(c => c
                    .NoDiff()
                    .WithDescriminator(fileExtension));
        }
    }
}
