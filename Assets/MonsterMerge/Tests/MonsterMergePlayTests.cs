using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace MonsterMerge.Tests
{
    /// <summary>
    /// Kịch bản T01–T16 trong task list, chạy trên GameScene thật.
    /// LogAssert mặc định làm test fail nếu có Debug.LogError/Exception, gồm cả MissingReferenceException.
    /// </summary>
    public sealed class MonsterMergePlayTests
    {
        private const string ScenePath = "Assets/MonsterMerge/Scenes/GameScene.unity";

        private GameManager game;
        private MergeManager merge;
        private DropController drop;
        private AbilityManager ability;
        private DangerZone danger;
        private TankBounds tank;
        private int savedBest;
        private int savedHint;
        private int savedMagnetTip;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            savedBest = PlayerPrefs.GetInt(GameManager.BestScoreKey, 0);
            savedHint = PlayerPrefs.GetInt(GameUI.HintSeenKey, 0);
            savedMagnetTip = PlayerPrefs.GetInt(AbilityManager.MagnetTipSeenKey, 0);
            PlayerPrefs.DeleteKey(GameManager.BestScoreKey);
            Time.timeScale = 1f;

            SceneManager.LoadScene(ScenePath);
            yield return null;
            yield return null;
            Resolve();
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = 1f;
            PlayerPrefs.SetInt(GameManager.BestScoreKey, savedBest);
            PlayerPrefs.SetInt(GameUI.HintSeenKey, savedHint);
            PlayerPrefs.SetInt(AbilityManager.MagnetTipSeenKey, savedMagnetTip);
            PlayerPrefs.Save();
        }

        private void Resolve()
        {
            game = Object.FindAnyObjectByType<GameManager>();
            merge = Object.FindAnyObjectByType<MergeManager>();
            drop = Object.FindAnyObjectByType<DropController>();
            ability = Object.FindAnyObjectByType<AbilityManager>();
            danger = Object.FindAnyObjectByType<DangerZone>();
            tank = Object.FindAnyObjectByType<TankBounds>();
        }

        private Vector2 OnFloor(float x, int tier)
        {
            return new Vector2(x, tank.FloorY + merge.GetTier(tier).radius + 0.01f);
        }

        private Monster SpawnOnFloor(float x, int tier)
        {
            return merge.Spawn(merge.GetTier(tier), OnFloor(x, tier));
        }

        private int CountTier(int tier) => merge.ActiveMonsters.Count(m => m.Tier == tier);

        // Hợp nhất bảo toàn tổng 2^(tier-1): nhân bản hoặc mất quái đều làm sai con số này.
        private int TotalValue() => merge.ActiveMonsters.Sum(m => 1 << (m.Tier - 1));

        private static IEnumerator Seconds(float seconds)
        {
            float end = Time.time + seconds;
            while (Time.time < end)
            {
                yield return null;
            }
        }

        private static IEnumerator FixedSteps(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }

        [UnityTest]
        public IEnumerator T01_DropAtBothWalls_StaysInsideTank()
        {
            float cooldownWait = 0.6f;
            Monster left = drop.TryDropAt(-100f);
            Assert.NotNull(left);
            Assert.GreaterOrEqual(left.Position.x, tank.InnerLeft + left.Radius - 0.001f);

            yield return Seconds(cooldownWait);

            Monster right = drop.TryDropAt(100f);
            Assert.NotNull(right);
            Assert.LessOrEqual(right.Position.x, tank.InnerRight - right.Radius + 0.001f);

            yield return Seconds(1.5f);

            foreach (Monster monster in merge.ActiveMonsters)
            {
                Assert.GreaterOrEqual(monster.Position.x, tank.InnerLeft);
                Assert.LessOrEqual(monster.Position.x, tank.InnerRight);
                Assert.GreaterOrEqual(monster.Position.y, tank.FloorY);
            }
        }

        [UnityTest]
        public IEnumerator T02b_TwentyDrops_AllInsideTank_CooldownBlocksSpam()
        {
            Time.timeScale = 4f;
            int dropped = 0;

            for (int i = 0; i < 20; i++)
            {
                float x = Mathf.Lerp(-3f, 3f, (i % 7) / 6f);
                if (drop.TryDropAt(x) != null)
                {
                    dropped++;
                    Assert.IsNull(drop.TryDropAt(x), "Cooldown phải chặn lần thả ngay sau đó.");
                }

                yield return Seconds(0.55f);
            }

            Assert.AreEqual(20, dropped);
            foreach (Monster monster in merge.ActiveMonsters)
            {
                Assert.That(monster.Position.x, Is.InRange(tank.InnerLeft, tank.InnerRight));
            }
        }

        [UnityTest]
        public IEnumerator T03_SameTierPair_MergesOnce_ScoresOnce()
        {
            SpawnOnFloor(-0.21f, 1);
            SpawnOnFloor(0.21f, 1);

            yield return Seconds(0.5f);

            Assert.AreEqual(1, merge.ActiveMonsters.Count);
            Assert.AreEqual(1, CountTier(2));
            Assert.AreEqual(merge.GetTier(2).mergeScore, game.Score);
        }

        [UnityTest]
        public IEnumerator T04_ThreeTouching_NoDuplicateNoDoubleScore()
        {
            SpawnOnFloor(-0.43f, 1);
            SpawnOnFloor(0f, 1);
            SpawnOnFloor(0.43f, 1);

            yield return Seconds(1f);

            Assert.AreEqual(3, TotalValue());
            Assert.AreEqual(1, CountTier(2));
            Assert.AreEqual(1, CountTier(1));
            Assert.AreEqual(merge.GetTier(2).mergeScore, game.Score);
        }

        [UnityTest]
        public IEnumerator T04b_ChainMerge_NewBoingLandsOnBoing_MergesToTier3()
        {
            // Cặp Blip hợp nhất trên không; Boing mới rơi xuống đúng Boing có sẵn và phải hợp nhất tiếp.
            SpawnOnFloor(0f, 2);
            float y = tank.FloorY + 1.2f;
            merge.Spawn(merge.GetTier(1), new Vector2(-0.21f, y));
            merge.Spawn(merge.GetTier(1), new Vector2(0.21f, y));

            yield return Seconds(2f);

            Assert.AreEqual(4, TotalValue());
            Assert.AreEqual(1, merge.ActiveMonsters.Count, "Boing mới sinh phải hợp nhất tiếp với Boing đang nằm dưới.");
            Assert.AreEqual(1, CountTier(3));
            Assert.AreEqual(merge.GetTier(2).mergeScore + merge.GetTier(3).mergeScore, game.Score);
            Assert.AreEqual(2, merge.Combo, "Hai merge liên tiếp trong 1.5 giây phải tính combo.");
        }

        [UnityTest]
        public IEnumerator T04c_TwoSeparatedBoings_DoNotMergeWithoutContact()
        {
            SpawnOnFloor(-1f, 2);
            SpawnOnFloor(1f, 2);

            yield return Seconds(1f);

            Assert.AreEqual(2, CountTier(2), "Cùng cấp nhưng chưa chạm thì không hợp nhất.");
        }

        [UnityTest]
        public IEnumerator T05_DifferentTiers_DoNotMerge()
        {
            SpawnOnFloor(-0.25f, 1);
            SpawnOnFloor(0.25f, 2);

            yield return Seconds(0.7f);

            Assert.AreEqual(2, merge.ActiveMonsters.Count);
            Assert.AreEqual(0, game.Score);
        }

        [UnityTest]
        public IEnumerator T06_TwoCrags_DoNotMerge()
        {
            SpawnOnFloor(-0.6f, 5);
            SpawnOnFloor(0.6f, 5);

            yield return Seconds(0.7f);

            Assert.AreEqual(2, CountTier(5));
            Assert.AreEqual(0, game.Score);
        }

        private static int CountBurstFx() => Object.FindObjectsByType<SpriteRenderer>().Count(r => r.name == "BurstFx");

        private static IEnumerator WaitUntil(System.Func<bool> condition, float timeout)
        {
            float end = Time.time + timeout;
            while (!condition() && Time.time < end)
            {
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator V02_T01a_ClassicCap_IsDefault_CragsStay()
        {
            Assert.AreEqual(MaxTierMergeMode.ClassicCap, merge.MaxTierMode, "Mặc định phải giữ hành vi v0.1.");
            int events = 0;
            merge.Merged += _ => events++;

            SpawnOnFloor(-0.59f, 5);
            SpawnOnFloor(0.59f, 5);
            yield return Seconds(0.7f);

            Assert.AreEqual(2, CountTier(5));
            Assert.AreEqual(0, events);
            Assert.AreEqual(0, merge.BurstCount);
        }

        [UnityTest]
        public IEnumerator V02_T01b_OverloadBurst_TwoCrags_RemovedAndRewardedOnce()
        {
            merge.MaxTierMode = MaxTierMergeMode.OverloadBurst;
            int events = 0;
            MergeResult last = default;
            merge.Merged += result =>
            {
                events++;
                last = result;
            };

            SpawnOnFloor(-0.59f, 5);
            SpawnOnFloor(0.59f, 5);
            yield return Seconds(0.7f);

            Assert.AreEqual(0, merge.ActiveMonsters.Count);
            Assert.AreEqual(0, Object.FindObjectsByType<Monster>().Length, "Không được còn object quái.");
            Assert.AreEqual(1, events, "Chỉ một sự kiện cho một cặp.");
            Assert.IsTrue(last.IsBurst);
            Assert.IsNull(last.Created, "Burst không được sinh cấp mới.");
            Assert.AreEqual(merge.BurstScore, game.Score);
            Assert.AreEqual(25, ability.Energy, "Năng lượng cộng như một merge thường.");
            Assert.AreEqual(1, merge.BurstCount);
            Assert.AreEqual(1, merge.MergeCount);
        }

        [UnityTest]
        public IEnumerator V02_T01c_OverloadBurst_ThreeCrags_OnlyOnePairBursts()
        {
            merge.MaxTierMode = MaxTierMergeMode.OverloadBurst;
            int events = 0;
            merge.Merged += _ => events++;

            SpawnOnFloor(-1.18f, 5);
            SpawnOnFloor(0f, 5);
            SpawnOnFloor(1.18f, 5);
            yield return Seconds(1f);

            Assert.AreEqual(1, CountTier(5), "Một cặp biến mất, một Crag ở lại.");
            Assert.AreEqual(1, events);
            Assert.AreEqual(1, merge.BurstCount);
            Assert.AreEqual(merge.BurstScore, game.Score);
        }

        [UnityTest]
        public IEnumerator V02_T01d_BurstFx_AppearsThenCleansUp()
        {
            merge.MaxTierMode = MaxTierMergeMode.OverloadBurst;
            var effects = Object.FindAnyObjectByType<MergeEffects>();

            SpawnOnFloor(-0.59f, 5);
            SpawnOnFloor(0.59f, 5);
            yield return WaitUntil(() => merge.BurstCount == 1, 1f);

            Assert.AreEqual(1, merge.BurstCount);
            Assert.AreEqual(1, CountBurstFx(), "Burst phải có hiệu ứng.");

            yield return Seconds(0.6f);
            Assert.AreEqual(0, CountBurstFx(), "Hiệu ứng phải tự hủy.");
            Assert.AreEqual(0, effects.ActiveCount);
        }

        [UnityTest]
        public IEnumerator V02_T01e_BurstFx_CappedAtMaxActive()
        {
            var effects = Object.FindAnyObjectByType<MergeEffects>();
            int played = 0;
            for (int i = 0; i < 20; i++)
            {
                if (effects.PlayBurst(new Vector2(0f, tank.FloorY + 1f)))
                {
                    played++;
                }
            }

            yield return null;
            Assert.AreEqual(8, played, "Giới hạn mặc định là 8 hiệu ứng cùng lúc.");
            Assert.AreEqual(8, CountBurstFx());

            yield return Seconds(0.6f);
            Assert.AreEqual(0, CountBurstFx());
            Assert.IsTrue(effects.PlayBurst(Vector2.zero), "Hết hiệu ứng cũ thì phải phát được tiếp.");
        }

        [UnityTest]
        public IEnumerator V02_T01f_RestartRightAfterBurst_NoLeaks()
        {
            merge.MaxTierMode = MaxTierMergeMode.OverloadBurst;
            SpawnOnFloor(-0.59f, 5);
            SpawnOnFloor(0.59f, 5);
            yield return WaitUntil(() => merge.BurstCount == 1, 1f);
            Assert.AreEqual(1, merge.BurstCount);

            game.Restart();
            yield return null;
            yield return null;
            Resolve();

            Assert.AreEqual(0, Object.FindObjectsByType<Monster>().Length);
            Assert.AreEqual(0, CountBurstFx());
            Assert.AreEqual(0, merge.BurstCount);
            Assert.AreEqual(0, game.Score);
            Assert.AreEqual(MaxTierMergeMode.ClassicCap, merge.MaxTierMode, "Restart quay về giá trị Inspector.");
        }

        private static int CountFx(string fxName) => Object.FindObjectsByType<SpriteRenderer>().Count(r => r.name == fxName);

        /// <summary>12 cặp Blip chạm nhau sẵn, xếp 3 hàng; mỗi cặp hợp nhất ngay ở bước vật lý kế tiếp.</summary>
        private void SpawnTwelveBlipPairs(float baseY)
        {
            float[] xs = { -1.65f, -0.55f, 0.55f, 1.65f };
            for (int row = 0; row < 3; row++)
            {
                float y = baseY + row * 0.7f;
                foreach (float x in xs)
                {
                    merge.Spawn(merge.GetTier(1), new Vector2(x - 0.21f, y));
                    merge.Spawn(merge.GetTier(1), new Vector2(x + 0.21f, y));
                }
            }
        }

        [UnityTest]
        public IEnumerator V02_T04a_TwentyFourMerges_FxPopupsAndSfxStayBounded()
        {
            var effects = Object.FindAnyObjectByType<MergeEffects>();
            var ui = Object.FindAnyObjectByType<GameUI>();
            Assert.IsTrue(effects.JuiceEnabled, "Mặc định phải bật juice.");
            var sfxTimes = new System.Collections.Generic.List<float>();
            effects.SfxPlayed += () => sfxTimes.Add(Time.time);
            int maxFx = 0;
            int maxPopups = 0;

            for (int batch = 0; batch < 2; batch++)
            {
                SpawnTwelveBlipPairs(tank.FloorY + 0.3f + batch * 2f);
                for (float end = Time.time + 1.2f; Time.time < end;)
                {
                    maxFx = Mathf.Max(maxFx, effects.ActiveCount);
                    maxPopups = Mathf.Max(maxPopups, ui.ActivePopupCount);
                    yield return null;
                }
            }

            TestContext.Out.WriteLine($"merge={merge.MergeCount}, sfx={sfxTimes.Count}, fx tối đa={maxFx}, popup tối đa={maxPopups}");
            Assert.GreaterOrEqual(merge.MergeCount, 24);
            Assert.LessOrEqual(maxFx, 8, "Vòng sáng không được vượt giới hạn.");
            Assert.LessOrEqual(maxPopups, 6, "Popup điểm không được vượt giới hạn.");
            Assert.GreaterOrEqual(sfxTimes.Count, 1);
            Assert.LessOrEqual(sfxTimes.Count, merge.MergeCount);
            for (int i = 1; i < sfxTimes.Count; i++)
            {
                Assert.GreaterOrEqual(sfxTimes[i] - sfxTimes[i - 1], effects.SfxMinInterval - 0.0001f, "Hai tiếng không được chồng nhau.");
            }

            yield return Seconds(1f);
            Assert.AreEqual(0, effects.ActiveCount);
            Assert.AreEqual(0, CountFx("MergeFx") + CountFx("BurstFx"), "Hiệu ứng phải tự hủy hết.");
            Assert.AreEqual(0, ui.ActivePopupCount);
        }

        [UnityTest]
        public IEnumerator V02_T04b_VisualPop_AnimatesBodyNotPhysicsRoot()
        {
            SpawnOnFloor(-0.21f, 1);
            SpawnOnFloor(0.21f, 1);
            yield return WaitUntil(() => merge.MergeCount == 1, 1f);
            Monster created = merge.ActiveMonsters[0];
            float diameter = created.Radius * 2f;
            float maxVisual = 1f;
            float maxRoot = 0f;

            for (float end = Time.time + 0.4f; Time.time < end;)
            {
                maxVisual = Mathf.Max(maxVisual, created.VisualPopScale);
                maxRoot = Mathf.Max(maxRoot, created.transform.localScale.x);
                yield return null;
            }

            Assert.Greater(maxVisual, 1.05f, "Phần hình phải nảy lên.");
            Assert.LessOrEqual(maxRoot, diameter + 0.0001f, "Gốc vật lý không được phình theo pop.");
            Assert.AreEqual(1f, created.VisualPopScale, 0.0001f, "Pop phải về đúng kích thước.");
            Assert.AreEqual(diameter, created.transform.localScale.x, 0.0001f);
            Assert.AreEqual(0.5f, created.GetComponent<CircleCollider2D>().radius, 0.0001f);
        }

        [UnityTest]
        public IEnumerator V02_T04c_JuiceDisabled_NoRingNoSfxNoPop_PopupKept()
        {
            var effects = Object.FindAnyObjectByType<MergeEffects>();
            var ui = Object.FindAnyObjectByType<GameUI>();
            effects.JuiceEnabled = false;
            int sfx = 0;
            effects.SfxPlayed += () => sfx++;
            int maxRings = 0;
            int maxPopups = 0;
            float maxVisual = 1f;

            SpawnOnFloor(-0.21f, 1);
            SpawnOnFloor(0.21f, 1);
            for (float end = Time.time + 0.5f; Time.time < end;)
            {
                maxRings = Mathf.Max(maxRings, CountFx("MergeFx"));
                maxPopups = Mathf.Max(maxPopups, ui.ActivePopupCount);
                if (merge.ActiveMonsters.Count == 1)
                {
                    maxVisual = Mathf.Max(maxVisual, merge.ActiveMonsters[0].VisualPopScale);
                }

                yield return null;
            }

            Assert.AreEqual(1, merge.MergeCount);
            Assert.AreEqual(0, maxRings);
            Assert.AreEqual(0, sfx);
            Assert.AreEqual(1f, maxVisual, 0.0001f);
            Assert.GreaterOrEqual(maxPopups, 1, "Popup điểm của v0.1 vẫn phải còn.");
        }

        [UnityTest]
        public IEnumerator V02_T04d_ComboFromThree_IsEmphasized_OnlyWithJuice()
        {
            var ui = Object.FindAnyObjectByType<GameUI>();
            Text comboText = Object.FindObjectsByType<Text>().First(t => t.name == "ComboText");

            for (int i = 0; i < 3; i++)
            {
                float x = -1.4f + i * 1.4f;
                SpawnOnFloor(x - 0.21f, 1);
                SpawnOnFloor(x + 0.21f, 1);
            }

            yield return WaitUntil(() => merge.Combo >= 3, 1f);
            yield return null;
            Assert.GreaterOrEqual(merge.Combo, 3);
            Assert.AreEqual(ui.ComboEmphasisColor, comboText.color);
            Assert.Greater(comboText.rectTransform.localScale.x, 1.01f, "Chữ combo phải phóng to.");

            game.Restart();
            yield return null;
            yield return null;
            Resolve();
            Object.FindAnyObjectByType<MergeEffects>().JuiceEnabled = false;
            ui = Object.FindAnyObjectByType<GameUI>();
            comboText = Object.FindObjectsByType<Text>().First(t => t.name == "ComboText");

            for (int i = 0; i < 3; i++)
            {
                float x = -1.4f + i * 1.4f;
                SpawnOnFloor(x - 0.21f, 1);
                SpawnOnFloor(x + 0.21f, 1);
            }

            yield return WaitUntil(() => merge.Combo >= 3, 1f);
            yield return null;
            Assert.GreaterOrEqual(merge.Combo, 3);
            Assert.AreNotEqual(ui.ComboEmphasisColor, comboText.color);
            Assert.AreEqual(1f, comboText.rectTransform.localScale.x, 0.0001f);
        }

        private static float UprightError(Transform transform) => Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, 0f));

        [UnityTest]
        public IEnumerator V02_T03a_RotatedMonster_LabelStaysUpright()
        {
            Monster monster = SpawnOnFloor(0f, 3);
            yield return FixedSteps(2);

            foreach (float angle in new[] { 90f, 180f, 270f })
            {
                monster.Body.rotation = angle;
                yield return FixedSteps(2);
                yield return new WaitForEndOfFrame();

                Assert.AreEqual(angle, monster.Body.rotation, 1f, "Thân phải giữ góc xoay vật lý.");
                Assert.Less(UprightError(monster.LabelTransform), 0.5f, $"Nhãn phải đứng thẳng khi thân xoay {angle} độ.");
            }
        }

        [UnityTest]
        public IEnumerator V02_T03b_SpinningMonster_KeepsSpinning_LabelUpright()
        {
            Monster monster = SpawnOnFloor(0f, 1);
            yield return FixedSteps(2);
            monster.Body.angularVelocity = 360f;
            float startRotation = monster.Body.rotation;

            // Đo ở cuối frame: đó là thứ người chơi thấy. Ngay sau yield null, LateUpdate chưa chạy
            // nên nhãn còn lệch đúng góc quái quay trong frame đó.
            var endOfFrame = new WaitForEndOfFrame();
            for (int i = 0; i < 10; i++)
            {
                yield return endOfFrame;
                Assert.Less(UprightError(monster.LabelTransform), 0.5f, "Nhãn phải đứng thẳng ở mọi frame.");
            }

            Assert.Greater(Mathf.Abs(monster.Body.rotation - startRotation), 5f, "Giữ nhãn đứng không được cản quái lăn.");
        }

        [UnityTest]
        public IEnumerator V02_T03c_MergedMonster_LabelUprightAndNoLeftoverLabels()
        {
            Monster left = SpawnOnFloor(-0.21f, 1);
            Monster right = SpawnOnFloor(0.21f, 1);
            left.Body.angularVelocity = 400f;
            right.Body.angularVelocity = -400f;
            yield return Seconds(0.5f);

            Assert.AreEqual(1, CountTier(2));
            Assert.Less(UprightError(merge.ActiveMonsters[0].LabelTransform), 0.5f, "Quái mới sinh phải có nhãn đứng ngay.");

            game.EndGame();
            game.Restart();
            yield return null;
            yield return null;
            Resolve();

            Assert.AreEqual(1, Object.FindObjectsByType<TextMesh>().Length, "Sau Restart chỉ còn nhãn của preview.");
        }

        [UnityTest]
        public IEnumerator T07_BriefCrossingOfDangerLine_NoGameOver()
        {
            Monster boing = merge.Spawn(merge.GetTier(2), new Vector2(0f, tank.DangerY + 1f));
            boing.Body.linearVelocity = Vector2.up * 3f;

            yield return Seconds(3f);

            Assert.AreEqual(GameState.Playing, game.State);
            Assert.AreEqual(0f, danger.Timer);
        }

        [UnityTest]
        public IEnumerator T07b_ContinuousDropSpam_FallingMonstersDoNotCountAsOverflow()
        {
            for (int i = 0; i < 12; i++)
            {
                drop.TryDropAt(Mathf.Lerp(-1.8f, 1.8f, i / 11f));
                yield return Seconds(0.52f);
            }

            Assert.AreEqual(GameState.Playing, game.State);
        }

        [UnityTest]
        public IEnumerator T08_RealOverflow_GameOverExactlyOnce()
        {
            int endings = 0;
            game.GameEnded += () => endings++;

            Monster stuck = merge.Spawn(merge.GetTier(3), new Vector2(0f, tank.DangerY + 0.5f));
            stuck.Body.bodyType = RigidbodyType2D.Kinematic;

            yield return Seconds(2.6f);
            Assert.AreEqual(GameState.Playing, game.State, "Chưa đủ grace + 2 giây thì chưa được thua.");

            yield return Seconds(1f);
            Assert.AreEqual(GameState.GameOver, game.State);
            Assert.AreEqual(1, endings);
            Assert.IsNull(drop.TryDropAt(0f), "Hết ván thì không được thả.");

            yield return Seconds(0.5f);
            Assert.AreEqual(1, endings);
        }

        [UnityTest]
        public IEnumerator T09_RestartThreeTimes_CleanStateAndKeepsBest()
        {
            SpawnOnFloor(-0.21f, 1);
            SpawnOnFloor(0.21f, 1);
            yield return Seconds(0.5f);
            int best = game.BestScore;
            Assert.Greater(best, 0);

            for (int i = 0; i < 3; i++)
            {
                SpawnOnFloor(1f, 3);
                ability.AddEnergy(50);
                game.EndGame();
                game.Restart();
                yield return null;
                yield return null;
                Resolve();

                Assert.AreEqual(GameState.Playing, game.State);
                Assert.AreEqual(0, game.Score);
                Assert.AreEqual(best, game.BestScore);
                Assert.AreEqual(0, ability.Energy);
                Assert.AreEqual(0f, danger.Timer);
                Assert.AreEqual(0, merge.ActiveMonsters.Count);
                Assert.AreEqual(0, Object.FindObjectsByType<Monster>().Length);
            }
        }

        [UnityTest]
        public IEnumerator T11_FourMerges_EnergyFullAndCapped()
        {
            for (int i = 0; i < 5; i++)
            {
                float x = -1.6f + i * 0.8f;
                SpawnOnFloor(x - 0.21f, 1);
                SpawnOnFloor(x + 0.21f, 1);
                yield return Seconds(0.3f);

                Assert.AreEqual(Mathf.Min(100, (i + 1) * 25), ability.Energy);
            }

            Assert.IsTrue(ability.CanActivate);
        }

        /// <summary>Hai Magni nằm sàn, cách nhau 2.0: trong tầm hút 2.5 nhưng chưa chạm nhau.</summary>
        private IEnumerator SpawnMagniPair()
        {
            Monster left = SpawnOnFloor(-1f, 4);
            SpawnOnFloor(1f, 4);
            yield return Seconds(0.3f);
            Assert.AreEqual(2, CountTier(4), "Cặp Magni phải còn nguyên để làm mục tiêu.");
            pairLeft = left;
        }

        private Monster pairLeft;

        private Button MagnetButton() => Object.FindObjectsByType<Button>().First(b => b.name == "MagnetButton");

        private string MagnetLabel() => MagnetButton().GetComponentInChildren<Text>().text;

        // v0.2 TASK-02: không có mục tiêu thì nút chỉ báo lý do, không vào chế độ chọn.
        [UnityTest]
        public IEnumerator T12_SingleMagni_KeepsEnergy()
        {
            Monster magni = SpawnOnFloor(0f, 4);
            ability.AddEnergy(100);
            yield return Seconds(0.2f);
            string feedback = null;
            ability.Feedback += message => feedback = message;

            ability.ToggleTargeting();

            Assert.IsFalse(ability.IsTargeting, "Không có mục tiêu thì không vào chế độ chọn.");
            Assert.AreEqual(MagnetAvailability.NeedTwoMagni, ability.Availability);
            Assert.AreEqual("Cần 2 Magni", feedback);
            Assert.IsFalse(ability.TrySelectAt(magni.Position));
            Assert.AreEqual(100, ability.Energy);
            Assert.AreEqual(0f, magni.HighlightAmount, "Magni không có cặp không được sáng.");
        }

        [UnityTest]
        public IEnumerator T12b_MagniOutOfRange_KeepsEnergy()
        {
            Monster magni = SpawnOnFloor(-1.7f, 4);
            SpawnOnFloor(1.7f, 4);
            ability.AddEnergy(100);
            yield return Seconds(0.2f);
            string feedback = null;
            ability.Feedback += message => feedback = message;

            ability.ToggleTargeting();

            Assert.IsFalse(ability.IsTargeting);
            Assert.AreEqual(MagnetAvailability.NoPairInRange, ability.Availability, "Khoảng cách 3.4 vượt tầm hút 2.5.");
            Assert.AreEqual("Không có cặp trong tầm", feedback);
            Assert.IsFalse(ability.TrySelectAt(magni.Position));
            Assert.AreEqual(100, ability.Energy);
            Assert.AreEqual(0f, magni.HighlightAmount);
        }

        [UnityTest]
        public IEnumerator V02_T02a_ValidPair_HighlightsOnlyWhenEnergyFull()
        {
            yield return SpawnMagniPair();
            ability.AddEnergy(75);
            yield return null;

            Assert.AreEqual(MagnetAvailability.NotEnoughEnergy, ability.Availability);
            Assert.AreEqual(0f, pairLeft.HighlightAmount, "Chưa đủ năng lượng thì không được trông như dùng được.");
            Assert.IsFalse(MagnetButton().interactable);

            ability.AddEnergy(25);
            yield return null;

            Assert.AreEqual(MagnetAvailability.Ready, ability.Availability);
            Assert.IsTrue(ability.IsReady);
            Assert.Greater(pairLeft.HighlightAmount, 0f);
            Assert.IsTrue(MagnetButton().interactable);
            Assert.AreEqual("Kích hoạt Nam châm", MagnetLabel());
        }

        [UnityTest]
        public IEnumerator V02_T02b_NoTarget_ButtonShowsReason()
        {
            SpawnOnFloor(0f, 4);
            ability.AddEnergy(100);
            yield return null;
            Assert.AreEqual("Cần 2 Magni", MagnetLabel());
            Assert.IsFalse(ability.IsReady);

            SpawnOnFloor(-1.7f, 4);
            SpawnOnFloor(1.7f, 4);
            yield return Seconds(0.3f);
            Assert.AreEqual(3, CountTier(4));
            Assert.AreEqual(MagnetAvailability.Ready, ability.Availability, "Magni ở giữa cách hai bên 1.7, trong tầm.");
        }

        [UnityTest]
        public IEnumerator V02_T02c_Tip_ShownOnlyOnceAcrossRestart()
        {
            PlayerPrefs.DeleteKey(AbilityManager.MagnetTipSeenKey);
            int tips = 0;
            ability.TipRequested += _ => tips++;

            yield return SpawnMagniPair();
            ability.AddEnergy(100);
            yield return null;
            Assert.AreEqual(1, tips);

            ability.ToggleTargeting();
            ability.ToggleTargeting();
            yield return null;
            Assert.AreEqual(1, tips, "Vào rồi hủy chọn không được hiện lại hướng dẫn.");

            game.Restart();
            yield return null;
            yield return null;
            Resolve();
            ability.TipRequested += _ => tips++;

            yield return SpawnMagniPair();
            ability.AddEnergy(100);
            yield return null;
            Assert.IsTrue(ability.IsReady);
            Assert.AreEqual(1, tips, "Đã thấy thì không hiện lại ở ván sau.");
        }

        [UnityTest]
        public IEnumerator V02_T02d_Stats_CountActivationsSuccessesAndFailures()
        {
            yield return SpawnMagniPair();
            ability.AddEnergy(100);
            yield return null;

            ability.ToggleTargeting();
            Assert.IsFalse(ability.TrySelectAt(new Vector2(0f, tank.SpawnY)), "Chạm chỗ trống không được tính là hút.");
            Assert.IsTrue(ability.TrySelectAt(pairLeft.Position));

            Assert.AreEqual(1, ability.ActivateCount);
            Assert.AreEqual(1, ability.FailedAttemptCount);
            Assert.AreEqual(1, ability.SuccessCount);
            Assert.GreaterOrEqual(ability.EnergyFullAt, 0f);
            Assert.GreaterOrEqual(ability.FirstPairAt, 0f);
            Assert.AreEqual(0, ability.Energy);
        }

        [UnityTest]
        public IEnumerator V02_T02e_GameOver_ClearsHighlightAndButton()
        {
            yield return SpawnMagniPair();
            ability.AddEnergy(100);
            yield return null;
            Assert.Greater(pairLeft.HighlightAmount, 0f);

            game.EndGame();
            yield return null;

            Assert.AreEqual(0f, pairLeft.HighlightAmount);
            Assert.IsFalse(MagnetButton().interactable);
            Assert.IsFalse(ability.IsReady);
        }

        [UnityTest]
        public IEnumerator T13_TwoMagni_PullMergesThroughMergeManager()
        {
            Monster magni = SpawnOnFloor(-1f, 4);
            SpawnOnFloor(1f, 4);
            yield return Seconds(0.3f);
            Assert.AreEqual(2, CountTier(4));

            ability.AddEnergy(100);
            ability.ToggleTargeting();
            Assert.IsFalse(drop.CanDrop, "Đang chọn mục tiêu thì không được thả.");

            Assert.IsTrue(ability.TrySelectAt(magni.Position));
            Assert.AreEqual(0, ability.Energy);
            Assert.IsFalse(ability.IsTargeting);

            yield return Seconds(1.5f);

            Assert.AreEqual(0, CountTier(4));
            Assert.AreEqual(1, CountTier(5));
            Assert.AreEqual(merge.GetTier(5).mergeScore, game.Score);
            Assert.AreEqual(25, ability.Energy, "Bản thân cú merge vẫn cộng năng lượng như bình thường.");
        }

        [UnityTest]
        public IEnumerator T14_CancelTargeting_KeepsEnergy_DropWorks()
        {
            yield return SpawnMagniPair();
            ability.AddEnergy(100);
            ability.ToggleTargeting();
            Assert.IsTrue(ability.IsTargeting);
            Assert.IsNull(drop.TryDropAt(0f));

            ability.ToggleTargeting();
            yield return null;

            Assert.IsFalse(ability.IsTargeting);
            Assert.AreEqual(100, ability.Energy);
            Assert.NotNull(drop.TryDropAt(0f));
        }

        [UnityTest]
        public IEnumerator T15_RestartDuringPull_NoErrors()
        {
            Monster magni = SpawnOnFloor(-1f, 4);
            SpawnOnFloor(1f, 4);
            yield return Seconds(0.3f);

            ability.AddEnergy(100);
            ability.ToggleTargeting();
            Assert.IsTrue(ability.TrySelectAt(magni.Position));
            yield return FixedSteps(5);
            Assert.IsTrue(ability.IsPulling);

            game.Restart();
            yield return Seconds(1f);
            Resolve();

            Assert.AreEqual(0, merge.ActiveMonsters.Count);
            Assert.IsFalse(ability.IsPulling);
        }

        [UnityTest]
        public IEnumerator T16_GameOverDuringTargeting_BlocksEverything()
        {
            yield return SpawnMagniPair();
            ability.AddEnergy(100);
            ability.ToggleTargeting();
            Assert.IsTrue(ability.IsTargeting, "Phải đang chọn mục tiêu thì kịch bản mới có ý nghĩa.");

            game.EndGame();
            yield return null;

            Assert.IsFalse(ability.IsTargeting);
            Assert.IsFalse(ability.CanActivate);
            Assert.IsFalse(drop.CanDrop);

            ability.ToggleTargeting();
            Assert.IsFalse(ability.IsTargeting);
            Assert.IsNull(drop.TryDropAt(0f));
        }

        [UnityTest]
        public IEnumerator P1_1_SameDrop_BoingReboundsClearly_CragBarely()
        {
            const float DropHeight = 1.5f;
            int[] tiers = { 1, 2, 5 };
            float[] xs = { -1.5f, 0f, 1.5f };
            var monsters = new Monster[3];
            var peaks = new float[3];
            var landed = new bool[3];

            for (int i = 0; i < 3; i++)
            {
                MonsterData data = merge.GetTier(tiers[i]);
                monsters[i] = merge.Spawn(data, new Vector2(xs[i], tank.FloorY + data.radius + DropHeight));
            }

            for (float elapsed = 0f; elapsed < 1.5f; elapsed += Time.fixedDeltaTime)
            {
                yield return new WaitForFixedUpdate();
                for (int i = 0; i < 3; i++)
                {
                    landed[i] |= monsters[i].Body.linearVelocity.y > 0.01f;
                    if (landed[i])
                    {
                        float height = monsters[i].Position.y - (tank.FloorY + monsters[i].Radius);
                        peaks[i] = Mathf.Max(peaks[i], height);
                    }
                }
            }

            TestContext.Out.WriteLine($"Độ nảy lại từ {DropHeight}m: Blip={peaks[0]:F3} Boing={peaks[1]:F3} Crag={peaks[2]:F3}");
            Assert.Greater(peaks[1], 0.3f, "Boing phải nảy rõ.");
            Assert.Less(peaks[1], DropHeight, "Boing không được nảy cao hơn điểm thả.");
            Assert.Less(peaks[2], 0.05f, "Crag phải gần như không nảy.");
            Assert.Greater(peaks[1], peaks[0] * 3f, "Boing phải khác biệt rõ với Blip.");
        }

        private IEnumerator MeasureRoll(int tier, float[] result, int index)
        {
            const float Speed = 3f;
            Monster monster = SpawnOnFloor(-1.9f, tier);
            yield return FixedSteps(3);

            monster.Body.linearVelocity = new Vector2(Speed, 0f);
            float start = monster.Position.x;
            yield return Seconds(0.8f);

            result[index] = monster.Position.x - start;
        }

        [UnityTest]
        public IEnumerator P1_1_SamePush_GooRollsMuchLessThanBlip()
        {
            var distances = new float[2];
            yield return MeasureRoll(1, distances, 0);

            game.Restart();
            yield return null;
            yield return null;
            Resolve();

            yield return MeasureRoll(3, distances, 1);

            TestContext.Out.WriteLine($"Quãng lăn 0.8s với v=3: Blip={distances[0]:F2} Goo={distances[1]:F2}");
            Assert.Less(distances[1], distances[0] * 0.7f, "Goo phải khó lăn hơn hẳn Blip (GDD mục 3).");
        }

        [UnityTest]
        public IEnumerator Soak_RandomBot_FullGames_NoErrors()
        {
            // Bot ngẫu nhiên chơi hết ván ở tốc độ cao; bắt lỗi trạng thái hiếm và đo độ dài ván.
            Time.timeScale = 6f;
            Random.InitState(12345);

            for (int round = 0; round < 3; round++)
            {
                float started = Time.time;
                int drops = 0;

                while (game.State == GameState.Playing && Time.time - started < 600f)
                {
                    if (drop.TryDropAt(Random.Range(tank.InnerLeft, tank.InnerRight)) != null)
                    {
                        drops++;
                    }

                    foreach (Monster monster in merge.ActiveMonsters)
                    {
                        Assert.That(monster.Position.x, Is.InRange(tank.InnerLeft - 0.05f, tank.InnerRight + 0.05f), "Quái thoát khỏi bể.");
                        Assert.Greater(monster.Position.y, tank.FloorY - 0.05f, "Quái xuyên đáy.");
                    }

                    yield return null;
                }

                TestContext.Out.WriteLine(
                    $"Ván {round + 1}: {Time.time - started:F0}s game-time, {drops} lần thả, điểm {game.Score}, " +
                    $"cấp cao nhất {merge.ActiveMonsters.Max(m => m.Tier)}, còn {merge.ActiveMonsters.Count} quái, kết thúc={game.State}");

                game.EndGame();
                game.Restart();
                yield return null;
                yield return null;
                Resolve();
            }
        }
    }
}
