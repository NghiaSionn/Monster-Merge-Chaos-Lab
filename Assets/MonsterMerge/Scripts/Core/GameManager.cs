using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonsterMerge
{
    public enum GameState
    {
        Playing,
        GameOver
    }

    /// <summary>Vòng đời ván chơi, điểm và kỷ lục.</summary>
    public sealed class GameManager : MonoBehaviour
    {
        public const string BestScoreKey = "MonsterMerge.BestScore";

        [SerializeField] private MergeManager merge;
        [Tooltip("Ghi số liệu cuối ván ra Console để so sánh A/B.")]
        [SerializeField] private bool logRoundStats = true;

        private float roundStartTime;

        public GameState State { get; private set; } = GameState.Playing;
        public int Score { get; private set; }
        public int BestScore { get; private set; }

        public event Action ScoreChanged;
        public event Action GameEnded;

        private void Awake()
        {
            BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
            roundStartTime = Time.time;
        }

        private void OnEnable()
        {
            merge.Merged += OnMerged;
        }

        private void OnDisable()
        {
            merge.Merged -= OnMerged;
        }

        private void OnMerged(MergeResult result)
        {
            Score += result.Score;
            if (Score > BestScore)
            {
                BestScore = Score;
                PlayerPrefs.SetInt(BestScoreKey, BestScore);
            }

            ScoreChanged?.Invoke();
        }

        public void EndGame()
        {
            if (State == GameState.GameOver)
            {
                return;
            }

            State = GameState.GameOver;
            PlayerPrefs.Save();

            if (logRoundStats)
            {
                Debug.Log($"[MonsterMerge] Hết ván: mode={merge.MaxTierMode}, điểm={Score}, " +
                    $"thời gian={Time.time - roundStartTime:F0}s, merge={merge.MergeCount}, burst={merge.BurstCount}");
            }

            GameEnded?.Invoke();
        }

        /// <summary>Nạp lại scene: mọi object, timer, năng lượng, combo và coroutine đều được reset sạch.</summary>
        public void Restart()
        {
            int buildIndex = SceneManager.GetActiveScene().buildIndex;
            if (buildIndex < 0)
            {
                Debug.LogError("[GameManager] GameScene chưa có trong Build Settings. Chạy Tools/Monster Merge/Build Prototype Scene.", this);
                return;
            }

            PlayerPrefs.Save();
            SceneManager.LoadScene(buildIndex);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                PlayerPrefs.Save();
            }
        }

        private void OnApplicationQuit()
        {
            PlayerPrefs.Save();
        }
    }
}
