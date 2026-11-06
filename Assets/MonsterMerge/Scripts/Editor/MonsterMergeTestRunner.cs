using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace MonsterMerge.EditorTools
{
    /// <summary>Chạy PlayMode test và ghi kết quả ra Temp/MonsterMergeTestResults.txt.</summary>
    public static class MonsterMergeTestRunner
    {
        public const string ResultPath = "Temp/MonsterMergeTestResults.txt";

        private sealed class Collector : ICallbacks
        {
            private readonly StringBuilder report = new();

            public void RunStarted(ITestAdaptor testsToRun)
            {
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (!result.HasChildren)
                {
                    report.AppendLine($"{result.TestStatus,-8} {result.Duration,7:F2}s  {result.Name}");
                    if (result.TestStatus == TestStatus.Failed)
                    {
                        report.AppendLine($"    {result.Message}");
                    }

                    if (!string.IsNullOrWhiteSpace(result.Output))
                    {
                        report.AppendLine($"    {result.Output.Trim()}");
                    }
                }
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                report.AppendLine($"TONG KET: pass={result.PassCount} fail={result.FailCount} skip={result.SkipCount}");
                File.WriteAllText(ResultPath, report.ToString());
                Debug.Log($"[MonsterMerge] Test xong: pass={result.PassCount} fail={result.FailCount}");
            }
        }

        [MenuItem("Tools/Monster Merge/Run Play Mode Tests")]
        public static void Run()
        {
            Run(null);
        }

        public static void Run(string[] testNames)
        {
            File.Delete(ResultPath);

            // Không tự đổi runInBackground: Test Framework đã tự bật và khôi phục cờ này.
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new Collector());
            api.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.PlayMode,
                assemblyNames = new[] { "MonsterMerge.Tests" },
                testNames = testNames
            }));
        }
    }
}
