using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Compile);

    readonly string WebProjectPath = RootDirectory / "NukeBuildLearn.Web";
    readonly string CoreProjectPath = RootDirectory / "NukeBuildLearn.Core";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            Log.Information($"Starting To Clean: Removing:{WebProjectPath}\bin and {CoreProjectPath}\bin");
            var files = RootDirectory
                .GlobDirectories("*/Sample");

            files.ForEach(i =>
            {
                Log.Information($"Starting To Clean: Removing:{i}");
                Directory.Delete(i, true);
            });
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            Log.Information($"Starting To Compile: Configuration{Configuration}");
        });

}