using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VCSVersion;
using VCSVersion.Configuration;
using VCSVersion.VCS;
using VCSVersion.VersionCalculation.IncrementStrategies;

namespace HgVersion.Configuration
{
    public static class BranchConfigurationCalculator
    {
        /// <summary>
        /// Gets the <see cref="BranchConfig"/> for the current commit.
        /// </summary>
        public static BranchConfig GetBranchConfiguration(HgVersionContext context, IBranchHead targetBranch,
            IList<IBranchHead> excludedInheritBranches = null)
        {
            var matchingBranches = context.FullConfiguration.GetConfigForBranch(targetBranch.Name);

            if (matchingBranches == null)
            {
                Logger.WriteInfo(
                    $"No branch configuration found for branch '{targetBranch.Name}', "
                    + "falling back to default configuration");

                matchingBranches = new BranchConfig {Name = string.Empty};
                HgConfigurationProvider.ApplyBranchDefaults(context.FullConfiguration, matchingBranches, "",
                    new List<string>());
            }

            return matchingBranches.Increment == IncrementStrategyType.Inherit
                ? InheritBranchConfiguration(context, targetBranch, matchingBranches, excludedInheritBranches)
                : matchingBranches;
        }

        static BranchConfig InheritBranchConfiguration(HgVersionContext context, IBranchHead targetBranch,
            BranchConfig branchConfiguration, IList<IBranchHead> excludedInheritBranches)
        {
            var repository = context.Repository;
            var config = context.FullConfiguration;

            using (Logger.IndentLog("Attempting to inherit branch configuration from parent branch"))
            {
                var excludedBranches = new[] {targetBranch};
                // Check if we are a merge commit. If so likely we are a pull request
                var parentCount = repository.Parents(context.CurrentCommit).Count();
                if (parentCount == 2)
                {
                    excludedBranches = CalculateWhenMultipleParents(repository, context.CurrentCommit, ref targetBranch,
                        excludedBranches);
                }

                if (excludedInheritBranches == null)
                {
                    excludedInheritBranches = repository.Branches()
                        .Where(b =>
                        {
                            var branchConfig = config.GetConfigForBranch(b.Name);

                            return branchConfig != null && branchConfig.Increment == IncrementStrategyType.Inherit;
                        })
                        .ToList();
                }

                // Add new excluded branches.
                foreach (var excludedBranch in excludedBranches.Except(excludedInheritBranches))
                {
                    excludedInheritBranches.Add(excludedBranch);
                }

                var branchesToEvaluate = repository
                    .Branches()
                    .Except(excludedInheritBranches)
                    .ToList();

                var branchPoint = context.RepositoryMetadataProvider
                    .FindCommitWasBranchedFrom(targetBranch, excludedInheritBranches.ToArray());

                var possibleParents = CalculatePossibleParents(context, branchPoint, branchesToEvaluate);

                Logger.WriteInfo("Found possible parent branches: " +
                                 string.Join(", ", possibleParents.Select(p => p.Name)));

                if (possibleParents.Count == 1)
                {
                    var branchConfig = GetBranchConfiguration(context, possibleParents[0], excludedInheritBranches);
                    return new BranchConfig(branchConfiguration)
                    {
                        Increment = branchConfig.Increment,
                        PreventIncrementOfMergedBranchVersion = branchConfig.PreventIncrementOfMergedBranchVersion,
                        // If we are inheriting from develop then we should behave like develop
                        TracksReleaseBranches = branchConfig.TracksReleaseBranches
                    };
                }

                // If we fail to inherit it is probably because the branch has been merged and we can't do much. So we will fall back to develop's config
                // if develop exists and default if not
                string errorMessage;
                if (possibleParents.Count == 0)
                    errorMessage = "Failed to inherit Increment branch configuration, no branches found.";
                else
                    errorMessage = "Failed to inherit Increment branch configuration, ended up with: " +
                                   string.Join(", ", possibleParents.Select(p => p.Name));

                var developBranchRegex = config.Branches[HgConfigurationProvider.DevelopBranchKey].Regex;
                var defaultBranchRegex = config.Branches[HgConfigurationProvider.DefaultBranchKey].Regex;

                var chosenBranch = repository.Branches()
                    .FirstOrDefault(b => 
                        Regex.IsMatch(b.Name, developBranchRegex, RegexOptions.IgnoreCase) || 
                        Regex.IsMatch(b.Name, defaultBranchRegex, RegexOptions.IgnoreCase));

                if (chosenBranch == null)
                {
                    // TODO We should call the build server to generate this exception, each build server works differently
                    // for fetch issues and we could give better warnings.
                    throw new InvalidOperationException(
                        $"Could not find a '{HgConfigurationProvider.DevelopBranchKey}' or "
                        + $"'{HgConfigurationProvider.DefaultBranchKey}' branch, neither locally nor remotely.");
                }

                var branchName = chosenBranch.Name;
                Logger.WriteWarning(errorMessage + Environment.NewLine + Environment.NewLine + "Falling back to " +
                                    branchName + " branch config");

                // To prevent infinite loops, make sure that a new branch was chosen.
                if (Equals(targetBranch, chosenBranch))
                {
                    Logger.WriteWarning(
                        "Fallback branch wants to inherit Increment branch configuration from itself. "
                        + "Using patch increment instead.");
                    
                    return new BranchConfig(branchConfiguration)
                    {
                        Increment = IncrementStrategyType.Patch
                    };
                }

                var inheritingBranchConfig = GetBranchConfiguration(context, chosenBranch, excludedInheritBranches);
                return new BranchConfig(branchConfiguration)
                {
                    Increment = inheritingBranchConfig.Increment,
                    PreventIncrementOfMergedBranchVersion =
                        inheritingBranchConfig.PreventIncrementOfMergedBranchVersion,
                    // If we are inheriting from develop then we should behave like develop
                    TracksReleaseBranches = inheritingBranchConfig.TracksReleaseBranches
                };
            }
        }

