using System;
using System.Text.RegularExpressions;
using HgVersion.Exceptions;

namespace HgVersion.SemanticVersions
{
    /// <summary>
    /// A semantic version implementation (https://semver.org/spec/v2.0.0.html).
    /// </summary>
    public sealed class SemanticVersion : IFormattable, IEquatable<SemanticVersion>, IComparable<SemanticVersion>
    {
        private static readonly Regex SemVerPattern = 
            new Regex(
                @"^(?<SemVer>" + 
                @"(?<Major>\d+)" + 
                @"(\.(?<Minor>\d+))" + 
                @"(\.(?<Patch>\d+))?)" + 
                @"(\.(?<FourthPart>\d+))?" + 
                @"(-(?<Tag>[^\+]*))?" + 
                @"(\+(?<BuildMetadata>.*))?$",
                RegexOptions.Compiled);
        
        /// <summary>
        /// Empty version
        /// </summary>
        public static readonly SemanticVersion Empty = new SemanticVersion();

        /// <summary>
        /// Major version part
        /// </summary>
        public int Major { get; }
        
        /// <summary>
        /// Minor version part
        /// </summary>
        public int Minor { get; }
        
        /// <summary>
        /// Patch version part
        /// </summary>
        public int Patch { get; }
        
        /// <summary>
        /// Pre-release version tag
        /// </summary>
        public PreReleaseTag PreReleaseTag { get; }

        /// <summary>
        /// Build metadata
        /// </summary>
        public BuildMetadata BuildMetadata { get; }

        /// <summary>
        /// Create an instance of <see cref="SemanticVersion"/>
        /// </summary>
        /// <param name="major">Major version part</param>
        /// <param name="minor">Minor version part</param>
        /// <param name="patch">Patch version part</param>
        public SemanticVersion(int major = 0, int minor = 0, int patch = 0)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            PreReleaseTag = new PreReleaseTag();
            BuildMetadata = new BuildMetadata();
        }
        
