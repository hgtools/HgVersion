using System.Text.RegularExpressions;
using HgVersion.SemanticVersions;

namespace HgVersion.VersionCalculation
{
    public sealed class PreReleaseTagCalculator : IPreReleaseTagCalculator
    {
        public PreReleaseTag CalculateTag(IVersionContext context, SemanticVersion semVersion)
        {
            var comparer = new SemanticVersionComarer(SemanticVersionComparation.MajorMinorPatch);
            // TODO: implement
//            var tagToUse = GetBranchSpecificTag(context.Configuration, context.CurrentBranch.FriendlyName, branchNameOverride);
//
//            int? number = null;
//
//            var lastTag = context.RepositoryMetadataProvider
//                .GetVersionTagsOnBranch(context.CurrentBranch, context.Configuration.GitTagPrefix)
//                .FirstOrDefault(v => v.PreReleaseTag.Name == tagToUse);
//
//            if (lastTag != null &&
//                MajorMinorPatchEqual(lastTag, semanticVersion) &&
//                lastTag.PreReleaseTag.HasTag())
//            {
//                number = lastTag.PreReleaseTag.Number + 1;
//            }
//
//            if (number == null)
//            {
//                number = 1;
//            }
            
//            return new PreReleaseTag(tagToUse, number);
            return new PreReleaseTag("alpha", 1);
        }
        
//        public static string GetBranchSpecificTag(EffectiveConfiguration configuration, string branchFriendlyName, string branchNameOverride)
//        {
//            var tagToUse = configuration.Tag;
//            if (tagToUse == "useBranchName")
//            {
//                tagToUse = "{BranchName}";
//            }
//            if (tagToUse.Contains("{BranchName}"))
//            {
//                var branchName = branchNameOverride ?? branchFriendlyName;
//                if (!string.IsNullOrWhiteSpace(configuration.BranchPrefixToTrim))
//                {
//                    branchName = Regex.Replace(branchName, configuration.BranchPrefixToTrim, string.Empty, RegexOptions.IgnoreCase);
//                }
//                branchName = Regex.Replace(branchName, "[^a-zA-Z0-9-]", "-");
//
//                tagToUse = tagToUse.Replace("{BranchName}", branchName);
//            }
//            return tagToUse;
//        }
    }
}