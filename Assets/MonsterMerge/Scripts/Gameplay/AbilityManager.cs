using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MonsterMerge
{
    /// <summary>Kỹ năng nam châm có dùng được không, và nếu không thì vì sao (TASK-02 v0.2).</summary>
    public enum MagnetAvailability
    {
        NotEnoughEnergy,
        NeedTwoMagni,
        NoPairInRange,
        Ready
    }

    /// <summary>
    /// Năng lượng và kỹ năng Magnetic Pull (GDD mục 4). Không đổi luật hợp nhất: chỉ tạo lực hút.
    /// Năng lượng chỉ bị trừ khi bắt đầu hút hợp lệ, kể cả khi hai Magni bị kẹt (quyết định 07/11/2026).
    /// </summary>
    public sealed class AbilityManager : MonoBehaviour
    {
        public const string MagnetTipSeenKey = "MonsterMerge.MagnetTipSeen";

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

        [Header("Phản hồi")]
        [Tooltip("Mức làm sáng các Magni có cặp trong tầm khi kỹ năng sẵn sàng. 0 = tắt.")]
        [SerializeField, Range(0f, 1f)] private float magniHighlight = 0.4f;
        [Tooltip("Ghi số liệu kỹ năng ra Console khi hết ván (chỉ trong Editor).")]
        [SerializeField] private bool logStats = true;

        private readonly List<Monster> magnis = new();
        private MagnetAvailability availability;
        private bool trackingTap;
        private Coroutine pullRoutine;
        private float roundStartTime;

        public int Energy { get; private set; }
        public int MaxEnergy => maxEnergy;
        public bool IsTargeting { get; private set; }
        public bool IsPulling => pullRoutine != null;

        /// <summary>Đầy năng lượng và đang chơi: nút bấm được (để hiện lý do nếu chưa có mục tiêu).</summary>
        public bool CanActivate => game.State == GameState.Playing && Energy >= maxEnergy;

        /// <summary>Đầy năng lượng và có ít nhất một cặp Magni trong tầm.</summary>
        public bool IsReady => CanActivate && availability == MagnetAvailability.Ready;

        public MagnetAvailability Availability => availability;

        /// <summary>Số lần vào chế độ chọn mục tiêu.</summary>
        public int ActivateCount { get; private set; }

        /// <summary>Số lần hút hợp lệ.</summary>
        public int SuccessCount { get; private set; }

        /// <summary>Số lần chạm khi đang chọn mà không hút được.</summary>
        public int FailedAttemptCount { get; private set; }

        /// <summary>Giây kể từ đầu ván khi năng lượng đầy lần đầu; -1 nếu chưa.</summary>
        public float EnergyFullAt { get; private set; } = -1f;

        /// <summary>Giây kể từ đầu ván khi lần đầu có cặp Magni trong tầm; -1 nếu chưa.</summary>
        public float FirstPairAt { get; private set; } = -1f;

        public event Action StateChanged;
        public event Action<string> Feedback;

        /// <summary>Hướng dẫn một lần, hiện lâu hơn Feedback.</summary>
        public event Action<string> TipRequested;

        /// <summary>Câu giải thích vì sao chưa dùng được. Rỗng khi sẵn sàng.</summary>
        public static string ReasonText(MagnetAvailability value)
        {
            return value switch
            {
                MagnetAvailability.NotEnoughEnergy => "Chưa đủ năng lượng",
                MagnetAvailability.NeedTwoMagni => "Cần 2 Magni",
                MagnetAvailability.NoPairInRange => "Không có cặp trong tầm",
                _ => string.Empty
            };
        }

        private void Awake()
        {
            roundStartTime = Time.time;
        }

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
            if (value == Energy)
            {
                return;
            }

            Energy = value;
            if (Energy >= maxEnergy && EnergyFullAt < 0f)
            {
                EnergyFullAt = Time.time - roundStartTime;
            }

            RefreshAvailability();
            StateChanged?.Invoke();
        }

        /// <summary>
        /// Nút Activate: bấm lần đầu vào chế độ chọn, bấm lần nữa để hủy.
        /// Chưa có mục tiêu hợp lệ thì chỉ báo lý do, không vào chế độ chọn.
        /// </summary>
        public void ToggleTargeting()
        {
            if (IsTargeting)
            {
                SetTargeting(false);
                return;
            }

            if (!CanActivate)
            {
                return;
            }

            RefreshAvailability();
            if (availability != MagnetAvailability.Ready)
            {
                Feedback?.Invoke(ReasonText(availability));
                return;
            }

            ActivateCount++;
            SetTargeting(true);
            Feedback?.Invoke("Chạm vào một Magni đang sáng");
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
            RefreshAvailability();

            if (IsTargeting)
            {
                ReadTargetTap();
            }
        }

        private void ReadTargetTap()
        {
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

        /// <summary>
        /// Tính lại trạng thái kỹ năng và làm sáng các Magni có cặp trong tầm.
        /// Chỉ duyệt Magni nên chi phí nhỏ; danh sách được dùng lại, không cấp phát mỗi frame.
        /// </summary>
        private void RefreshAvailability()
        {
            magnis.Clear();
            IReadOnlyList<Monster> monsters = merge.ActiveMonsters;
            for (int i = 0; i < monsters.Count; i++)
            {
                if (IsValidMagni(monsters[i]))
                {
                    magnis.Add(monsters[i]);
                }
            }

            bool showHighlight = CanActivate;
            bool anyPair = false;
            for (int i = 0; i < magnis.Count; i++)
            {
                bool hasPartner = false;
                for (int j = 0; j < magnis.Count && !hasPartner; j++)
                {
                    hasPartner = i != j && Vector2.Distance(magnis[i].Position, magnis[j].Position) <= pullRange;
                }

                anyPair |= hasPartner;
                magnis[i].SetHighlight(showHighlight && hasPartner ? magniHighlight : 0f);
            }

            if (anyPair && FirstPairAt < 0f)
            {
                FirstPairAt = Time.time - roundStartTime;
            }

            MagnetAvailability next = Energy < maxEnergy ? MagnetAvailability.NotEnoughEnergy
                : magnis.Count < 2 ? MagnetAvailability.NeedTwoMagni
                : !anyPair ? MagnetAvailability.NoPairInRange
                : MagnetAvailability.Ready;

            if (next == availability)
            {
                return;
            }

            availability = next;
            if (IsReady)
            {
                ShowTipOnce();
            }

            StateChanged?.Invoke();
        }

        private void ShowTipOnce()
        {
            if (PlayerPrefs.GetInt(MagnetTipSeenKey, 0) != 0)
            {
                return;
            }

            PlayerPrefs.SetInt(MagnetTipSeenKey, 1);
            TipRequested?.Invoke("Nam châm sẵn sàng! Bấm nút rồi chạm một Magni đang sáng");
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
                FailedAttemptCount++;
                Feedback?.Invoke("Hãy chạm đúng một Magni đang sáng");
                return false;
            }

            Monster partner = FindNearestPartner(selected);
            if (partner == null)
            {
                FailedAttemptCount++;
                Feedback?.Invoke(ReasonText(MagnetAvailability.NoPairInRange));
                return false;
            }

            SuccessCount++;
            Energy = 0;
            SetTargeting(false);
            RefreshAvailability();
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
            RefreshAvailability();
            StateChanged?.Invoke();

            if (logStats && Application.isEditor)
            {
                Debug.Log($"[MonsterMerge] Nam châm: đầy lần đầu={FormatTime(EnergyFullAt)}, có cặp lần đầu={FormatTime(FirstPairAt)}, " +
                    $"kích hoạt={ActivateCount}, thành công={SuccessCount}, thất bại={FailedAttemptCount}");
            }
        }

        private static string FormatTime(float seconds) => seconds < 0f ? "chưa" : $"{seconds:F0}s";
    }
}
