using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MonsterMerge.EditorTools
{
    /// <summary>
    /// Build APK Development để test trên máy thật (v0.2 TASK-08). Chỉ gồm GameScene; kết quả ghi ra
    /// Temp/MonsterMergeBuildResult.txt. Thư mục Builds/ và file .apk nằm trong .gitignore.
    /// </summary>
    public static class MonsterMergeAndroidBuild
    {
        public const string ResultPath = "Temp/MonsterMergeBuildResult.txt";
        private const string ApkPath = "Builds/Android/MonsterMerge-v0.2-dev.apk";
        private const string ScenePath = "Assets/MonsterMerge/Scenes/GameScene.unity";

        [MenuItem("Tools/Monster Merge/Build Android Dev APK")]
        public static void Build()
        {
            File.Delete(ResultPath);
            var report = new StringBuilder();

            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                report.AppendLine("result=Blocked");
                report.AppendLine("BLOCKED: missing modules. Cài Android Build Support, OpenJDK, Android SDK & NDK Tools trong Unity Hub.");
                File.WriteAllText(ResultPath, report.ToString());
                Debug.LogError("[MonsterMerge] Chưa cài module Android.");
                return;
            }

            bool restoreBundle = EditorUserBuildSettings.buildAppBundle;
            EditorUserBuildSettings.buildAppBundle = false;

            try
            {
                var options = new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = ApkPath,
                    target = BuildTarget.Android,
                    options = BuildOptions.Development
                };

                BuildSummary summary = BuildPipeline.BuildPlayer(options).summary;
                report.AppendLine("result=" + summary.result);
                report.AppendLine("time=" + summary.totalTime);
                report.AppendLine("sizeMB=" + (summary.totalSize / 1048576.0).ToString("F1"));
                report.AppendLine("output=" + summary.outputPath);
                report.AppendLine("errors=" + summary.totalErrors + " warnings=" + summary.totalWarnings);

                Debug.Log($"[MonsterMerge] Build Android: {summary.result}, {summary.totalTime}, {summary.outputPath}");
                if (summary.result == BuildResult.Succeeded)
                {
                    EditorUtility.RevealInFinder(ApkPath);
                }
            }
            catch (System.Exception exception)
            {
                report.AppendLine("result=Exception");
                report.AppendLine(exception.ToString());
                Debug.LogException(exception);
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = restoreBundle;
                File.WriteAllText(ResultPath, report.ToString());
            }
        }
    }
}
