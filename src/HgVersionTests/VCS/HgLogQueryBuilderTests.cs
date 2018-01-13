using HgVersion.VCS;
using NUnit.Framework;
using System;

namespace HgVersionTests.VCS
{
    [TestFixture, Parallelizable(ParallelScope.All)]
    public class HgLogQueryBuilderTests
    {
        [Test]
        public void SingleTest()
        {
            var hash = "1abfccc3cb0790837bd06eade2916867343dee33";

            var builder = new HgLogQueryBuilder();
            var query = builder.Single(hash);

            Assert.That(query.Revision.ToString(), Is.EqualTo(hash));
        }

        [Test]
        public void AncestorsOfTest()
        {
            var hash = "1abfccc3cb0790837bd06eade2916867343dee33";

            var builder = new HgLogQueryBuilder();
            var query = builder.AncestorsOf(hash);

            Assert.That(query.Revision.ToString(), Is.EqualTo($"::{hash}"));
        }

        [Test]
        public void ByBranchTest()
        {
            var branch = "default";

            var builder = new HgLogQueryBuilder();
            var query = builder.ByBranch(branch);

            Assert.That(query.Revision.ToString(), Is.EqualTo($"branch('{branch}')"));
        }

        [Test]
        public void CommonAncestorOfTest()
        {
            var branch1 = "default";
            var branch2 = "dev";

            var builder = new HgLogQueryBuilder();
            var query = builder
                .CommonAncestorOf(
                    builder.ByBranch(branch1),
                    builder.ByBranch(branch2));

            Assert.That(query.Revision.ToString(), Is.EqualTo($"ancestor(branch('{branch1}'), branch('{branch2}'))"));
        }

        [Test]
        public void FirstBranchCommitTest()
        {
            var branch = "default";

            var builder = new HgLogQueryBuilder();
            var query = builder.ByBranch(branch)
                .Limit(1);

            Assert.That(query.Revision.ToString(), Is.EqualTo($"limit(branch('{branch}'), 1)"));
        }

        [Test]
        public void TaggedBranchCommitsTest()
        {
            var branch = "default";

            var builder = new HgLogQueryBuilder();
            var query = builder.TaggedBranchCommits(branch);

            Assert.That(query.Revision.ToString(), Is.EqualTo($"branch('{branch}') and tag()"));
        }
    }
}
