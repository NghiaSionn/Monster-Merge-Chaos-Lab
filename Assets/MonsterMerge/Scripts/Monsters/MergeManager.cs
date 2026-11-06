using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterMerge
{
    public readonly struct MergeResult
    {
        public readonly Monster Created;
        public readonly Vector2 Position;
        public readonly int Score;
        public readonly int Combo;

        public MergeResult(Monster created, Vector2 position, int score, int combo)
        {
            Created = created;
            Position = position;
            Score = score;
            Combo = combo;
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

        private readonly List<Monster> active = new();
        private readonly Queue<(Monster a, Monster b)> pending = new();
        private int nextSerial = 1;
        private int combo;
        private float lastMergeTime = float.NegativeInfinity;

        public event Action<MergeResult> Merged;

        public IReadOnlyList<Monster> ActiveMonsters => active;
        public int Combo => combo;

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
                && a.Tier < tiers.Length;
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
            MonsterData data = GetTier(a.Tier + 1);
            Vector2 position = tank.ClampInside((a.Position + b.Position) * 0.5f, data.radius);
            Vector2 velocity = (a.Body.linearVelocity + b.Body.linearVelocity) * (0.5f * inheritVelocity);

            Remove(a);
            Remove(b);

            Monster created = Spawn(data, position, popIn: true);
            created.Body.linearVelocity = velocity;
            created.Flash();

            combo = Time.time - lastMergeTime <= comboWindow ? combo + 1 : 1;
            lastMergeTime = Time.time;

            Merged?.Invoke(new MergeResult(created, position, data.mergeScore, combo));
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
