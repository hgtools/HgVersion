using HgVersion.SemanticVersions;
using HgVersion.VersionCalculation.BaseVersionCalculation;

namespace HgVersion.VersionCalculation
{
    /// <inheritdoc />
    public sealed class NextVersionCalculator : IVersionCalculator
    {
        private static readonly IBaseVersionCalculator DefaultBaseVersionCalculator =
            new BaseVersionCalculator(
                new FallbackBaseVersionStrategy());

        private static readonly IMetadataCalculator DefaultMetadataCalculator =
            new MetadataCalculator();

        private static readonly IPreReleaseTagCalculator DefaultTagCalculator =
            new PreReleaseTagCalculator();

        private readonly IBaseVersionCalculator _baseVersionCalculator;
        private readonly IMetadataCalculator _metadataCalculator;
        private readonly IPreReleaseTagCalculator _tagCalculator;

        public NextVersionCalculator(
            IBaseVersionCalculator baseVersionCalculator = null,
            IMetadataCalculator metadataCalculator = null,
            IPreReleaseTagCalculator tagCalculator = null)
        {
            _baseVersionCalculator = baseVersionCalculator ?? DefaultBaseVersionCalculator;
            _metadataCalculator = metadataCalculator ?? DefaultMetadataCalculator;
            _tagCalculator = tagCalculator ?? DefaultTagCalculator;
        }

        /// <inheritdoc />
        public SemanticVersion CalculateVersion(IVersionContext context)
        {
            var baseVersion = _baseVersionCalculator.CalculateVersion(context);
            var semver = baseVersion.MaybeIncrement(context);
            var buildMetadata = _metadataCalculator.CalculateMetadata(context, baseVersion.Source);
            var preReleaseTag = _tagCalculator.CalculateTag(context, semver);

            return new SemanticVersion(
                semver.Major,
                semver.Minor,
                semver.Patch,
                preReleaseTag,
                buildMetadata);
        }
    }
}