using System;
using HgVersion.SemanticVersions;

namespace HgVersion.AssemblyVersioning
{
    public static class AssemblyVersionsGenerator
    {
        public static string GetAssemblyVersion(this SemanticVersion version, AssemblyVersioningScheme scheme)
        {
            switch (scheme)
            {
                case AssemblyVersioningScheme.Major:
                    return $"{version.Major}.0.0.0";
                case AssemblyVersioningScheme.MajorMinor:
                    return $"{version.Major}.{version.Minor}.0.0";
                case AssemblyVersioningScheme.MajorMinorPatch:
                    return $"{version.Major}.{version.Minor}.{version.Patch}.0";
                case AssemblyVersioningScheme.MajorMinorPatchTag:
                    return $"{version.Major}.{version.Minor}.{version.Patch}.{version.PreReleaseTag.Number ?? 0}";
                case AssemblyVersioningScheme.None:
                    return null;
                default:
                    throw new ArgumentException($"Unexpected value ({scheme}).", nameof(scheme));
            }
        }

        public static string GetAssemblyFileVersion(this SemanticVersion version, AssemblyFileVersioningScheme scheme)
        {
            switch (scheme)
            {
                case AssemblyFileVersioningScheme.Major:
                    return $"{version.Major}.0.0.0";
                case AssemblyFileVersioningScheme.MajorMinor:
                    return $"{version.Major}.{version.Minor}.0.0";
                case AssemblyFileVersioningScheme.MajorMinorPatch:
                    return $"{version.Major}.{version.Minor}.{version.Patch}.0";
                case AssemblyFileVersioningScheme.MajorMinorPatchTag:
                    return $"{version.Major}.{version.Minor}.{version.Patch}.{version.PreReleaseTag.Number ?? 0}";
                case AssemblyFileVersioningScheme.None:
                    return null;
                default:
                    throw new ArgumentException($"Unexpected value ({scheme}).", nameof(scheme));
            }
        }
    }
}