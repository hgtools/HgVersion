using HgVersion.VersionAssemblyInfoResources;
using HgVersionTests.Configuration;
using HgVersionTests.Helpers;
using Moq;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using VCSVersion.AssemblyVersioning;
using VCSVersion.Helpers;
using VCSVersion.Output;
using VCSVersion.SemanticVersions;

namespace HgVersionTests.VersionAssemblyInfoResources
{
    [TestFixture, Parallelizable(ParallelScope.All)]
    public class AssemblyInfoFileUpdaterTests
    {
        [Test]
        [TestCase("cs")]
        [TestCase("fs")]
        [TestCase("vb")]
        public void ShouldCreateAssemblyInfoFileWhenNotExistsAndEnsureAssemblyInfo(string fileExtension)
        {
            var fileSystem = new TestFileSystem();
            var workingDir = Path.GetTempPath();
            var assemblyInfoFile = "VersionAssemblyInfo." + fileExtension;
            var fullPath = Path.Combine(workingDir, assemblyInfoFile);

            var version = SemanticVersion.Parse("1.0.0", "v");
            var variables = version.ToVersionVariables(new TestEffectiveConfiguration());

            using (var assemblyInfoFileUpdater = new AssemblyInfoFileUpdater(assemblyInfoFile, workingDir, variables, fileSystem, true))
            {
                assemblyInfoFileUpdater.Update();

                fileSystem.ReadAllText(fullPath)
                    .ShouldMatchApproved(c => c
                        .NoDiff()
                        .WithDescriminator(fileExtension));
            }
        }

        [Test]
        [TestCase("cs")]
        [TestCase("fs")]
        [TestCase("vb")]
        public void ShouldCreateAssemblyInfoFileAtPathWhenNotExistsAndEnsureAssemblyInfo(string fileExtension)
        {
            var fileSystem = new TestFileSystem();
            var workingDir = Path.GetTempPath();
            var assemblyInfoFile = Path.Combine("src", "Project", "Properties", "VersionAssemblyInfo." + fileExtension);
            var fullPath = Path.Combine(workingDir, assemblyInfoFile);

            var version = SemanticVersion.Parse("1.0.0", "v");
            var variables = version.ToVersionVariables(new TestEffectiveConfiguration());

            using (var assemblyInfoFileUpdater = new AssemblyInfoFileUpdater(assemblyInfoFile, workingDir, variables, fileSystem, true))
            {
                assemblyInfoFileUpdater.Update();

                fileSystem.ReadAllText(fullPath)
                    .ShouldMatchApproved(c => c
                        .NoDiff()
                        .WithDescriminator(fileExtension));
            }
        }

        [Test]
        [TestCase("cs")]
        [TestCase("fs")]
        [TestCase("vb")]
        public void ShouldCreateAssemblyInfoFilesAtPathWhenNotExistsAndEnsureAssemblyInfo(string fileExtension)
        {
            var fileSystem = new TestFileSystem();
            var workingDir = Path.GetTempPath();
            var assemblyInfoFiles = new HashSet<string>
            {
                "AssemblyInfo." + fileExtension,
                Path.Combine("src", "Project", "Properties", "VersionAssemblyInfo." + fileExtension)
            };

            var version = SemanticVersion.Parse("1.0.0", "v");
            var variables = version.ToVersionVariables(new TestEffectiveConfiguration());

            using (var assemblyInfoFileUpdater = new AssemblyInfoFileUpdater(assemblyInfoFiles, workingDir, variables, fileSystem, true))
            {
                assemblyInfoFileUpdater.Update();

                foreach (var item in assemblyInfoFiles)
                {
                    var fullPath = Path.Combine(workingDir, item);
                    fileSystem.ReadAllText(fullPath)
                        .ShouldMatchApproved(c => c
                            .NoDiff()
                            .WithDescriminator(fileExtension));
                }
            }
        }

