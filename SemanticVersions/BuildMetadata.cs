using System;
using System.Text.RegularExpressions;

namespace HgVersion.SemanticVersions
{
    /// <summary>
    /// Contains metadata about build provides <see cref="SemanticVersion"/>
    /// </summary>
    public sealed class BuildMetadata : IFormattable, IEquatable<BuildMetadata>
    {
        private static readonly Regex BuildMetadataPattern 
            = new Regex(
                @"(?<BuildNumber>\d+)?" + 
                @"(\.?Branch(Name)?\." + 
                @"(?<BranchName>[^\.]+))?" + 
                @"(\.?Sha?\.(?<Sha>[^\.]+))?" + 
                @"(?<Other>.*)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        public int? CommitsSinceTag { get; }
        public string Branch { get; }
        public string Sha { get; }
        public string OtherMetaData { get; }
        public DateTimeOffset CommitDate { get; }
        public int CommitsSinceVersionSource { get; }

        /// <summary>
        /// Create an instance of <see cref="BuildMetadata"/>
        /// </summary>
        public BuildMetadata()
        { }

        /// <summary>
        /// Create an instance of <see cref="BuildMetadata"/>
        /// </summary>
        public BuildMetadata(int? commitsSinceTag, string branch, string commitSha, DateTimeOffset commitDate, string otherMetadata = null)
        {
            Sha = commitSha;
            CommitsSinceTag = commitsSinceTag;
            Branch = branch;
            CommitDate = commitDate;
            OtherMetaData = otherMetadata;
            CommitsSinceVersionSource = commitsSinceTag ?? 0;
        }

        /// <summary>
        /// Create a copy of <see cref="BuildMetadata"/>
        /// </summary>
        /// <param name="buildMetadata">Original build metadata</param>
        public BuildMetadata(BuildMetadata buildMetadata)
        {
            Sha = buildMetadata.Sha;
            CommitsSinceTag = buildMetadata.CommitsSinceTag;
            Branch = buildMetadata.Branch;
            CommitDate = buildMetadata.CommitDate;
            OtherMetaData = buildMetadata.OtherMetaData;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToString(null);
        }


        /// <inheritdoc />
        public bool Equals(BuildMetadata other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return CommitsSinceTag == other.CommitsSinceTag 
                   && string.Equals(Branch, other.Branch, StringComparison.Ordinal) 
                   && string.Equals(Sha, other.Sha, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BuildMetadata) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = CommitsSinceTag.GetHashCode();
                hashCode = (hashCode * 397) ^ (Branch?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Sha?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        /// <summary>
        /// <para>b - Formats just the build number</para>
        /// <para>s - Formats the build number and the Commit Sha</para>
        /// <para>f - Formats the full build metadata</para>
        /// <para>p - Formats the padded build number. Can specify an integer for padding, default is 4. (i.e., p5)</para>
        /// </summary>
        public string ToString(string format, IFormatProvider formatProvider = null)
        {
            if (formatProvider != null)
            {
                if (formatProvider.GetFormat(GetType()) is ICustomFormatter formatter)
                    return formatter.Format(format, this, formatProvider);
            }

            if (string.IsNullOrEmpty(format))
                format = "b";

            format = format.ToLower();
            if (format.StartsWith("p", StringComparison.Ordinal))
            {
                // Handle format
                var padding = 4;
                if (format.Length > 1)
                {
                    // try to parse
                    int p;
                    if (int.TryParse(format.Substring(1), out p))
                    {
                        padding = p;
                    }
                }

                return CommitsSinceTag?.ToString("D" + padding) ?? string.Empty; 
            }
            
            return ToString(format)
                .TrimStart('.');
        }

        private string ToString(string format)
        {
            if (string.IsNullOrEmpty(format))
                format = "b";

            switch (format.ToLower())
            {
                case "b":
                    return CommitsSinceTag?.ToString();
                case "s":
                    return $"{CommitsSinceTag}" +
                           $"{(string.IsNullOrEmpty(Sha) ? null : ".Sha." + Sha)}";
                case "f":
                    return
                        $"{CommitsSinceTag}" +
                        $"{(string.IsNullOrEmpty(Branch) ? null : ".Branch." + Branch)}" +
                        $"{(string.IsNullOrEmpty(Sha) ? null : ".Sha." + Sha)}" +
                        $"{(string.IsNullOrEmpty(OtherMetaData) ? null : "." + OtherMetaData)}";
                default:
                    throw new ArgumentException("Unrecognised format", nameof(format));
            }
        }

        public static bool operator ==(BuildMetadata left, BuildMetadata right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BuildMetadata left, BuildMetadata right)
        {
            return !Equals(left, right);
        }

        public static implicit operator string(BuildMetadata preReleaseTag)
        {
            return preReleaseTag.ToString();
        }

        public static implicit operator BuildMetadata(string preReleaseTag)
        {
            return Parse(preReleaseTag);
        }

        public static BuildMetadata Parse(string buildMetadata)
        {
            var commitsSinceTag = default(int?);
            var branch = default(string);
            var sha = default(string);
            var otherMetadata = default(string);
            var date = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            
            if (string.IsNullOrEmpty(buildMetadata))
                return new BuildMetadata();

            var parsed = BuildMetadataPattern.Match(buildMetadata);

            if (parsed.Groups["BuildNumber"].Success)
                commitsSinceTag = int.Parse(parsed.Groups["BuildNumber"].Value);

            if (parsed.Groups["BranchName"].Success)
                branch = parsed.Groups["BranchName"].Value;

            if (parsed.Groups["Sha"].Success)
                sha = parsed.Groups["Sha"].Value;

            if (parsed.Groups["Other"].Success && !string.IsNullOrEmpty(parsed.Groups["Other"].Value))
                otherMetadata = parsed.Groups["Other"].Value.TrimStart('.');

            return new BuildMetadata(commitsSinceTag, branch, sha, date, otherMetadata);
        }
    }
}