        /// <summary>
        /// Create an instance of <see cref="SemanticVersion"/>
        /// </summary>
        /// <param name="major">Major version part</param>
        /// <param name="minor">Minor version part</param>
        /// <param name="patch">Patch version part</param>
        /// <param name="preReleaseTag">Pre-release version tag</param>
        /// <param name="buildMetadata">Build metadata</param>
        public SemanticVersion(int major, int minor, int patch, PreReleaseTag preReleaseTag = null, BuildMetadata buildMetadata = null)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            PreReleaseTag = preReleaseTag ?? new PreReleaseTag();
            BuildMetadata = buildMetadata ?? new BuildMetadata();
        }

        /// <summary>
        /// Create a copy of <see cref="SemanticVersion"/>
        /// </summary>
        /// <param name="semanticVersion">Original version</param>
        public SemanticVersion(SemanticVersion semanticVersion)
        {
            Major = semanticVersion.Major;
            Minor = semanticVersion.Minor;
            Patch = semanticVersion.Patch;

            PreReleaseTag = new PreReleaseTag(semanticVersion.PreReleaseTag);
            BuildMetadata = new BuildMetadata(semanticVersion.BuildMetadata);
        }

        /// <summary>
        /// Check whether this version is empty
        /// </summary>
        public bool IsEmpty()
        {
            return Equals(Empty);
        }

        public static bool operator ==(SemanticVersion v1, SemanticVersion v2)
        {
            if (ReferenceEquals(v1, null))
                return ReferenceEquals(v2, null);

            return v1.Equals(v2);
        }

        public static bool operator !=(SemanticVersion v1, SemanticVersion v2)
        {
            return !(v1 == v2);
        }

        public static bool operator >(SemanticVersion v1, SemanticVersion v2)
        {
            if (v1 == null)
                throw new ArgumentNullException(nameof(v1));
            if (v2 == null)
                throw new ArgumentNullException(nameof(v2));
            return v1.CompareTo(v2) > 0;
        }

        public static bool operator >=(SemanticVersion v1, SemanticVersion v2)
        {
            if (v1 == null)
                throw new ArgumentNullException(nameof(v1));
            if (v2 == null)
                throw new ArgumentNullException(nameof(v2));
            return v1.CompareTo(v2) >= 0;
        }

        public static bool operator <=(SemanticVersion v1, SemanticVersion v2)
        {
            if (v1 == null)
                throw new ArgumentNullException(nameof(v1));
            if (v2 == null)
                throw new ArgumentNullException(nameof(v2));

            return v1.CompareTo(v2) <= 0;
        }

        public static bool operator <(SemanticVersion v1, SemanticVersion v2)
        {
            if (v1 == null)
                throw new ArgumentNullException(nameof(v1));
            if (v2 == null)
                throw new ArgumentNullException(nameof(v2));

            return v1.CompareTo(v2) < 0;
        }

        public static SemanticVersion Parse(string version, string tagPrefixRegex = null)
        {
            if (!TryParse(version, tagPrefixRegex, out var semanticVersion))
                throw new SemanticVersionExceptionException($"Failed to parse {version} into a Semantic Version");

            return semanticVersion;
        }

        public static bool TryParse(string version, string tagPrefixRegex, out SemanticVersion semanticVersion)
        {
            var match = Regex.Match(version, $"^({tagPrefixRegex})?(?<version>.*)$");

            if (!match.Success)
            {
                semanticVersion = null;
                return false;
            }
            
            version = match.Groups["version"].Value;
            var parsed = SemVerPattern.Match(version);

            if (!parsed.Success)
            {
                semanticVersion = null;
                return false;
            }

            var buildMetaData = BuildMetadata.Parse(parsed.Groups["BuildMetadata"].Value);
            var fourthPart = parsed.Groups["FourthPart"];
            
            if (fourthPart.Success && buildMetaData.CommitsSinceTag == null)
            {
                buildMetaData = new BuildMetadata(
                    commitsSinceTag: int.Parse(fourthPart.Value),
                    branch: buildMetaData.Branch,
                    commitDate: buildMetaData.CommitDate,
                    commitSha: buildMetaData.Sha,
                    otherMetadata: buildMetaData.OtherMetaData);
            }

            semanticVersion = new SemanticVersion(
                major: int.Parse(parsed.Groups["Major"].Value),
                minor: parsed.Groups["Minor"].Success ? int.Parse(parsed.Groups["Minor"].Value) : 0,
                patch: parsed.Groups["Patch"].Success ? int.Parse(parsed.Groups["Patch"].Value) : 0,
                preReleaseTag: PreReleaseTag.Parse(parsed.Groups["Tag"].Value),
                buildMetadata: buildMetaData
            );

            return true;
        }

        /// <inheritdoc />
        public int CompareTo(SemanticVersion value)
        {
            if (value == null)
            {
                return 1;
            }
            
            if (Major != value.Major)
            {
                if (Major > value.Major)
                    return 1;

                return -1;
            }
            
            if (Minor != value.Minor)
            {
                if (Minor > value.Minor)
                    return 1;

                return -1;
            }
            
            if (Patch != value.Patch)
            {
                if (Patch > value.Patch)
                    return 1;

                return -1;
            }
            
            if (PreReleaseTag != value.PreReleaseTag)
            {
                if (PreReleaseTag > value.PreReleaseTag)
                    return 1;

                return -1;
            }

            return 0;
        }
        
        /// <inheritdoc />
        public override string ToString()
        {
            return ToString(null);
        }

        /// <summary>
        /// <para>s - Default SemVer [1.2.3-beta.4+5]</para>
        /// <para>f - Full SemVer [1.2.3-beta.4+5]</para>
        /// <para>i - Informational SemVer [1.2.3-beta.4+5.Branch.master.BranchType.Master.Sha.000000]</para>
        /// <para>j - Just the SemVer part [1.2.3]</para>
        /// <para>t - SemVer with the tag [1.2.3-beta.4]</para>
        /// </summary>
        public string ToString(string format, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(format))
                format = "s";

            if (formatProvider != null)
            {
                if (formatProvider.GetFormat(GetType()) is ICustomFormatter formatter)
                    return formatter.Format(format, this, formatProvider);
            }

            switch (format)
            {
                case "j":
                    return $"{Major}.{Minor}.{Patch}";
                case "s":
                    return PreReleaseTag.IsNull() ? ToString("j"): $"{ToString("j")}-{PreReleaseTag}";
                case "t":
                    return PreReleaseTag.IsNull() ? ToString("j") : $"{ToString("j")}-{PreReleaseTag.ToString("t")}";
                case "f":
                    {
                        var buildMetadata = BuildMetadata.ToString();
                        return string.IsNullOrEmpty(buildMetadata) ? ToString("s") : $"{ToString("s")}+{buildMetadata}";
                    }
                case "i":
                    {
                        var buildMetadata = BuildMetadata.ToString("f");
                        return string.IsNullOrEmpty(buildMetadata) ? ToString("s") : $"{ToString("s")}+{buildMetadata}";
                    }
                default:
                    throw new ArgumentException($"Unrecognised format '{format}'", nameof(format));
            }
        }

        /// <inheritdoc />
        public bool Equals(SemanticVersion other)
        {
            return SemanticVersionComarer
                .Default
                .Equals(this, other);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SemanticVersion) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return SemanticVersionComarer
                .Default
                .GetHashCode(this);
        }
    }
}