        [Test]
        [TestCase("cs")]
        [TestCase("fs")]
        [TestCase("vb")]
        public void ShouldNotCreateAssemblyInfoFileWhenNotExistsAndNotEnsureAssemblyInfo(string fileExtension)
        {
            var fileSystem = new TestFileSystem();
            var workingDir = Path.GetTempPath();
            var assemblyInfoFile = "VersionAssemblyInfo." + fileExtension;
            var fullPath = Path.Combine(workingDir, assemblyInfoFile);

            var version = SemanticVersion.Parse("1.0.0", "v");
            var variables = version.ToVersionVariables(new TestEffectiveConfiguration());

            using (var assemblyInfoFileUpdater = new AssemblyInfoFileUpdater(assemblyInfoFile, workingDir, variables, fileSystem, false))
            {
                assemblyInfoFileUpdater.Update();
                fileSystem.Exists(fullPath).ShouldBeFalse();
            }
        }

        [Test]
        public void ShouldNotCreateAssemblyInfoFileForUnknownSourceCodeAndEnsureAssemblyInfo()
        {
            var workingDir = Path.GetTempPath();
            var assemblyInfoFile = "VersionAssemblyInfo.js";
            var fullPath = Path.Combine(workingDir, assemblyInfoFile);

            var fileSystemMock = new Mock<IFileSystem>();
            var fileSystem = fileSystemMock.Object;

            var version = SemanticVersion.Parse("1.0.0", "v");
            var variables = version.ToVersionVariables(new TestEffectiveConfiguration());

            using (var assemblyInfoFileUpdater = new AssemblyInfoFileUpdater(assemblyInfoFile, workingDir, variables, fileSystem, true))
            {
                assemblyInfoFileUpdater.Update();

                fileSystemMock.Verify(f => f.WriteAllText(fullPath, It.IsAny<string>()),
                    Times.Never());
            }
        }

        [Test]
        public void ShouldStartSearchFromWorkingDirectory()
        {
            var workingDir = Path.GetTempPath();
            var assemblyInfoFiles = new HashSet<string>();

            var fileSystemMock = new Mock<IFileSystem>();
            var fileSystem = fileSystemMock.Object;

            var version = SemanticVersion.Parse("1.0.0", "v");
            var variables = version.ToVersionVariables(new TestEffectiveConfiguration());

            using (var assemblyInfoFileUpdater = new AssemblyInfoFileUpdater(assemblyInfoFiles, workingDir, variables, fileSystem, false))
            {
                assemblyInfoFileUpdater.Update();

                fileSystemMock.Verify(f => f.DirectoryGetFiles(workingDir, It.IsAny<string>(), It.IsAny<SearchOption>()),
                    Times.Once());
            }
        }

        [Test]
        [TestCase("cs", "[assembly: AssemblyFileVersion(\"1.0.0.0\")]")]
        [TestCase("fs", "[<assembly: AssemblyFileVersion(\"1.0.0.0\")>]")]
        [TestCase("vb", "<Assembly: AssemblyFileVersion(\"1.0.0.0\")>")]
        public void ShouldNotReplaceAssemblyVersionWhenVersionSchemeIsNone(string fileExtension, string assemblyFileContent)
        {
            var assemblyInfoFile = "AssemblyInfo." + fileExtension;

            VerifyAssemblyInfoFile(assemblyFileContent, assemblyInfoFile, AssemblyVersioningScheme.None,
                verify: content => content.ShouldMatchApproved(c => c
                    .NoDiff()
                    .LocateTestMethodUsingAttribute<TestCaseAttribute>()
                    .WithDescriminator(fileExtension)));
        }

