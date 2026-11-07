using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MonsterMerge.Tests
{
    /// <summary>
    /// Đi qua đường input thật: Pointer, EventSystem, InputSystemUIInputModule, nút UI.
    /// InputTestFixture cô lập Input System nên kết quả không phụ thuộc cửa sổ Editor có focus hay không.
    /// </summary>
    public sealed class MonsterMergeInputTests
    {
        private const string ScenePath = "Assets/MonsterMerge/Scenes/GameScene.unity";

        private readonly InputTestFixture input = new();

        private Mouse mouse;
        private MergeManager merge;
        private DropController drop;
        private AbilityManager ability;
        private TankBounds tank;
        private Button magnetButton;
        private int savedBest;
        private int savedHint;
        private int savedMagnetTip;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            savedBest = PlayerPrefs.GetInt(GameManager.BestScoreKey, 0);
            savedHint = PlayerPrefs.GetInt(GameUI.HintSeenKey, 0);
            savedMagnetTip = PlayerPrefs.GetInt(AbilityManager.MagnetTipSeenKey, 0);

            // Phải chạy trước khi nạp scene để UI module bật action trên hệ input đã cô lập.
            input.Setup();
            mouse = InputSystem.AddDevice<Mouse>();

            SceneManager.LoadScene(ScenePath);
            yield return null;
            yield return null;

            merge = Object.FindAnyObjectByType<MergeManager>();
            drop = Object.FindAnyObjectByType<DropController>();
            ability = Object.FindAnyObjectByType<AbilityManager>();
            tank = Object.FindAnyObjectByType<TankBounds>();
            magnetButton = Object.FindObjectsByType<Button>().First(b => b.name == "MagnetButton");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            // Hủy EventSystem trước để UI module tắt action khi hệ input cô lập còn sống.
            Object.Destroy(EventSystem.current.gameObject);
            yield return null;
            input.TearDown();

            PlayerPrefs.SetInt(GameManager.BestScoreKey, savedBest);
            PlayerPrefs.SetInt(GameUI.HintSeenKey, savedHint);
            PlayerPrefs.SetInt(AbilityManager.MagnetTipSeenKey, savedMagnetTip);
            PlayerPrefs.Save();
        }

        private static Vector2 ToScreen(Vector2 world) => Camera.main.WorldToScreenPoint(world);

        private static void AssertPortraitGameView()
        {
            Assert.Less(Camera.main.aspect, 1f,
                $"Game View đang {Screen.width}x{Screen.height}. Chạy Tools/Monster Merge/Set Game View 9-16 rồi test lại.");
        }

        [UnityTest]
        public IEnumerator T19_NoClickableUiCoversPlayArea()
        {
            yield return null;
            AssertPortraitGameView();

            for (float y = tank.FloorY + 0.1f; y <= tank.SpawnY + 0.3f; y += 0.2f)
            {
                for (float x = tank.InnerLeft + 0.1f; x <= tank.InnerRight - 0.1f; x += 0.2f)
                {
                    Assert.IsFalse(PointerUtility.IsOverUi(ToScreen(new Vector2(x, y))), $"UI che vùng chơi tại ({x:F1}, {y:F1}).");
                }
            }
        }

        private Vector2 ButtonCenter()
        {
            var rect = (RectTransform)magnetButton.transform;
            return RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));
        }

        private IEnumerator MoveTo(Vector2 screen)
        {
            input.Set(mouse.position, screen);
            yield return null;
            yield return null;
        }

        private IEnumerator Press()
        {
            input.Press(mouse.leftButton);
            yield return null;
            yield return null;
        }

        private IEnumerator Release()
        {
            input.Release(mouse.leftButton);
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator T02_MouseClick_DropsExactlyOne_AtPointerX()
        {
            yield return MoveTo(ToScreen(new Vector2(0.5f, 0f)));
            yield return Press();

            Assert.IsTrue(drop.IsAiming, "Nhấn trong vùng chơi phải bắt đầu ngắm.");
            Assert.AreEqual(0, merge.ActiveMonsters.Count, "Chỉ nhấn giữ thì chưa được thả.");

            yield return Release();

            Assert.AreEqual(1, merge.ActiveMonsters.Count);
            Assert.AreEqual(0.5f, merge.ActiveMonsters[0].Position.x, 0.05f);
        }

        [UnityTest]
        public IEnumerator T02c_DragThenRelease_UsesLastPointerX_ClampedToWall()
        {
            yield return MoveTo(ToScreen(new Vector2(0f, 0f)));
            yield return Press();
            yield return MoveTo(ToScreen(new Vector2(100f, 0f)));
            yield return Release();

            Assert.AreEqual(1, merge.ActiveMonsters.Count);
            Monster monster = merge.ActiveMonsters[0];
            Assert.AreEqual(tank.InnerRight - monster.Radius, monster.Position.x, 0.01f);
        }

        [UnityTest]
        public IEnumerator V02_T05e_TouchTap_DropsExactlyOne_AtFingerX()
        {
            var touch = InputSystem.AddDevice<Touchscreen>();
            touch.MakeCurrent();
            Vector2 screen = ToScreen(new Vector2(-0.8f, 0f));

            // Xếp hàng sự kiện để Input System xử lý ở đầu frame như trên máy thật. Nếu xử lý ngay trong test,
            // Touchscreen tự cập nhật lại ở lượt kế tiếp và wasPressedThisFrame đã thành false khi Update chạy.
            input.BeginTouch(1, screen, queueEventOnly: true);
            yield return null;
            yield return null;
            Assert.IsTrue(drop.IsAiming, "Chạm trong vùng chơi phải bắt đầu ngắm.");
            Assert.AreEqual(0, merge.ActiveMonsters.Count);

            input.EndTouch(1, screen, queueEventOnly: true);
            yield return null;
            yield return null;

            Assert.AreEqual(1, merge.ActiveMonsters.Count);
            Assert.AreEqual(-0.8f, merge.ActiveMonsters[0].Position.x, 0.05f);
        }

        [UnityTest]
        public IEnumerator V02_T05f_TouchOnMagnetButton_FiresButton_DoesNotDrop()
        {
            merge.Spawn(merge.GetTier(4), new Vector2(-1f, tank.FloorY + 0.48f));
            merge.Spawn(merge.GetTier(4), new Vector2(1f, tank.FloorY + 0.48f));
            yield return new WaitForSeconds(0.3f);
            int monstersBefore = merge.ActiveMonsters.Count;
            ability.AddEnergy(100);
            yield return null;

            var touch = InputSystem.AddDevice<Touchscreen>();
            touch.MakeCurrent();
            Vector2 screen = ButtonCenter();

            input.BeginTouch(1, screen, queueEventOnly: true);
            yield return null;
            yield return null;
            Assert.IsFalse(drop.IsAiming, "Chạm lên nút không được bắt đầu ngắm.");

            input.EndTouch(1, screen, queueEventOnly: true);
            yield return null;
            yield return null;

            Assert.AreEqual(monstersBefore, merge.ActiveMonsters.Count, "Chạm nút không được thả quái.");
            Assert.IsTrue(ability.IsTargeting, "Nút phải nhận chạm qua EventSystem.");
        }

        [UnityTest]
        public IEnumerator T10_ClickOnButton_FiresButton_DoesNotDrop()
        {
            // v0.2 TASK-02: nút chỉ vào chế độ chọn khi có cặp Magni trong tầm.
            merge.Spawn(merge.GetTier(4), new Vector2(-1f, tank.FloorY + 0.48f));
            merge.Spawn(merge.GetTier(4), new Vector2(1f, tank.FloorY + 0.48f));
            yield return new WaitForSeconds(0.3f);
            int monstersBefore = merge.ActiveMonsters.Count;

            ability.AddEnergy(100);
            yield return null;

            yield return MoveTo(ButtonCenter());
            yield return Press();
            Assert.IsFalse(drop.IsAiming, "Nhấn lên UI không được bắt đầu ngắm.");
            yield return Release();

            Assert.AreEqual(monstersBefore, merge.ActiveMonsters.Count, "Bấm UI không được thả quái.");
            Assert.IsTrue(ability.IsTargeting, "Nút thật phải nhận click qua EventSystem.");
        }

        [UnityTest]
        public IEnumerator T10b_DragFromTankReleaseOnUi_CancelsDrop()
        {
            yield return MoveTo(ToScreen(Vector2.zero));
            yield return Press();
            Assert.IsTrue(drop.IsAiming, "Kịch bản chỉ có ý nghĩa nếu đang ngắm thật.");

            yield return MoveTo(ButtonCenter());
            yield return Release();

            Assert.AreEqual(0, merge.ActiveMonsters.Count);
            Assert.IsFalse(drop.IsAiming);
        }

        [UnityTest]
        public IEnumerator T13b_TapOnMagni_SelectsTarget_WithoutDropping()
        {
            Monster magni = merge.Spawn(merge.GetTier(4), new Vector2(-1f, tank.FloorY + 0.48f));
            merge.Spawn(merge.GetTier(4), new Vector2(1f, tank.FloorY + 0.48f));
            yield return new WaitForSeconds(0.3f);

            ability.AddEnergy(100);
            ability.ToggleTargeting();

            AssertPortraitGameView();

            yield return MoveTo(ToScreen(magni.Position));
            yield return Press();
            yield return Release();

            Assert.AreEqual(0, ability.Energy, "Chạm đúng Magni phải kích hoạt kỹ năng.");
            Assert.AreEqual(2, merge.ActiveMonsters.Count, "Lần chạm chọn Magni không được đồng thời thả quái.");
            Assert.IsFalse(drop.IsAiming);
        }
    }
}
