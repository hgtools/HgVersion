﻿using System.IO;
using Mercurial;
using NUnit.Framework;
using VCSVersion;
using VCSVersion.SemanticVersions;

namespace HgVersionTests
{
    [TestFixture, Parallelizable(ParallelScope.All)]
    public class VersionFinderTests
    {
        [Test]
        public void FindVersion_NotInitedRepositoryThrowException()
        {
            using (var context = new TestVesionContext(inited: false))
            {
                var finder = new VersionFinder();
                Assert.Throws<MercurialExecutionException>(() => finder.FindVersion(context));
            }
        }
        
        [Test]
        public void FindVersion_WithFirstNonTaggedCommit()
        {
            using (var context = new TestVesionContext())
            {
                context.WriteTextAndCommit("test.txt", "dummy", "init commit");

                var finder = new VersionFinder();
                var version = finder.FindVersion(context);
                var expected = SemanticVersion.Parse("0.1.0");
                var comparer = new SemanticVersionComparer(SemanticVersionComparation.MajorMinorPatch);
                
                Assert.That(version, Is.EqualTo(expected).Using(comparer));
            }
        }
    }
}
