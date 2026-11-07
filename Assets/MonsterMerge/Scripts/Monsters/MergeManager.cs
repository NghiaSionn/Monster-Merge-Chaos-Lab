using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterMerge
{
    /// <summary>Luật khi hai quái cấp cuối chạm nhau. Dùng để so sánh A/B trong sprint v0.2.</summary>
    public enum MaxTierMergeMode
    {
        /// <summary>A: cặp cấp cuối không hợp nhất (hành vi v0.1).</summary>
        ClassicCap,

        /// <summary>B: cặp cấp cuối biến mất và thưởng điểm; không sinh cấp mới.</summary>
        OverloadBurst
    }

    public readonly struct MergeResult
    {
        /// <summary>Quái mới sinh ra; null khi là burst.</summary>
        public readonly Monster Created;
        public readonly Vector2 Position;
        public readonly int Score;
        public readonly int Combo;
        public readonly bool IsBurst;

        public MergeResult(Monster created, Vector2 position, int score, int combo, bool isBurst = false)
        {
            Created = created;
            Position = position;
            Score = score;
            Combo = combo;
            IsBurst = isBurst;
        }
    }

    /// <summary>Nơi duy nhất quyết định cặp hợp lệ và sinh quái mới. Cũng là nơi duy nhất tạo/hủy Monster.</summary>
    public sealed class MergeManager : MonoBehaviour
    {
        [SerializeField] private Monster monsterPrefab;
        [Tooltip("Thứ tự bắt buộc: phần tử 0 là cấp 1. Cấp cuối cùng không hợp nhất.")]
        [SerializeField] private MonsterData[] tiers;
        [SerializeField] private Transform monsterRoot;
        [SerializeField] private TankBounds tank;
        [SerializeField] private GameManager game;

        [SerializeField, Min(0f)] private float comboWindow = 1.5f;
        [Tooltip("Giới hạn tốc độ mọi quái, chống bắn văng/xuyên tường.")]
        [SerializeField, Min(1f)] private float maxSpeed = 12f;
        [Tooltip("Tỷ lệ vận tốc trung bình của cặp mà quái mới kế thừa.")]
        [SerializeField, Range(0f, 1f)] private float inheritVelocity = 0.5f;

        [Header("Cấp cuối (A/B)")]
        [SerializeField] private MaxTierMergeMode maxTierMode = MaxTierMergeMode.ClassicCap;
        [Tooltip("Điểm thưởng cho một cặp cấp cuối biến mất ở OverloadBurst.")]
        [SerializeField, Min(0)] private int burstScore = 300;
        [Tooltip("Xung lực đẩy quái xung quanh khi burst. 0 = tắt (mặc định, ổn định nhất).")]
        [SerializeField, Min(0f)] private float burstPushImpulse;
        [SerializeField, Min(0.1f)] private float burstPushRadius = 1.5f;

        private readonly List<Monster> active = new();
        private readonly Queue<(Monster a, Monster b)> pending = new();
        private int nextSerial = 1;
        private int combo;
        private float lastMergeTime = float.NegativeInfinity;

        public event Action<MergeResult> Merged;

        public IReadOnlyList<Monster> ActiveMonsters => active;
        public int Combo => combo;

        /// <summary>Chế độ cấp cuối. Có setter để test chuyển A/B mà không cần Inspector.</summary>
        public MaxTierMergeMode MaxTierMode
        {
            get => maxTierMode;
            set => maxTierMode = value;
        }

        /// <summary>Số lần hợp nhất trong ván, tính cả burst.</summary>
        public int MergeCount { get; private set; }

        /// <summary>Số cặp cấp cuối đã biến mất trong ván.</summary>
        public int BurstCount { get; private set; }

        /// <summary>Điểm thưởng của một lần burst (TASK-01, production/monster-merge-tasks-v0.2.md).</summary>
        public int BurstScore => burstScore;

        private void Awake()
        {
            for (int i = 0; i < tiers.Length; i++)
            {
                if (tiers[i] == null || tiers[i].tier != i + 1)
                {
                    Debug.LogError($"[MergeManager] tiers[{i}] phải là MonsterData cấp {i + 1}.", this);
                }
            }
        }

        public MonsterData GetTier(int tier)
        {
            return tier >= 1 && tier <= tiers.Length ? tiers[tier - 1] : null;
        }

        public Monster Spawn(MonsterData data, Vector2 position, bool popIn = false)
        {
            Monster monster = Instantiate(monsterPrefab, position, Quaternion.identity, monsterRoot);
            monster.Initialize(data, this, nextSerial++, maxSpeed, popIn);
            monster.MarkDropped();
            active.Add(monster);
            return monster;
        }

        /// <summary>Khóa cả hai quái ngay lập tức; việc tạo/hủy thật sự chờ tới FixedUpdate kế tiếp.</summary>
        public void RequestMerge(Monster a, Monster b)
        {
            if (!CanMerge(a, b))
            {
                return;
            }

            a.LockForMerge();
            b.LockForMerge();
            pending.Enqueue((a, b));
        }

        private bool CanMerge(Monster a, Monster b)
        {
            return a != null && b != null && a != b
                && game.State == GameState.Playing
                && !a.IsMerging && !b.IsMerging
                && a.HasBeenDropped && b.HasBeenDropped
                && a.Tier == b.Tier
                && (a.Tier < tiers.Length || maxTierMode == MaxTierMergeMode.OverloadBurst);
        }

        private void FixedUpdate()
        {
            while (pending.Count > 0)
            {
                (Monster a, Monster b) = pending.Dequeue();
                if (a != null && b != null)
                {
                    ExecuteMerge(a, b);
                }
            }
        }

        private void ExecuteMerge(Monster a, Monster b)
        {
            if (a.Tier >= tiers.Length)
            {
                ExecuteBurst(a, b);
                return;
            }

            MonsterData data = GetTier(a.Tier + 1);
            Vector2 position = tank.ClampInside((a.Position + b.Position) * 0.5f, data.radius);
            Vector2 velocity = (a.Body.linearVelocity + b.Body.linearVelocity) * (0.5f * inheritVelocity);

            Remove(a);
            Remove(b);

            Monster created = Spawn(data, position, popIn: true);
            created.Body.linearVelocity = velocity;
            created.Flash();

            AdvanceCombo();
            Merged?.Invoke(new MergeResult(created, position, data.mergeScore, combo));
        }

        /// <summary>
        /// OverloadBurst: xóa cả hai, không sinh cấp mới. Đi qua cùng sự kiện Merged nên điểm, năng lượng
        /// và combo được cộng đúng như một lần hợp nhất bình thường.
        /// </summary>
        private void ExecuteBurst(Monster a, Monster b)
        {
            Vector2 position = (a.Position + b.Position) * 0.5f;

            Remove(a);
            Remove(b);

            if (burstPushImpulse > 0f)
            {
                PushAway(position);
            }

            BurstCount++;
            AdvanceCombo();
            Merged?.Invoke(new MergeResult(null, position, burstScore, combo, isBurst: true));
        }

        private void AdvanceCombo()
        {
            MergeCount++;
            combo = Time.time - lastMergeTime <= comboWindow ? combo + 1 : 1;
            lastMergeTime = Time.time;
        }

        private void PushAway(Vector2 center)
        {
            for (int i = 0; i < active.Count; i++)
            {
                Monster monster = active[i];
                Vector2 offset = monster.Position - center;
                float distance = offset.magnitude;
                if (monster.IsMerging || distance >= burstPushRadius || distance < 0.0001f)
                {
                    continue;
                }

                float falloff = 1f - distance / burstPushRadius;
                monster.Body.AddForce(offset / distance * (burstPushImpulse * falloff), ForceMode2D.Impulse);
            }
        }

        private void Remove(Monster monster)
        {
            active.Remove(monster);
            // Tắt ngay để physics không còn thấy nó trong lúc chờ Destroy cuối frame.
            monster.gameObject.SetActive(false);
            Destroy(monster.gameObject);
        }
    }
}
