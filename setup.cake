#load nuget:https://www.myget.org/F/cake-contrib/api/v2?package=Cake.Recipe&prerelease

Environment.SetVariableNames();

BuildParameters.SetParameters(
    context: Context,
    buildSystem: BuildSystem,
    sourceDirectoryPath: "./src",
    title: "HgVersion",
    repositoryOwner: "vCipher",
    repositoryName: "HgVersion",
    appVeyorAccountName: "vCipher",
    shouldRunCodecov: false,
    shouldRunDotNetCorePack: true,
    solutionFilePath: "./src/HgVersion.sln");

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(
    context: Context,
    dupFinderExcludePattern: new string[] {
        BuildParameters.RootDirectoryPath + "/src/*Tests/**/*.cs",
        BuildParameters.RootDirectoryPath + "/src/**/*.AssemblyInfo.cs",
    });

Build.RunDotNetCore();