        [Test]
        [TestCase("cs", "[assembly: AssemblyVersion(\"1.0.0.0\")]\r\n[assembly: AssemblyFileVersion(\"1.0.0.0\")]")]
        [TestCase("fs", "[<assembly: AssemblyVersion(\"1.0.0.0\")>]\r\n[<assembly: AssemblyFileVersion(\"1.0.0.0\")>]")]
        [TestCase("vb", "<Assembly: AssemblyVersion(\"1.0.0.0\")>\r\n<Assembly: AssemblyFileVersion(\"1.0.0.0\")>")]
        public void ShouldAddAssemblyInformationalVersionWhenUpdatingAssemblyVersionFile(string fileExtension, string assemblyFileContent)
        {
            var workingDir = Path.GetTempPath();
            var assemblyInfoFile = "AssemblyInfo." + fileExtension;
            var fileName = Path.Combine(workingDir, assemblyInfoFile);

            VerifyAssemblyInfoFile(assemblyFileContent, assemblyInfoFile,
                verify: content => content.ShouldMatchApproved(c => c
                    .NoDiff()
                    .LocateTestMethodUsingAttribute<TestCaseAttribute>()
                    .WithDescriminator(fileExtension)));
        }

        [Test]
        [TestCase("cs", "[assembly: AssemblyFileVersion(\"1.0.0.0\")]")]
        [TestCase("fs", "[<assembly: AssemblyFileVersion(\"1.0.0.0\")>]")]
        [TestCase("vb", "<Assembly: AssemblyFileVersion(\"1.0.0.0\")>")]
        public void ShouldNotAddAssemblyInformationalVersionWhenUpdatingAssemblyVersionFileWhenVersionSchemeIsNone(string fileExtension, string assemblyFileContent)
        {
            var workingDir = Path.GetTempPath();
            var assemblyInfoFile = "AssemblyInfo." + fileExtension;
            var fileName = Path.Combine(workingDir, assemblyInfoFile);

            VerifyAssemblyInfoFile(assemblyFileContent, assemblyInfoFile, AssemblyVersioningScheme.None,
                verify: content => content.ShouldMatchApproved(c => c
                    .NoDiff()
                    .LocateTestMethodUsingAttribute<TestCaseAttribute>()
                    .WithDescriminator(fileExtension)));
        }

        [Test]
        [TestCase("cs", "[assembly: AssemblyVersion(\"1.0.0.0\")]\r\n[assembly: AssemblyInformationalVersion(\"1.0.0.0\")]\r\n[assembly: AssemblyFileVersion(\"1.0.0.0\")]")]
        [TestCase("fs", "[<assembly: AssemblyVersion(\"1.0.0.0\")>]\r\n[<assembly: AssemblyInformationalVersion(\"1.0.0.0\")>]\r\n[<assembly: AssemblyFileVersion(\"1.0.0.0\")>]")]
        [TestCase("vb", "<Assembly: AssemblyVersion(\"1.0.0.0\")>\r\n<Assembly: AssemblyInformationalVersion(\"1.0.0.0\")>\r\n<Assembly: AssemblyFileVersion(\"1.0.0.0\")>")]
        public void ShouldReplaceAssemblyVersion(string fileExtension, string assemblyFileContent)
        {
            var fileName = "AssemblyInfo." + fileExtension;

            VerifyAssemblyInfoFile(assemblyFileContent, fileName, AssemblyVersioningScheme.MajorMinor,
                match: content => content.Contains(@"AssemblyVersion(""2.3.0.0"")") &&
                    content.Contains(@"AssemblyInformationalVersion(""2.3.1+3.Branch.foo.Sha.hash"")") &&
                    content.Contains(@"AssemblyFileVersion(""2.3.1.0"")"));
        }

