using System.Collections.Generic;
using UnityEngine;

namespace MonsterMerge
{
    /// <summary>Đếm thời gian tràn liên tục. Không dùng trigger nên preview không bao giờ được tính.</summary>
    public sealed class DangerZone : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private MergeManager merge;
        [SerializeField] private TankBounds tank;
        [SerializeField] private SpriteRenderer lineRenderer;

        [SerializeField, Min(0.1f)] private float overflowSeconds = 2f;
        [Tooltip("Bỏ qua quái vừa thả/vừa hợp nhất trong khoảng này. Nếu không, quái đang rơi qua vạch sẽ giữ timer chạy mãi khi thả liên tục.")]
        [SerializeField, Min(0f)] private float settleGrace = 1f;
        [SerializeField] private Color safeColor = new(1f, 0.45f, 0.4f, 0.55f);
        [SerializeField] private Color alertColor = new(1f, 0.1f, 0.1f, 1f);

        [Header("Cảnh báo sắp tràn (v0.2 TASK-05)")]
        [Tooltip("Chỉ báo sắp tràn khi bộ đếm đã chạy quá khoảng này, để quái nảy qua vạch một chút không làm vạch chớp.")]
        [SerializeField, Min(0f)] private float warnAfter = 0.25f;
        [Tooltip("Vạch dày lên bao nhiêu lần khi cảnh báo.")]
        [SerializeField, Min(1f)] private float warnThickness = 2.5f;
        [SerializeField, Min(0.1f)] private float warnPulseHz = 4f;

        private Vector3 lineBaseScale = Vector3.one;

        public float Timer { get; private set; }
        public float Progress => Mathf.Clamp01(Timer / overflowSeconds);

        /// <summary>Đang thật sự sắp thua: có quái nằm trên vạch đủ lâu, không phải quái đang rơi hay nảy qua.</summary>
        public bool IsWarning => Timer >= warnAfter;

        private void Awake()
        {
            if (lineRenderer != null)
            {
                lineBaseScale = lineRenderer.transform.localScale;
            }
        }

        private void Update()
        {
            if (game.State != GameState.Playing)
            {
                return;
            }

            Timer = IsOverflowing() ? Timer + Time.deltaTime : 0f;
            UpdateLine();

            if (Timer >= overflowSeconds)
            {
                game.EndGame();
            }
        }

        private void UpdateLine()
        {
            if (lineRenderer == null)
            {
                return;
            }

            Color color = Color.Lerp(safeColor, alertColor, Progress);
            Vector3 scale = lineBaseScale;

            if (IsWarning)
            {
                float pulse = (Mathf.Sin(Time.time * 2f * Mathf.PI * warnPulseHz) + 1f) * 0.5f;
                color.a *= Mathf.Lerp(0.55f, 1f, pulse);
                scale.y *= warnThickness;
            }

            lineRenderer.color = color;
            lineRenderer.transform.localScale = scale;
        }

        private bool IsOverflowing()
        {
            IReadOnlyList<Monster> monsters = merge.ActiveMonsters;
            for (int i = 0; i < monsters.Count; i++)
            {
                Monster monster = monsters[i];
                if (monster.HasBeenDropped
                    && !monster.IsMerging
                    && Time.time - monster.DroppedAt >= settleGrace
                    && monster.TopY > tank.DangerY)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