        private static List<IBranchHead> CalculatePossibleParents(HgVersionContext context, ICommit branchPoint, List<IBranchHead> branchesToEvaluate)
        {
            if (branchPoint == null)
            {
                return context.RepositoryMetadataProvider
                    .GetBranchesContainingCommit(context.CurrentCommit, branchesToEvaluate, true)
                    // It fails to inherit Increment branch configuration if more than 1 parent;
                    // therefore no point to get more than 2 parents
                    .Take(2)
                    .ToList();
            }
            
            var branches = context
                .RepositoryMetadataProvider
                .GetBranchesContainingCommit(branchPoint, branchesToEvaluate, true)
                .ToList();
            
            if (branches.Count > 1)
            {
                var currentTipBranches = context
                    .RepositoryMetadataProvider
                    .GetBranchesContainingCommit(context.CurrentCommit, branchesToEvaluate, true)
                    .ToList();

                return branches
                    .Except(currentTipBranches)
                    .ToList();
            }

            return branches;
        }

        static IBranchHead[] CalculateWhenMultipleParents(IRepository repository, ICommit currentCommit, ref IBranchHead currentBranch, IBranchHead[] excludedBranches)
        {
            var parents = repository
                .Parents(currentCommit)
                .ToArray();

            var branches = repository.Branches()
                .Where(b => Equals(b.Commit, parents[1]))
                .ToList();

            if (branches.Count == 1)
            {
                var branch = branches[0];
                excludedBranches = new[]
                {
                    currentBranch,
                    branch
                };
                currentBranch = branch;
            }
            else if (branches.Count > 1)
            {
                currentBranch = branches.FirstOrDefault(b => b.Name == HgConfigurationProvider.DefaultBranchKey) ?? branches.First();
            }
            else
            {
                var possibleTargetBranches = repository.Branches()
                    .Where(b => Equals(b.Commit, parents[0]))
                    .ToList();

                if (possibleTargetBranches.Count > 1)
                {
                    currentBranch = possibleTargetBranches.FirstOrDefault(b => b.Name == HgConfigurationProvider.DefaultBranchKey) ??
                                    possibleTargetBranches.First();
                }
                else
                {
                    currentBranch = possibleTargetBranches.FirstOrDefault() ?? currentBranch;
                }
            }

            Logger.WriteInfo("HEAD is merge commit, this is likely a pull request using " 
                             + $"{currentBranch.Name} as base");

            return excludedBranches;
        }
    }
}