        [Test]
        [TestCase("cs", "[assembly: AssemblyVersion(\"1.0.0.0\")]\r\n[assembly: AssemblyInformationalVersion(\"1.0.0.0\")]\r\n[assembly: AssemblyFileVersion(\"1.0.0.0\")]")]
        [TestCase("fs", "[<assembly: AssemblyVersion(\"1.0.0.0\")>]\r\n[<assembly: AssemblyInformationalVersion(\"1.0.0.0\")>]\r\n[<assembly: AssemblyFileVersion(\"1.0.0.0\")>]")]
        [TestCase("vb", "<Assembly: AssemblyVersion(\"1.0.0.0\")>\r\n<Assembly: AssemblyInformationalVersion(\"1.0.0.0\")>\r\n<Assembly: AssemblyFileVersion(\"1.0.0.0\")>")]
        public void ShouldReplaceAssemblyVersionInRelativePath(string fileExtension, string assemblyFileContent)
        {
            var assemblyInfoFile = Path.Combine("Project", "src", "Properties", "AssemblyInfo." + fileExtension);

            VerifyAssemblyInfoFile(assemblyFileContent, assemblyInfoFile, AssemblyVersioningScheme.MajorMinor,
                match: content => content.Contains(@"AssemblyVersion(""2.3.0.0"")") &&
                    content.Contains(@"AssemblyInformationalVersion(""2.3.1+3.Branch.foo.Sha.hash"")") &&
                    content.Contains(@"AssemblyFileVersion(""2.3.1.0"")"));
        }

        [Test]
        [TestCase("cs", "[assembly: AssemblyVersion ( \"1.0.0.0\") ]\r\n[assembly: AssemblyInformationalVersion\t(\t\"1.0.0.0\"\t)]\r\n[assembly: AssemblyFileVersion\r\n(\r\n\"1.0.0.0\"\r\n)]")]
        [TestCase("fs", "[<assembly: AssemblyVersion ( \"1.0.0.0\" )>]\r\n[<assembly: AssemblyInformationalVersion\t(\t\"1.0.0.0\"\t)>]\r\n[<assembly: AssemblyFileVersion\r\n(\r\n\"1.0.0.0\"\r\n)>]")]
        [TestCase("vb", "<Assembly: AssemblyVersion ( \"1.0.0.0\" )>\r\n<Assembly: AssemblyInformationalVersion\t(\t\"1.0.0.0\"\t)>\r\n<Assembly: AssemblyFileVersion\r\n(\r\n\"1.0.0.0\"\r\n)>")]
        public void ShouldReplaceAssemblyVersionInRelativePathWithWhiteSpace(string fileExtension, string assemblyFileContent)
        {
            var assemblyInfoFile = Path.Combine("Project", "src", "Properties", "AssemblyInfo." + fileExtension);

            VerifyAssemblyInfoFile(assemblyFileContent, assemblyInfoFile, AssemblyVersioningScheme.MajorMinor,
                match: content => content.Contains(@"AssemblyVersion(""2.3.0.0"")") &&
                    content.Contains(@"AssemblyInformationalVersion(""2.3.1+3.Branch.foo.Sha.hash"")") &&
                    content.Contains(@"AssemblyFileVersion(""2.3.1.0"")"));
        }

        [Test]
        [TestCase("cs", "[assembly: AssemblyVersion(\"1.0.0.*\")]\r\n[assembly: AssemblyInformationalVersion(\"1.0.0.*\")]\r\n[assembly: AssemblyFileVersion(\"1.0.0.*\")]")]
        [TestCase("fs", "[<assembly: AssemblyVersion(\"1.0.0.*\")>]\r\n[<assembly: AssemblyInformationalVersion(\"1.0.0.*\")>]\r\n[<assembly: AssemblyFileVersion(\"1.0.0.*\")>]")]
        [TestCase("vb", "<Assembly: AssemblyVersion(\"1.0.0.*\")>\r\n<Assembly: AssemblyInformationalVersion(\"1.0.0.*\")>\r\n<Assembly: AssemblyFileVersion(\"1.0.0.*\")>")]
        public void ShouldReplaceAssemblyVersionWithStar(string fileExtension, string assemblyFileContent)
        {
            var assemblyInfoFile = "AssemblyInfo." + fileExtension;

            VerifyAssemblyInfoFile(assemblyFileContent, assemblyInfoFile, AssemblyVersioningScheme.MajorMinor,
                match: content => content.Contains(@"AssemblyVersion(""2.3.0.0"")") &&
                    content.Contains(@"AssemblyInformationalVersion(""2.3.1+3.Branch.foo.Sha.hash"")") &&
                    content.Contains(@"AssemblyFileVersion(""2.3.1.0"")"));
        }

