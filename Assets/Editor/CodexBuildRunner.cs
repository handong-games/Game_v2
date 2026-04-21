using System;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class CodexBuildRunner
{
    public static void PerformBuild()
    {
        string[] scenes = new[]
        {
            "Assets/Scenes/TitleScene.unity",
            "Assets/Scenes/SampleScene.unity"
        };

        string location = @"C:\Users\reg24\Favorites\claude\Unity Build\Game.exe";
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = location,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        Console.WriteLine($"BUILD_RESULT:{report.summary.result}");
        Console.WriteLine($"BUILD_OUTPUT:{location}");
        Console.WriteLine($"BUILD_ERRORS:{report.summary.totalErrors}");
        Console.WriteLine($"BUILD_WARNINGS:{report.summary.totalWarnings}");
        EditorApplication.Exit(report.summary.result == BuildResult.Succeeded ? 0 : 1);
    }
}
