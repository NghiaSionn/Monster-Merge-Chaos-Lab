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

        public GameState State { get; private set; } = GameState.Playing;
        public int Score { get; private set; }
        public int BestScore { get; private set; }

        public event Action ScoreChanged;
        public event Action GameEnded;

        private void Awake()
        {
            BestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
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