        [Test]
        [TestCase("cs", "[assembly: AssemblyVersionAttribute(\"1.0.0.0\")]\r\n[assembly: AssemblyInformationalVersionAttribute(\"1.0.0.0\")]\r\n[assembly: AssemblyFileVersionAttribute(\"1.0.0.0\")]")]
        [TestCase("fs", "[<assembly: AssemblyVersionAttribute(\"1.0.0.0\")>]\r\n[<assembly: AssemblyInformationalVersionAttribute(\"1.0.0.0\")>]\r\n[<assembly: AssemblyFileVersionAttribute(\"1.0.0.0\")>]")]
        [TestCase("vb", "<Assembly: AssemblyVersionAttribute(\"1.0.0.0\")>\r\n<Assembly: AssemblyInformationalVersionAttribute(\"1.0.0.0\")>\r\n<Assembly: AssemblyFileVersionAttribute(\"1.0.0.0\")>")]
        public void ShouldReplaceAssemblyVersionWithAtttributeSuffix(string fileExtension, string assemblyFileContent)
        {
            var assemblyInfoFile = "AssemblyInfo." + fileExtension;

            VerifyAssemblyInfoFile(assemblyFileContent, assemblyInfoFile, 
                match: content => !content.Contains(@"AssemblyVersionAttribute(""1.0.0.0"")") &&
                    !content.Contains(@"AssemblyInformationalVersionAttribute(""1.0.0.0"")") &&
                    !content.Contains(@"AssemblyFileVersionAttribute(""1.0.0.0"")") &&
                    content.Contains(@"AssemblyVersion(""2.3.1.0"")") &&
                    content.Contains(@"AssemblyInformationalVersion(""2.3.1+3.Branch.foo.Sha.hash"")") &&
                    content.Contains(@"AssemblyFileVersion(""2.3.1.0"")"));
        }

        [Test]
        [TestCase("cs")]
        [TestCase("fs")]
        [TestCase("vb")]
        public void ShouldAddAssemblyVersionIfMissingFromInfoFile(string fileExtension)
        {
            var assemblyInfoFile = "AssemblyInfo." + fileExtension;

            VerifyAssemblyInfoFile("", assemblyInfoFile, AssemblyVersioningScheme.MajorMinor,
                match: content => content.Contains(@"AssemblyVersion(""2.3.0.0"")") &&
                    content.Contains(@"AssemblyInformationalVersion(""2.3.1+3.Branch.foo.Sha.hash"")") &&
                    content.Contains(@"AssemblyFileVersion(""2.3.1.0"")"));
        }

        [Test]
        [TestCase("cs", "[assembly: AssemblyVersion(\"2.2.0.0\")]\r\n[assembly: AssemblyInformationalVersion(\"2.2.0+5.Branch.foo.Sha.hash\")]\r\n[assembly: AssemblyFileVersion(\"2.2.0.0\")]")]
        [TestCase("fs", "[<assembly: AssemblyVersion(\"2.2.0.0\")>]\r\n[<assembly: AssemblyInformationalVersion(\"2.2.0+5.Branch.foo.Sha.hash\")>]\r\n[<assembly: AssemblyFileVersion(\"2.2.0.0\")>]")]
        [TestCase("vb", "<Assembly: AssemblyVersion(\"2.2.0.0\")>\r\n<Assembly: AssemblyInformationalVersion(\"2.2.0+5.Branch.foo.Sha.hash\")>\r\n<Assembly: AssemblyFileVersion(\"2.2.0.0\")>")]
        public void ShouldReplaceAlreadySubstitutedValues(string fileExtension, string assemblyFileContent)
        {
            var assemblyInfoFile = "AssemblyInfo." + fileExtension;

            VerifyAssemblyInfoFile(assemblyFileContent, assemblyInfoFile, AssemblyVersioningScheme.MajorMinor,
                match: content => content.Contains(@"AssemblyVersion(""2.3.0.0"")") &&
                    content.Contains(@"AssemblyInformationalVersion(""2.3.1+3.Branch.foo.Sha.hash"")") &&
                    content.Contains(@"AssemblyFileVersion(""2.3.1.0"")"));
        }

