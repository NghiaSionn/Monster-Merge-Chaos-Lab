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
        [Tooltip("Đọc công tắc JuiceEnabled; bỏ trống thì coi như tắt juice.")]
        [SerializeField] private MergeEffects effects;

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
        [SerializeField, Min(0.1f)] private float tipSeconds = 3.5f;
        [SerializeField, Min(0.1f)] private float comboSeconds = 1.5f;

        [Header("Popup điểm")]
        [Tooltip("Số popup tối đa cùng lúc; merge dồn dập sẽ bỏ qua popup mới thay vì phủ kín bể.")]
        [SerializeField, Min(1)] private int maxPopups = 6;

        [Header("Combo (TASK-04)")]
        [SerializeField, Min(2)] private int comboEmphasisFrom = 3;
        [SerializeField] private Color comboColor = new(1f, 0.85f, 0.3f);
        [SerializeField] private Color comboEmphasisColor = new(1f, 0.5f, 0.15f);
        [SerializeField, Range(1f, 2f)] private float comboPunchScale = 1.35f;
        [SerializeField, Min(0.05f)] private float comboPunchSeconds = 0.2f;

        [Header("Nút nam châm")]
        [SerializeField] private Color magnetReadyColor = new(0.56f, 0.36f, 0.95f);
        [Tooltip("Đầy năng lượng nhưng chưa có mục tiêu: nút vẫn bấm được để xem lý do, nhưng không sáng.")]
        [SerializeField] private Color magnetWaitingColor = new(0.3f, 0.26f, 0.42f);

        private Coroutine messageRoutine;
        private Coroutine comboRoutine;
        private Coroutine comboPunchRoutine;
        private int activePopups;

        /// <summary>Số popup điểm đang hiển thị.</summary>
        public int ActivePopupCount => activePopups;

        public Color ComboEmphasisColor => comboEmphasisColor;

        private bool JuiceOn => effects != null && effects.JuiceEnabled;

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
            ability.TipRequested += ShowTip;
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
            ability.TipRequested -= ShowTip;
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

            bool targeting = ability.IsTargeting;
            magnetButton.interactable = ability.CanActivate || targeting;
            magnetButton.image.color = targeting || ability.IsReady ? magnetReadyColor : magnetWaitingColor;

            if (targeting)
            {
                magnetButtonText.text = "Hủy";
            }
            else if (ability.CanActivate && !ability.IsReady)
            {
                magnetButtonText.text = AbilityManager.ReasonText(ability.Availability);
            }
            else
            {
                magnetButtonText.text = "Kích hoạt Nam châm";
            }
        }

        private void OnMerged(MergeResult result)
        {
            if (result.Score > 0 && activePopups < maxPopups)
            {
                StartCoroutine(FloatScore($"+{result.Score}", result.Position));
            }

            if (result.Combo >= 2)
            {
                if (comboRoutine != null)
                {
                    StopCoroutine(comboRoutine);
                }

                bool emphasize = JuiceOn && result.Combo >= comboEmphasisFrom;
                comboText.color = emphasize ? comboEmphasisColor : comboColor;
                comboRoutine = StartCoroutine(ShowTimed(comboText, $"Combo x{result.Combo}", comboSeconds));

                if (comboPunchRoutine != null)
                {
                    StopCoroutine(comboPunchRoutine);
                }

                comboText.rectTransform.localScale = Vector3.one;
                if (emphasize)
                {
                    comboPunchRoutine = StartCoroutine(PunchCombo());
                }
            }
        }

        private void ShowMessage(string message) => ShowMessageFor(message, messageSeconds);

        private void ShowTip(string message) => ShowMessageFor(message, tipSeconds);

        private void ShowMessageFor(string message, float seconds)
        {
            if (messageRoutine != null)
            {
                StopCoroutine(messageRoutine);
            }

            messageRoutine = StartCoroutine(ShowTimed(messageText, message, seconds));
        }

        private static IEnumerator ShowTimed(Text target, string value, float seconds)
        {
            target.text = value;
            yield return new WaitForSeconds(seconds);
            target.text = string.Empty;
        }

        /// <summary>Chữ combo phóng to rồi thu về; nằm ở góc HUD nên không che bể.</summary>
        private IEnumerator PunchCombo()
        {
            RectTransform rect = comboText.rectTransform;
            for (float elapsed = 0f; elapsed < comboPunchSeconds; elapsed += Time.deltaTime)
            {
                float t = elapsed / comboPunchSeconds;
                rect.localScale = Vector3.one * Mathf.Lerp(comboPunchScale, 1f, t);
                yield return null;
            }

            rect.localScale = Vector3.one;
            comboPunchRoutine = null;
        }

        private IEnumerator FloatScore(string value, Vector2 worldPosition)
        {
            activePopups++;
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

            activePopups--;
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
