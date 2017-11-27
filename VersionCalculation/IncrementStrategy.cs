using System;
using HgVersion.SemanticVersions;

namespace HgVersion.VersionCalculation
{
    public sealed class IncrementStrategy : IIncrementStrategy
    {
        private readonly VersionField _incrementField;

        public IncrementStrategy(VersionField incrementField)
        {
            _incrementField = incrementField;
        }

        public SemanticVersion IncrementVersion(SemanticVersion semver)
        {
            return semver.PreReleaseTag.IsNull() 
                ? IncrementVersion(_incrementField, semver) 
                : IncrementPreReleaseTagVersion(semver);
        }

        private SemanticVersion IncrementPreReleaseTagVersion(SemanticVersion semver)
        {
            if (semver.PreReleaseTag.Number != null)
            {
                return new SemanticVersion(
                    semver.Major,
                    semver.Minor,
                    semver.Patch,
                    new PreReleaseTag(
                        semver.PreReleaseTag.Name,
                        semver.PreReleaseTag.Number + 1),
                    semver.BuildMetadata);
            }
            
            return new SemanticVersion(semver);
        }

        private SemanticVersion IncrementVersion(VersionField incrementField, SemanticVersion semver)
        {
            switch (incrementField)
            {
                case VersionField.None:
                    return new SemanticVersion(semver);
                
                case VersionField.Major:
                    return new SemanticVersion(
                        major: semver.Major + 1,
                        minor: 0,
                        patch: 0,
                        preReleaseTag: semver.PreReleaseTag,
                        buildMetadata: semver.BuildMetadata);
                
                case VersionField.Minor:
                    return new SemanticVersion(
                        major: semver.Major,
                        minor: semver.Minor + 1,
                        patch: 0,
                        preReleaseTag: semver.PreReleaseTag,
                        buildMetadata: semver.BuildMetadata);
                    
                case VersionField.Patch:
                    return new SemanticVersion(
                        major: semver.Major,
                        minor: semver.Minor,
                        patch: semver.Patch + 1,
                        preReleaseTag: semver.PreReleaseTag,
                        buildMetadata: semver.BuildMetadata);
                    
                default:
                    throw new ArgumentException(nameof(incrementField));
            }
        }
    }
}