        [Test]
        [TestCase("cs", "[assembly: AssemblyVersion(\"1.0.0.0\")]\r\n[assembly: AssemblyInformationalVersion(\"1.0.0.0\")]\r\n[assembly: AssemblyFileVersion(\"1.0.0.0\")]")]
        [TestCase("fs", "[<assembly: AssemblyVersion(\"1.0.0.0\")>]\r\n[<assembly: AssemblyInformationalVersion(\"1.0.0.0\")>]\r\n[<assembly: AssemblyFileVersion(\"1.0.0.0\")>]")]
        [TestCase("vb", "<Assembly: AssemblyVersion(\"1.0.0.0\")>\r\n<Assembly: AssemblyInformationalVersion(\"1.0.0.0\")>\r\n<Assembly: AssemblyFileVersion(\"1.0.0.0\")>")]
        public void ShouldReplaceAssemblyVersionWhenCreatingAssemblyVersionFileAndEnsureAssemblyInfo(string fileExtension, string assemblyFileContent)
        {
            var assemblyInfoFile = "AssemblyInfo." + fileExtension;

            VerifyAssemblyInfoFile(assemblyFileContent, assemblyInfoFile,
                match: content => content.Contains(@"AssemblyVersion(""2.3.1.0"")") &&
                    content.Contains(@"AssemblyInformationalVersion(""2.3.1+3.Branch.foo.Sha.hash"")") &&
                    content.Contains(@"AssemblyFileVersion(""2.3.1.0"")"));
        }

        [Test]
        [TestCase("cs", "[assembly: AssemblyVersion (AssemblyInfo.Version) ]\r\n[assembly: AssemblyInformationalVersion(AssemblyInfo.InformationalVersion)]\r\n[assembly: AssemblyFileVersion(AssemblyInfo.FileVersion)]")]
        [TestCase("fs", "[<assembly: AssemblyVersion (AssemblyInfo.Version)>]\r\n[<assembly: AssemblyInformationalVersion(AssemblyInfo.InformationalVersion)>]\r\n[<assembly: AssemblyFileVersion(AssemblyInfo.FileVersion)>]")]
        [TestCase("vb", "<Assembly: AssemblyVersion (AssemblyInfo.Version)>\r\n<Assembly: AssemblyInformationalVersion(AssemblyInfo.InformationalVersion)>\r\n<Assembly: AssemblyFileVersion(AssemblyInfo.FileVersion)>")]
        public void ShouldReplaceAssemblyVersionInRelativePathWithVariables(string fileExtension, string assemblyFileContent)
        {
            var assemblyInfoFile = Path.Combine("Project", "src", "Properties", "AssemblyInfo." + fileExtension);

            VerifyAssemblyInfoFile(assemblyFileContent, assemblyInfoFile, AssemblyVersioningScheme.MajorMinor,
                match: content => content.Contains(@"AssemblyVersion(""2.3.0.0"")") &&
                    content.Contains(@"AssemblyInformationalVersion(""2.3.1+3.Branch.foo.Sha.hash"")") &&
                    content.Contains(@"AssemblyFileVersion(""2.3.1.0"")"));
        }

