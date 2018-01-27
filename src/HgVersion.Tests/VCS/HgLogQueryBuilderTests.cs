using HgVersion.VCS;
using NUnit.Framework;
using System;
using System.Threading;

namespace HgVersion.Tests.VCS
{
    [TestFixture, Parallelizable(ParallelScope.All)]
    public class HgLogQueryBuilderTests
    {
        [Test]
        public void SingleTest()
        {
            var hash = "1abfccc3cb0790837bd06eade2916867343dee33";

            var select = new HgLogQueryBuilder();
            var query = select.Single(hash);

            Assert.That(query.Revision.ToString(), Is.EqualTo(hash));
        }

        [Test]
        public void AncestorsOfTest()
        {
            var hash = "1abfccc3cb0790837bd06eade2916867343dee33";

            var select = new HgLogQueryBuilder();
            var query = select.AncestorsOf(hash);

            Assert.That(query.Revision.ToString(), Is.EqualTo($"::{hash}"));
        }

        [Test]
        public void ByBranchTest()
        {
            var branch = "default";

            var select = new HgLogQueryBuilder();
            var query = select.ByBranch(branch);

            Assert.That(query.Revision.ToString(), Is.EqualTo($"branch('{branch}')"));
        }

        [Test]
        public void CommonAncestorOfTest()
        {
            var branch1 = "default";
            var branch2 = "dev";

            var select = new HgLogQueryBuilder();
            var query = select
                .CommonAncestorOf(
                    select.ByBranch(branch1),
                    select.ByBranch(branch2));

            Assert.That(query.Revision.ToString(), Is.EqualTo($"ancestor(branch('{branch1}'), branch('{branch2}'))"));
        }

        [Test]
        public void FirstBranchCommitTest()
        {
            var branch = "default";

            var select = new HgLogQueryBuilder();
            var query = select.ByBranch(branch)
                .Limit(1);

            Assert.That(query.Revision.ToString(), Is.EqualTo($"limit(branch('{branch}'), 1)"));
        }
        
        [Test]
        public void TaggedTest()
        {
            var select = new HgLogQueryBuilder();
            var query = select.Tagged();

            Assert.That(query.Revision.ToString(), Is.EqualTo("tag()"));
        }
        
        [Test]
        public void TaggedWithPatternTest()
        {
            var select = new HgLogQueryBuilder();
            var query = select.Tagged(@"\w+");

            Assert.That(query.Revision.ToString(), Is.EqualTo(@"tag('re:\w+')"));
        }

        [Test]
        public void IntersectTest()
        {
            var branch = "default";

            var select = new HgLogQueryBuilder();
            var query = select.Intersect(
                select.ByBranch(branch),
                select.Tagged()
            );

            Assert.That(query.Revision.ToString(), Is.EqualTo($"branch('{branch}') and tag()"));
        }
    }
}
