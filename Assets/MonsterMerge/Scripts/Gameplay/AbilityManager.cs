using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MonsterMerge
{
    /// <summary>Năng lượng và kỹ năng Magnetic Pull. Không đổi luật hợp nhất: chỉ tạo lực hút.</summary>
    public sealed class AbilityManager : MonoBehaviour
    {
        [SerializeField] private GameManager game;
        [SerializeField] private MergeManager merge;
        [SerializeField] private Camera worldCamera;

        [Header("Năng lượng")]
        [SerializeField, Min(1)] private int maxEnergy = 100;
        [SerializeField, Min(0)] private int energyPerMerge = 25;

        [Header("Magnetic Pull")]
        [SerializeField, Min(1)] private int magniTier = 4;
        [SerializeField, Min(0.1f)] private float pullRange = 2.5f;
        [SerializeField, Min(0.05f)] private float pullDuration = 0.6f;
        [Tooltip("Gia tốc hút (nhân với mass nên không phụ thuộc cân nặng).")]
        [SerializeField, Min(0f)] private float pullForce = 12f;
        [SerializeField, Min(0.1f)] private float maxPullSpeed = 6f;
        [Tooltip("Nới vùng chạm quanh Magni cho dễ bấm bằng ngón tay.")]
        [SerializeField, Min(0f)] private float tapTolerance = 0.15f;

        private bool trackingTap;
        private Coroutine pullRoutine;

        public int Energy { get; private set; }
        public int MaxEnergy => maxEnergy;
        public bool IsTargeting { get; private set; }
        public bool IsPulling => pullRoutine != null;
        public bool CanActivate => game.State == GameState.Playing && Energy >= maxEnergy;

        public event Action StateChanged;
        public event Action<string> Feedback;

        private void OnEnable()
        {
            merge.Merged += OnMerged;
            game.GameEnded += OnGameEnded;
        }

        private void OnDisable()
        {
            merge.Merged -= OnMerged;
            game.GameEnded -= OnGameEnded;
        }

        private void OnMerged(MergeResult result)
        {
            AddEnergy(energyPerMerge);
        }

        public void AddEnergy(int amount)
        {
            int value = Mathf.Clamp(Energy + amount, 0, maxEnergy);
            if (value != Energy)
            {
                Energy = value;
                StateChanged?.Invoke();
            }
        }

        /// <summary>Nút Activate: bấm lần đầu vào chế độ chọn, bấm lần nữa để hủy.</summary>
        public void ToggleTargeting()
        {
            if (IsTargeting)
            {
                SetTargeting(false);
            }
            else if (CanActivate)
            {
                SetTargeting(true);
                Feedback?.Invoke("Chạm vào một Magni (cấp 4)");
            }
        }

        private void SetTargeting(bool value)
        {
            trackingTap = false;
            if (IsTargeting != value)
            {
                IsTargeting = value;
                StateChanged?.Invoke();
            }
        }

        private void Update()
        {
            if (!IsTargeting)
            {
                return;
            }

            Pointer pointer = Pointer.current;
            if (pointer == null)
            {
                return;
            }

            Vector2 screen = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame)
            {
                trackingTap = !PointerUtility.IsOverUi(screen);
            }

            // Chọn lúc NHẢ: lần bấm này không thể bị DropController hiểu thành lệnh thả sau khi thoát targeting.
            if (pointer.press.wasReleasedThisFrame && trackingTap)
            {
                trackingTap = false;
                if (!PointerUtility.IsOverUi(screen))
                {
                    TrySelectAt(PointerUtility.ToWorld(worldCamera, screen));
                }
            }
        }

        /// <summary>Chọn Magni tại điểm world. Năng lượng chỉ bị trừ khi có mục tiêu hợp lệ.</summary>
        public bool TrySelectAt(Vector2 worldPoint)
        {
            if (!IsTargeting)
            {
                return false;
            }

            Monster selected = FindTapped(worldPoint);
            if (selected == null)
            {
                Feedback?.Invoke("Hãy chạm đúng một Magni");
                return false;
            }

            Monster partner = FindNearestPartner(selected);
            if (partner == null)
            {
                Feedback?.Invoke("Không có Magni gần đó");
                return false;
            }

            Energy = 0;
            SetTargeting(false);
            StateChanged?.Invoke();
            Feedback?.Invoke("Hút!");
            pullRoutine = StartCoroutine(Pull(selected, partner));
            return true;
        }

        private bool IsValidMagni(Monster monster)
        {
            return monster != null && monster.Tier == magniTier && monster.HasBeenDropped && !monster.IsMerging;
        }

        private Monster FindTapped(Vector2 point)
        {
            Monster best = null;
            float bestDistance = float.MaxValue;
            IReadOnlyList<Monster> monsters = merge.ActiveMonsters;

            for (int i = 0; i < monsters.Count; i++)
            {
                Monster monster = monsters[i];
                if (!IsValidMagni(monster))
                {
                    continue;
                }

                float distance = Vector2.Distance(point, monster.Position);
                if (distance <= monster.Radius + tapTolerance && distance < bestDistance)
                {
                    best = monster;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private Monster FindNearestPartner(Monster source)
        {
            Monster best = null;
            float bestDistance = float.MaxValue;
            IReadOnlyList<Monster> monsters = merge.ActiveMonsters;

            for (int i = 0; i < monsters.Count; i++)
            {
                Monster monster = monsters[i];
                if (monster == source || !IsValidMagni(monster))
                {
                    continue;
                }

                float distance = Vector2.Distance(source.Position, monster.Position);
                bool closer = distance < bestDistance;
                // Hòa khoảng cách thì lấy Serial nhỏ hơn để kết quả ổn định giữa các lần chơi.
                bool tieBreak = Mathf.Approximately(distance, bestDistance) && best != null && monster.Serial < best.Serial;
                if (distance <= pullRange && (closer || tieBreak))
                {
                    best = monster;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private IEnumerator Pull(Monster a, Monster b)
        {
            a.Flash();
            b.Flash();

            float elapsed = 0f;
            var wait = new WaitForFixedUpdate();

            while (elapsed < pullDuration && IsValidMagni(a) && IsValidMagni(b) && game.State == GameState.Playing)
            {
                Vector2 direction = (b.Position - a.Position).normalized;
                ApplyPull(a, direction);
                ApplyPull(b, -direction);

                yield return wait;
                elapsed += Time.fixedDeltaTime;
            }

            pullRoutine = null;
        }

        private void ApplyPull(Monster monster, Vector2 direction)
        {
            Rigidbody2D body = monster.Body;
            if (Vector2.Dot(body.linearVelocity, direction) < maxPullSpeed)
            {
                body.AddForce(direction * (pullForce * body.mass));
            }
        }

        private void OnGameEnded()
        {
            SetTargeting(false);
            StateChanged?.Invoke();
        }
    }
}