        [Test]
        [TestCase("cs", "[assembly: AssemblyVersion (  AssemblyInfo.VersionInfo  ) ]\r\n[assembly: AssemblyInformationalVersion\t(\tAssemblyInfo.InformationalVersion\t)]\r\n[assembly: AssemblyFileVersion\r\n(\r\nAssemblyInfo.FileVersion\r\n)]")]
        [TestCase("fs", "[<assembly: AssemblyVersion ( AssemblyInfo.VersionInfo )>]\r\n[<assembly: AssemblyInformationalVersion\t(\tAssemblyInfo.InformationalVersion\t)>]\r\n[<assembly: AssemblyFileVersion\r\n(\r\nAssemblyInfo.FileVersion\r\n)>]")]
        [TestCase("vb", "<Assembly: AssemblyVersion ( AssemblyInfo.VersionInfo )>\r\n<Assembly: AssemblyInformationalVersion\t(\tAssemblyInfo.InformationalVersion\t)>\r\n<Assembly: AssemblyFileVersion\r\n(\r\nAssemblyInfo.FileVersion\r\n)>")]
        public void ShouldReplaceAssemblyVersionInRelativePathWithVariablesAndWhiteSpace(string fileExtension, string assemblyFileContent)
        {
            var assemblyInfoFile = Path.Combine("Project", "src", "Properties", "AssemblyInfo." + fileExtension);

            VerifyAssemblyInfoFile(assemblyFileContent, assemblyInfoFile, AssemblyVersioningScheme.MajorMinor,
                match: content => content.Contains(@"AssemblyVersion(""2.3.0.0"")") &&
                    content.Contains(@"AssemblyInformationalVersion(""2.3.1+3.Branch.foo.Sha.hash"")") &&
                    content.Contains(@"AssemblyFileVersion(""2.3.1.0"")"));
        }

        private static void VerifyAssemblyInfoFile(
            string assemblyFileContent,
            string assemblyInfoFile,
            AssemblyVersioningScheme versioningScheme = AssemblyVersioningScheme.MajorMinorPatch,
            Action<string> verify = null)
        {
            var workingDir = Path.GetTempPath();
            var fileName = Path.Combine(workingDir, assemblyInfoFile);

            VerifyAssemblyInfoFile(assemblyFileContent, fileName, versioningScheme, (mock, variables) =>
            {
                using (var assemblyInfoFileUpdater = new AssemblyInfoFileUpdater(assemblyInfoFile, workingDir, variables, mock.Object, false))
                {
                    assemblyInfoFileUpdater.Update();

                    verify(mock.Object.ReadAllText(fileName));
                }
            });
        }

        private static void VerifyAssemblyInfoFile(
            string assemblyFileContent,
            string assemblyInfoFile,
            AssemblyVersioningScheme versioningScheme = AssemblyVersioningScheme.MajorMinorPatch,
            Expression<Func<string, bool>> match = null)
        {
            var workingDir = Path.GetTempPath();
            var fileName = Path.Combine(workingDir, assemblyInfoFile);

            VerifyAssemblyInfoFile(assemblyFileContent, fileName, versioningScheme, (mock, variables) =>
            {
                using (var assemblyInfoFileUpdater = new AssemblyInfoFileUpdater(assemblyInfoFile, workingDir, variables, mock.Object, false))
                {
                    assemblyInfoFileUpdater.Update();

                    mock.Verify(f => f.WriteAllText(fileName, It.Is(match)),
                        Times.Once());
                }
            });
        }

        private static void VerifyAssemblyInfoFile(
            string assemblyFileContent,
            string fileName,
            AssemblyVersioningScheme versioningScheme = AssemblyVersioningScheme.MajorMinorPatch,
            Action<Mock<IFileSystem>, VersionVariables> verify = null)
        {
            var version = new SemanticVersion
            (
                major: 2,
                minor: 3,
                patch: 1,
                buildMetadata: new BuildMetadata(3, "foo", "hash", DateTimeOffset.Now)
            );

            var fileSystem = new Mock<IFileSystem>();

            fileSystem.Setup(f => f.Exists(fileName))
                .Returns(true);

            fileSystem.Setup(f => f.ReadAllText(fileName))
                .Returns(assemblyFileContent);

            fileSystem.Setup(f => f.WriteAllText(fileName, It.IsAny<string>()))
                .Callback<string, string>((name, content) =>
                {
                    fileSystem.Setup(f => f.ReadAllText(name))
                        .Returns(content);
                });

            var config = new TestEffectiveConfiguration(assemblyVersioningScheme: versioningScheme);
            var variables = version.ToVersionVariables(config);

            verify(fileSystem, variables);
        }
    }
}
