using System;
using System.Text.RegularExpressions;
using VCSVersion.Configuration;
using VCSVersion.SemanticVersions;
using VCSVersion.VCS;

namespace HgVersion.VCS
{
    public sealed class HgMergeMessage : IMergeMessage
    {
        private static readonly Regex parseMergeMessage = new Regex(
            @"^Merge (branch|tag) '(?<Branch>[^']*)'",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex parseGitHubPullMergeMessage = new Regex(
            @"^Merge pull request #(?<PullRequestNumber>\d*) (from|in) (?<Source>.*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex smartGitMergeMessage = new Regex(
            @"^Finish (?<Branch>.*)",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private string _mergeMessage;
        private Config _config;

        public HgMergeMessage(string mergeMessage, Config config)
        {
            _mergeMessage = mergeMessage ?? string.Empty;
            _config = config ?? new Config();

            var lastIndexOf = _mergeMessage.LastIndexOf("into", StringComparison.OrdinalIgnoreCase);
            if (lastIndexOf != -1)
            {
                // If we have into in the merge message the rest should be the target branch
                TargetBranch = _mergeMessage.Substring(lastIndexOf + 5);
            }

            MergedBranch = ParseBranch();

            // Remove remotes and branch prefixes like release/ feature/ hotfix/ etc
            var toMatch = Regex.Replace(MergedBranch, @"^(\w+[-/])*", "", RegexOptions.IgnoreCase);
            toMatch = Regex.Replace(toMatch, $"^{_config.TagPrefix}", "");
            
            // We don't match if the version is likely an ip (i.e starts with http://)
            var versionMatch = new Regex(@"^(?<!://)\d+\.\d+(\.*\d+)*");
            var version = versionMatch.Match(toMatch);

            if (version.Success)
            {
                if (SemanticVersion.TryParse(version.Value, _config.TagPrefix, out SemanticVersion val))
                {
                    Version = val;
                }
            }
        }

        private string ParseBranch()
        {
            var match = parseMergeMessage.Match(_mergeMessage);
            if (match.Success)
            {
                return match.Groups["Branch"].Value;
            }

            match = smartGitMergeMessage.Match(_mergeMessage);
            if (match.Success)
            {
                return match.Groups["Branch"].Value;
            }

            match = parseGitHubPullMergeMessage.Match(_mergeMessage);
            if (match.Success)
            {
                IsMergedPullRequest = true;
                int pullNumber;
                if (int.TryParse(match.Groups["PullRequestNumber"].Value, out pullNumber))
                {
                    PullRequestNumber = pullNumber;
                }
                var from = match.Groups["Source"].Value;
                // We could remove/separate the remote name at this point?
                return from;
            }

            return "";
        }

        public string TargetBranch { get; }
        public string MergedBranch { get; }
        public bool IsMergedPullRequest { get; private set; }
        public int? PullRequestNumber { get; private set; }
        public SemanticVersion Version { get; }
    }
}
