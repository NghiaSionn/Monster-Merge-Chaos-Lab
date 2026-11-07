using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MonsterMerge.Tests
{
    /// <summary>
    /// Chụp giao diện ra Temp/ để kiểm tra bằng mắt. [Explicit] nên không chạy cùng bộ test thường.
    /// </summary>
    public sealed class MonsterMergeScreenshotTests
    {
        private const string ScenePath = "Assets/MonsterMerge/Scenes/GameScene.unity";

        // Lưu trong repo để bằng chứng hình ảnh không mất khi Unity dọn Temp/.
        private const string EvidenceDir = "production/qa/evidence/prototype-v0.2";

        private static void SetResolution(uint width, uint height)
        {
#if UNITY_EDITOR
            UnityEditor.PlayModeWindow.SetCustomRenderingResolution(width, height, "Monster Merge capture");
#endif
        }

        private static IEnumerator Capture(string fileName)
        {
            Camera camera = Camera.main;
            Canvas canvas = Object.FindAnyObjectByType<Canvas>();

            // Canvas Overlay không đi qua camera nên phải đổi tạm để lọt vào ảnh chụp.
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1f;
            yield return null;
            yield return null;

            var target = new RenderTexture(Screen.width, Screen.height, 24);
            camera.targetTexture = target;
            camera.Render();

            RenderTexture.active = target;
            var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            image.Apply();
            RenderTexture.active = null;
            camera.targetTexture = null;

            Directory.CreateDirectory(EvidenceDir);
            File.WriteAllBytes(Path.Combine(EvidenceDir, fileName), image.EncodeToJPG(80));

            Object.Destroy(image);
            target.Release();
            Object.Destroy(target);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        private static IEnumerator Stage(uint width, uint height)
        {
            SetResolution(width, height);
            PlayerPrefs.DeleteKey(GameUI.HintSeenKey);
            SceneManager.LoadScene(ScenePath);
            yield return null;
            yield return null;
            yield return null;

            var merge = Object.FindAnyObjectByType<MergeManager>();
            var tank = Object.FindAnyObjectByType<TankBounds>();

            float x = tank.InnerLeft + 0.3f;
            for (int tier = 1; tier <= 5; tier++)
            {
                MonsterData data = merge.GetTier(tier);
                merge.Spawn(data, new Vector2(x + data.radius, tank.FloorY + data.radius + 0.01f));
                x += data.radius * 2f + 0.06f;
            }

            Object.FindAnyObjectByType<AbilityManager>().AddEnergy(75);
            yield return new WaitForSeconds(0.5f);
        }

        [UnityTest, Explicit]
        public IEnumerator CaptureScreens()
        {
            int savedHint = PlayerPrefs.GetInt(GameUI.HintSeenKey, 0);
            int savedBest = PlayerPrefs.GetInt(GameManager.BestScoreKey, 0);
            int savedMagnetTip = PlayerPrefs.GetInt(AbilityManager.MagnetTipSeenKey, 0);
            PlayerPrefs.DeleteKey(AbilityManager.MagnetTipSeenKey);

            yield return Stage(1080, 1920);
            Assert.AreEqual(1920, Screen.height, "Game View chưa đổi độ phân giải.");
            yield return Capture("mm_playing_9x16.jpg");

            // TASK-02: đầy năng lượng nhưng chỉ có 1 Magni, rồi thêm Magni thứ hai trong tầm.
            var ability = Object.FindAnyObjectByType<AbilityManager>();
            var stageMerge = Object.FindAnyObjectByType<MergeManager>();
            var stageTank = Object.FindAnyObjectByType<TankBounds>();
            ability.AddEnergy(25);
            yield return null;
            Assert.AreEqual(MagnetAvailability.NeedTwoMagni, ability.Availability);
            yield return Capture("mm_magni_can_2_9x16.jpg");

            stageMerge.Spawn(stageMerge.GetTier(4), new Vector2(-1.3f, stageTank.FloorY + 1.6f));
            yield return new WaitForSeconds(1f);
            Assert.IsTrue(ability.IsReady, "Magni thứ hai phải nằm trong tầm mà không chạm Magni đầu.");
            yield return Capture("mm_magni_ready_9x16.jpg");

            Object.FindAnyObjectByType<GameManager>().EndGame();
            yield return null;
            yield return Capture("mm_gameover_9x16.jpg");

            yield return Stage(1080, 2400);
            Assert.AreEqual(2400, Screen.height, "Game View chưa đổi độ phân giải.");
            yield return Capture("mm_playing_9x20.jpg");

            SetResolution(1080, 1920);
            SceneManager.LoadScene(ScenePath);
            yield return null;
            yield return null;
            var merge = Object.FindAnyObjectByType<MergeManager>();
            var tank = Object.FindAnyObjectByType<TankBounds>();
            merge.MaxTierMode = MaxTierMergeMode.OverloadBurst;
            float y = tank.FloorY + merge.GetTier(5).radius + 0.01f;
            merge.Spawn(merge.GetTier(5), new Vector2(-0.59f, y));
            merge.Spawn(merge.GetTier(5), new Vector2(0.59f, y));
            for (float waited = 0f; merge.BurstCount == 0 && waited < 1f; waited += Time.deltaTime)
            {
                yield return null;
            }

            Assert.AreEqual(1, merge.BurstCount, "Burst phải xảy ra để chụp hiệu ứng.");
            yield return Capture("mm_overload_burst_9x16.jpg");

            PlayerPrefs.SetInt(GameUI.HintSeenKey, savedHint);
            PlayerPrefs.SetInt(GameManager.BestScoreKey, savedBest);
            PlayerPrefs.SetInt(AbilityManager.MagnetTipSeenKey, savedMagnetTip);
            PlayerPrefs.Save();
        }
    }
}
