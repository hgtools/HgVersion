using HgVersion.VCS;
using Mercurial;
using NUnit.Framework;

namespace HgVersionTests.VCS
{
    [TestFixture, Parallelizable(ParallelScope.All)]
    public class HgLogQueryTests
    {
        [Test]
        public void LimitTest()
        {
            var branch = "dev";
            var builder = new HgLogQueryBuilder();
            var query = builder
                .ByBranch(branch)
                .Limit(2);

            Assert.That(query.Revision.ToString(), Is.EqualTo($"limit(branch('{branch}'), 2)"));
        }

        [Test]
        public void ParentsTest()
        {
            var hash = "1abfccc3cb0790837bd06eade2916867343dee33";
            var builder = new HgLogQueryBuilder();
            var query = builder.Single(hash)
                .Parents();

            Assert.That(query.Revision.ToString(), Is.EqualTo($"parents({hash})"));
        }

        [Test]
        public void ExceptTaggingCommitsTest()
        {
            var query = new HgLogQuery(RevSpec.All)
                .ExceptTaggingCommits();
            
            Assert.That(query.Revision.ToString(), Is.EqualTo("all() - (file(.hgtags) and (not tag()))"));
        }
    }
}
