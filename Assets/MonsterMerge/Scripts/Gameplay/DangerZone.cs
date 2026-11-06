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

        public float Timer { get; private set; }
        public float Progress => Mathf.Clamp01(Timer / overflowSeconds);

        private void Update()
        {
            if (game.State != GameState.Playing)
            {
                return;
            }

            Timer = IsOverflowing() ? Timer + Time.deltaTime : 0f;

            if (lineRenderer != null)
            {
                lineRenderer.color = Color.Lerp(safeColor, alertColor, Progress);
            }

            if (Timer >= overflowSeconds)
            {
                game.EndGame();
            }
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
