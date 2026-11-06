using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MonsterMerge
{
    /// <summary>Chỉ hiển thị và chuyển tiếp nút bấm. Không chứa luật chơi.</summary>
    public sealed class GameUI : MonoBehaviour
    {
        public const string HintSeenKey = "MonsterMerge.HintSeen";

        [SerializeField] private GameManager game;
        [SerializeField] private MergeManager merge;
        [SerializeField] private DropController drop;
        [SerializeField] private AbilityManager ability;
        [SerializeField] private Camera worldCamera;

        [Header("HUD")]
        [SerializeField] private Text scoreText;
        [SerializeField] private Text bestText;
        [SerializeField] private Image nextIcon;
        [SerializeField] private Text nextTierText;
        [SerializeField] private Text comboText;
        [SerializeField] private Text messageText;
        [SerializeField] private GameObject hintPanel;
        [SerializeField] private RectTransform floatingRoot;

        [Header("Năng lượng")]
        [SerializeField] private RectTransform energyFill;
        [SerializeField] private Text energyText;
        [SerializeField] private Button magnetButton;
        [SerializeField] private Text magnetButtonText;

        [Header("Game Over")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text finalBestText;
        [SerializeField] private Button restartButton;

        [SerializeField, Min(0.1f)] private float messageSeconds = 1.6f;
        [SerializeField, Min(0.1f)] private float comboSeconds = 1.5f;

        private Coroutine messageRoutine;
        private Coroutine comboRoutine;

        private void Awake()
        {
            magnetButton.onClick.AddListener(ability.ToggleTargeting);
            restartButton.onClick.AddListener(game.Restart);
        }

        private void OnEnable()
        {
            game.ScoreChanged += RefreshScore;
            game.GameEnded += OnGameEnded;
            merge.Merged += OnMerged;
            drop.QueueChanged += RefreshNext;
            drop.Dropped += HideHint;
            ability.StateChanged += RefreshEnergy;
            ability.Feedback += ShowMessage;
        }

        private void OnDisable()
        {
            game.ScoreChanged -= RefreshScore;
            game.GameEnded -= OnGameEnded;
            merge.Merged -= OnMerged;
            drop.QueueChanged -= RefreshNext;
            drop.Dropped -= HideHint;
            ability.StateChanged -= RefreshEnergy;
            ability.Feedback -= ShowMessage;
        }

        private void Start()
        {
            gameOverPanel.SetActive(false);
            comboText.text = string.Empty;
            messageText.text = string.Empty;
            hintPanel.SetActive(PlayerPrefs.GetInt(HintSeenKey, 0) == 0);

            RefreshScore();
            RefreshNext();
            RefreshEnergy();
        }

        private void RefreshScore()
        {
            scoreText.text = $"Điểm: {game.Score}";
            bestText.text = $"Kỷ lục: {game.BestScore}";
        }

        private void RefreshNext()
        {
            if (drop.Next == null)
            {
                return;
            }

            nextIcon.color = drop.Next.color;
            nextTierText.text = drop.Next.tier.ToString();
        }

        private void RefreshEnergy()
        {
            float ratio = (float)ability.Energy / ability.MaxEnergy;
            energyFill.anchorMax = new Vector2(ratio, 1f);
            energyText.text = $"Năng lượng {Mathf.RoundToInt(ratio * 100f)}%";

            magnetButton.interactable = ability.CanActivate || ability.IsTargeting;
            magnetButtonText.text = ability.IsTargeting ? "Hủy" : "Kích hoạt Nam châm";
        }

        private void OnMerged(MergeResult result)
        {
            if (result.Score > 0)
            {
                StartCoroutine(FloatScore($"+{result.Score}", result.Position));
            }

            if (result.Combo >= 2)
            {
                if (comboRoutine != null)
                {
                    StopCoroutine(comboRoutine);
                }

                comboRoutine = StartCoroutine(ShowTimed(comboText, $"Combo x{result.Combo}", comboSeconds));
            }
        }

        private void ShowMessage(string message)
        {
            if (messageRoutine != null)
            {
                StopCoroutine(messageRoutine);
            }

            messageRoutine = StartCoroutine(ShowTimed(messageText, message, messageSeconds));
        }

        private static IEnumerator ShowTimed(Text target, string value, float seconds)
        {
            target.text = value;
            yield return new WaitForSeconds(seconds);
            target.text = string.Empty;
        }

        private IEnumerator FloatScore(string value, Vector2 worldPosition)
        {
            var item = new GameObject("ScorePopup", typeof(RectTransform));
            item.transform.SetParent(floatingRoot, false);

            var text = item.AddComponent<Text>();
            text.font = scoreText.font;
            text.fontSize = 52;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.text = value;

            Vector3 start = worldCamera.WorldToScreenPoint(worldPosition);
            float rise = Screen.height * 0.06f;
            const float Duration = 0.7f;

            for (float elapsed = 0f; elapsed < Duration; elapsed += Time.deltaTime)
            {
                float t = elapsed / Duration;
                item.transform.position = start + Vector3.up * (rise * t);
                text.color = new Color(1f, 1f, 1f, 1f - t * t);
                yield return null;
            }

            Destroy(item);
        }

        private void HideHint()
        {
            if (hintPanel.activeSelf)
            {
                hintPanel.SetActive(false);
                PlayerPrefs.SetInt(HintSeenKey, 1);
            }
        }

        private void OnGameEnded()
        {
            finalScoreText.text = $"Điểm: {game.Score}";
            finalBestText.text = $"Kỷ lục: {game.BestScore}";
            hintPanel.SetActive(false);
            gameOverPanel.SetActive(true);
            RefreshEnergy();
        }
    }
}
