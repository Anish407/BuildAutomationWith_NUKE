using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    [Solution] readonly Solution Solution;
    public static int Main() => Execute<Build>(x => x.Publish);

    readonly string WebProjectPath = RootDirectory /"src"/ "NukeBuildLearn.Web";
    readonly string CoreProjectPath = RootDirectory/ "src" / "NukeBuildLearn.Core";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    AbsolutePath OutputDirectory => RootDirectory / "output";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            Log.Information($"Starting To Clean: Removing:{WebProjectPath}\bin and {CoreProjectPath}\bin");
            var files = RootDirectory
                .GlobDirectories("src/*/bin","src/*/obj","Tests/*/bin", "Tests/*/obj");

            files.ForEach(i =>
            {
                Log.Information($"Starting To Clean: Removing:{i}");
                Directory.Delete( i, true);
            });
            
            // Files are published to a directory called output
            // we need to delete the existing folder and create a new one and
            // publish files to the output folder
            if(Directory.Exists(OutputDirectory)) 
                Directory.Delete(OutputDirectory,true);
            
            Directory.CreateDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
              DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            Log.Information($"Starting To Compile: Configuration{Configuration}");
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });
    
    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetConfiguration(Configuration)
                .SetProjectFile(Solution)
                .EnableNoBuild()
                .EnableNoRestore());
        });
    Target Publish => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            Solution.AllProjects
                .Where(p => p.Name != "NukeBuildLearn.IntegrationTest" 
                            && p.Name != "NukeBuildLearn.UnitTest"
                            && p.Name != "NukeBuildLearn") // Filter out any invalid projects
                .ForEach(project =>
                {
                    DotNetPublish(s => s
                        .SetProject(project)
                        .SetConfiguration(Configuration)
                        .EnableNoBuild()
                        .SetSelfContained(false) 
                        .SetOutput(OutputDirectory / project.Name));
                });
        });
    // For packaging as a nuget 
    // Target Pack => _ => _
    //     .DependsOn(Test)
    //     .Executes(() =>
    //     {
    //         DotNetPack(s => s
    //             .SetProject(Solution)
    //             .SetConfiguration(Configuration)
    //             .EnableNoBuild()
    //             .SetOutputDirectory(OutputDirectory));
    //     });

}