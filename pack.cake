Task("NuGet-Pack")
    .IsDependentOn("DotNetCore-Build")
    .Does(() =>
{
    var projects = GetFiles(BuildParameters.SourceDirectoryPath + "/**/HgVersion.csproj");

    var settings = new DotNetCorePackSettings {
        NoBuild = true,
        Configuration = BuildParameters.Configuration,
        OutputDirectory = BuildParameters.Paths.Directories.NuGetPackages,
        ArgumentCustomization = (args) => {
            if (BuildParameters.ShouldBuildNugetSourcePackage)
            {
                args.Append("--include-source");
            }
            return args
                .Append("/p:Version={0}", BuildParameters.Version.SemVersion)
                .Append("/p:AssemblyVersion={0}", BuildParameters.Version.Version)
                .Append("/p:FileVersion={0}", BuildParameters.Version.Version)
                .Append("/p:AssemblyInformationalVersion={0}", BuildParameters.Version.InformationalVersion);
        }
    };

    foreach (var project in projects)
    {
        DotNetCorePack(project.ToString(), settings);
    }
});

BuildParameters.Tasks.PackageTask.IsDependentOn("NuGet-Pack");