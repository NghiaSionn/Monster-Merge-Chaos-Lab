using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

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

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            savedBest = PlayerPrefs.GetInt(GameManager.BestScoreKey, 0);
            savedHint = PlayerPrefs.GetInt(GameUI.HintSeenKey, 0);
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

        [UnityTest]
        public IEnumerator T12_SingleMagni_KeepsEnergy()
        {
            Monster magni = SpawnOnFloor(0f, 4);
            ability.AddEnergy(100);
            ability.ToggleTargeting();
            yield return Seconds(0.2f);

            Assert.IsFalse(ability.TrySelectAt(magni.Position));
            Assert.AreEqual(100, ability.Energy);
            Assert.IsTrue(ability.IsTargeting);
        }

        [UnityTest]
        public IEnumerator T12b_MagniOutOfRange_KeepsEnergy()
        {
            Monster magni = SpawnOnFloor(-1.7f, 4);
            SpawnOnFloor(1.7f, 4);
            ability.AddEnergy(100);
            ability.ToggleTargeting();
            yield return Seconds(0.2f);

            Assert.IsFalse(ability.TrySelectAt(magni.Position), "Khoảng cách 3.4 vượt tầm hút 2.5.");
            Assert.AreEqual(100, ability.Energy);
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
            ability.AddEnergy(100);
            ability.ToggleTargeting();

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
