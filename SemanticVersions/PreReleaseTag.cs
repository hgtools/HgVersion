using System;
using System.Text.RegularExpressions;

namespace HgVersion.SemanticVersions
{
    /// <summary>
    /// Pre-release version tag 
    /// </summary>
    public sealed class PreReleaseTag : IFormattable, IComparable<PreReleaseTag>, IEquatable<PreReleaseTag>
    {
        private static readonly Regex PreReleaseTagPattern = 
            new Regex(@"(?<name>.*?)\.?(?<number>\d+)?$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        
        /// <summary>
        /// Tag name
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Tag number
        /// </summary>
        public int? Number { get; }

        /// <summary>
        /// Create an instance of <see cref="PreReleaseTag"/>
        /// </summary>
        public PreReleaseTag()
        { }

        /// <summary>
        /// Create an instance of <see cref="PreReleaseTag"/>
        /// </summary>
        /// <param name="name">Tag name</param>
        /// <param name="number">Tag number</param>
        public PreReleaseTag(string name, int? number = null)
        {
            Name = name;
            Number = number;
        }

        /// <summary>
        /// Create a copy of <see cref="PreReleaseTag"/>
        /// </summary>
        /// <param name="tag">Original pre-release tag</param>
        public PreReleaseTag(PreReleaseTag tag)
        {
            Name = tag.Name;
            Number = tag.Number;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is PreReleaseTag tag && Equals(tag);
        }

        /// <inheritdoc />
        public int CompareTo(PreReleaseTag other)
        {
            if (!IsNull() && other.IsNull())
                return 1;

            if (IsNull() && !other.IsNull())
                return -1;

            var nameComparison = StringComparer.InvariantCultureIgnoreCase.Compare(Name, other.Name);
            if (nameComparison != 0)
                return nameComparison;

            return Nullable.Compare(Number, other.Number);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ToString(null);
        }

        /// <summary>
        /// Default formats:
        /// <para>t - SemVer 2.0 formatted tag [beta.1]</para>
        /// </summary>
        public string ToString(string format, IFormatProvider formatProvider = null)
        {
            if (formatProvider != null)
            {
                if (formatProvider.GetFormat(GetType()) is ICustomFormatter formatter)
                    return formatter.Format(format, this, formatProvider);
            }

            if (string.IsNullOrEmpty(format))
                format = "t";

            switch (format)
            {
                case "t":
                    return Number.HasValue ? $"{Name}.{Number}" : Name;
                default:
                    throw new ArgumentException("Unknown format", nameof(format));
            }
        }

        /// <summary>
        /// Check whether this tag is empty
        /// </summary>
        public bool IsNull()
        {
            return string.IsNullOrEmpty(Name);
        }

        /// <inheritdoc />
        public bool Equals(PreReleaseTag other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && Number == other.Number;
        }
        
        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name?.GetHashCode() ?? 0) * 397) ^ Number.GetHashCode();
            }
        }
        
        public static bool operator ==(PreReleaseTag left, PreReleaseTag right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PreReleaseTag left, PreReleaseTag right)
        {
            return !Equals(left, right);
        }

        public static bool operator >(PreReleaseTag left, PreReleaseTag right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <(PreReleaseTag left, PreReleaseTag right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >=(PreReleaseTag left, PreReleaseTag right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator <=(PreReleaseTag left, PreReleaseTag right)
        {
            return StringComparer.InvariantCultureIgnoreCase.Compare(left.Name, right.Name) != 1;
        }

        public static implicit operator string(PreReleaseTag preReleaseTag)
        {
            return preReleaseTag.ToString();
        }

        public static implicit operator PreReleaseTag(string preReleaseTag)
        {
            return Parse(preReleaseTag);
        }

        /// <summary>
        /// Parse input string into <see cref="PreReleaseTag"/>
        /// </summary>
        /// <param name="preReleaseTag">Input string</param>
        /// <returns></returns>
        public static PreReleaseTag Parse(string preReleaseTag)
        {
            if (string.IsNullOrEmpty(preReleaseTag))
                return new PreReleaseTag();

            var match = PreReleaseTagPattern.Match(preReleaseTag);
            if (!match.Success)
                return new PreReleaseTag();

            var value = match.Groups["name"].Value;
            var number = match.Groups["number"].Success 
                ? int.Parse(match.Groups["number"].Value) 
                : (int?) null;
            
            if (value.EndsWith("-"))
                return new PreReleaseTag(preReleaseTag);

            return new PreReleaseTag(value, number);
        }
    }
}