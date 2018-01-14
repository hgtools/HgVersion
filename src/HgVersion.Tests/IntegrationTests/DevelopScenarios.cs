using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using VCSVersion.Configuration;
using VCSVersion.VersionCalculation;

namespace HgVersion.Tests.IntegrationTests
{
    [TestFixture, Parallelizable(ParallelScope.All)]
    public class DevelopScenarios
    {
        private static IEnumerable<string> BranchNames
        {
            get
            {
                if (Encoding.Default == Encoding.GetEncoding("Windows-1251"))
                {
                    yield return "ветка";
                }
            }

        }


        [Test]
        public void WhenDevelopHasMultipleCommits_SpecifyExistingCommitId()
        {
            using (var context = new TestVesionContext())
            {
                context.CreateBranch("dev");
                context.WriteTextAndCommit("dummy.txt", "", "init commit");
                context.MakeTaggedCommit("1.0.0");

                context.MakeCommit();
                context.MakeCommit();
                context.MakeCommit();

                var thirdCommit = context.Tip(); 
                context.MakeCommit();
                context.MakeCommit();

                context.AssertFullSemver("1.1.0-alpha.3", commitId: thirdCommit.Hash);
            }
        }

        [Test]
        public void WhenTagBeforeDevelopBranched_ShouldUseTaggedCommit()
        {
            using (var context = new TestVesionContext())
            {
                context.WriteTextAndCommit("dummy.txt", "", "init commit");
                context.MakeTaggedCommit("1.0.0");
                
                context.CreateBranch("dev");
                context.MakeCommit();
                context.MakeCommit();
                
                context.AssertFullSemver("1.1.0-alpha.2");
            }
        }
        
        [Test]
        public void WhenDevelopBranchedFromTaggedCommitOnDefaultVersionDoesNotChange()
        {
            using (var context = new TestVesionContext())
            {
                context.CreateBranch("dev");
                context.WriteTextAndCommit("dummy.txt", "", "init commit");
                context.MakeTaggedCommit("1.0.0");
                
                context.AssertFullSemver("1.0.0");
            }
        }
        
        [Test]
        public void CanChangeDevelopTagViaConfig()
        {
            var config = new Config
            {
                Branches =
                {
                    ["dev"] = new BranchConfig
                    {
                        Regex = "dev",
                        Tag = "alpha",
                        SourceBranches = new List<string>()
                    }
                }
            };
            
            using (var context = new TestVesionContext())
            {
                context.CreateBranch("dev");
                context.WriteTextAndCommit("dummy.txt", "", "init commit");
                context.MakeTaggedCommit("1.0.0");
                context.MakeCommit();
                
                context.AssertFullSemver(config, "1.1.0-alpha.1");
            }
        }
        
        [Test]
        public void WhenDeveloperBranchExistsDontTreatAsDevelop()
        {
            using (var context = new TestVesionContext())
            {
                context.CreateBranch("default");
                context.MakeCommit();
                
                context.CreateBranch("developer");
                context.WriteTextAndCommit("dummy.txt", "", "init commit");
                context.MakeTaggedCommit("1.0.0");
                context.MakeCommit();
                
                context.AssertFullSemver("1.0.1-developer.1+1"); // this tag should be the branch name by default, not unstable
            }
        }
        
        [Test]
        public void WhenDevelopBranchedFromMaster_MinorIsIncreased()
        {
            using (var context = new TestVesionContext())
            {
                context.CreateBranch("dev");
                context.WriteTextAndCommit("dummy.txt", "", "init commit");
                context.MakeTaggedCommit("1.0.0");
                context.MakeCommit();
                
                context.AssertFullSemver("1.1.0-alpha.1");
            }
        }
        
//        [Test]
//        public void MergingReleaseBranchBackIntoDevelopWithMergingToMaster_DoesBumpDevelopVersion()
//        {
//            using (var fixture = new EmptyRepositoryFixture())
//            {
//                fixture.Repository.MakeATaggedCommit("1.0.0");
//                Commands.Checkout(fixture.Repository, fixture.Repository.CreateBranch("develop"));
//                fixture.Repository.MakeACommit();
//                Commands.Checkout(fixture.Repository, fixture.Repository.CreateBranch("release-2.0.0"));
//                fixture.Repository.MakeACommit();
//                Commands.Checkout(fixture.Repository, "master");
//                fixture.Repository.MergeNoFF("release-2.0.0", Generate.SignatureNow());
//
//                Commands.Checkout(fixture.Repository, "develop");
//                fixture.Repository.MergeNoFF("release-2.0.0", Generate.SignatureNow());
//                fixture.AssertFullSemver("2.1.0-alpha.2");
//            }
//        }
        
        [Test]
        public void CanHandleContinuousDelivery()
        {
            var config = new Config
            {
                Branches =
                {
                    ["develop"] = new BranchConfig
                    {
                        VersioningMode = VersioningMode.ContinuousDelivery
                    }
                }
            };
            using (var context = new TestVesionContext())
            {
                context.CreateBranch("dev");
                context.MakeCommit();
                context.MakeTaggedCommit("1.0.0");
                context.MakeCommit();
                context.MakeTaggedCommit("1.1.0-alpha7");
                
                context.AssertFullSemver(config, "1.1.0-alpha.7");
            }
        }

        [Test]
        [TestCaseSource(nameof(BranchNames))]
        public void WhenBranchNameHasRUChars_ItIsStillWorking(string branchName)
        {
            using (var context = new TestVesionContext())
            {
                context.WriteTextAndCommit("dummy.txt", "", "init commit");
                context.CreateBranch(branchName);
                context.WriteTextAndCommit("dummy.txt", "", "2nd commit");
                Assert.That(context.CurrentBranch.Name, Is.EqualTo(branchName));

            }
        }

        //        [Test]
        //        public void WhenDevelopBranchedFromMasterDetachedHead_MinorIsIncreased()
        //        {
        //            using (var fixture = new EmptyRepositoryFixture())
        //            {
        //                fixture.Repository.MakeATaggedCommit("1.0.0");
        //                Commands.Checkout(fixture.Repository, fixture.Repository.CreateBranch("develop"));
        //                fixture.Repository.MakeACommit();
        //                var commit = fixture.Repository.Head.Tip;
        //                fixture.Repository.MakeACommit();
        //                Commands.Checkout(fixture.Repository, commit);
        //                fixture.AssertFullSemver("1.1.0-alpha.1");
        //            }
        //        }

        //        [Test]
        //        public void InheritVersionFromReleaseBranch()
        //        {
        //            using (var fixture = new EmptyRepositoryFixture())
        //            {
        //                fixture.MakeATaggedCommit("1.0.0");
        //                fixture.BranchTo("develop");
        //                fixture.MakeACommit();
        //                fixture.BranchTo("release/2.0.0");
        //                fixture.MakeACommit();
        //                fixture.MakeACommit();
        //                fixture.Checkout("develop");
        //                fixture.AssertFullSemver("1.1.0-alpha.1");
        //                fixture.MakeACommit();
        //                fixture.AssertFullSemver("2.1.0-alpha.1");
        //                fixture.MergeNoFF("release/2.0.0");
        //                fixture.AssertFullSemver("2.1.0-alpha.4");
        //                fixture.BranchTo("feature/MyFeature");
        //                fixture.MakeACommit();
        //                fixture.AssertFullSemver("2.1.0-MyFeature.1+5");
        //            }
        //        }
    }
}