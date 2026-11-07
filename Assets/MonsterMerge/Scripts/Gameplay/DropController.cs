using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MonsterMerge
{
    /// <summary>Hàng đợi current/next, preview và thao tác kéo-thả. Không tính điểm, không hợp nhất.</summary>
    public sealed class DropController : MonoBehaviour
    {
        [Serializable]
        private struct SpawnEntry
        {
            public MonsterData data;
            [Min(0f)] public float weight;
        }

        [SerializeField] private GameManager game;
        [SerializeField] private MergeManager merge;
        [SerializeField] private AbilityManager ability;
        [SerializeField] private TankBounds tank;
        [SerializeField] private Camera worldCamera;

        [Header("Preview (không có collider, không phải Monster)")]
        [SerializeField] private Transform previewRoot;
        [SerializeField] private SpriteRenderer previewBody;
        [SerializeField] private TextMesh previewLabel;
        [Tooltip("Vạch dọc mờ từ preview xuống đáy, chỉ rõ quái sẽ rơi ở đâu (v0.2 TASK-05). Bỏ trống thì không hiện.")]
        [SerializeField] private SpriteRenderer dropGuide;

        [Header("Cân bằng")]
        [SerializeField] private SpawnEntry[] spawnTable;
        [SerializeField, Range(0.1f, 2f)] private float dropCooldown = 0.5f;
        [Tooltip("Khác 0 thì cố định chuỗi random để tái hiện lỗi.")]
        [SerializeField] private int randomSeed;

        private bool aiming;
        private float aimX;
        private float nextDropTime;

        public MonsterData Current { get; private set; }
        public MonsterData Next { get; private set; }
        public bool CanDrop => game.State == GameState.Playing && !ability.IsTargeting;
        public bool IsCoolingDown => Time.time < nextDropTime;
        public bool IsAiming => aiming;

        public event Action QueueChanged;
        public event Action Dropped;

        private void Awake()
        {
            if (randomSeed != 0)
            {
                UnityEngine.Random.InitState(randomSeed);
            }

            Current = RollSpawn();
            Next = RollSpawn();
        }

        private void Start()
        {
            RefreshPreview();
            QueueChanged?.Invoke();
        }

        private void Update()
        {
            if (!CanDrop)
            {
                aiming = false;
            }
            else
            {
                ReadPointer();
            }

            UpdatePreviewPosition();
        }

        private void ReadPointer()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null)
            {
                return;
            }

            Vector2 screen = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame)
            {
                // Lần bấm bắt đầu trên UI thì bỏ qua cả thao tác đó.
                aiming = !PointerUtility.IsOverUi(screen);
            }

            if (!aiming)
            {
                return;
            }

            aimX = PointerUtility.ToWorld(worldCamera, screen).x;

            if (pointer.press.wasReleasedThisFrame)
            {
                aiming = false;
                // Kéo sang UI rồi nhả được coi là hủy, không thả.
                if (!PointerUtility.IsOverUi(screen))
                {
                    TryDropAt(aimX);
                }
            }
            else if (!pointer.press.isPressed)
            {
                aiming = false;
            }
        }

        /// <summary>Thả quái hiện tại tại X, đã clamp theo bán kính. Trả về null nếu chưa được phép.</summary>
        public Monster TryDropAt(float x)
        {
            if (!CanDrop || IsCoolingDown)
            {
                return null;
            }

            float clampedX = tank.ClampX(x, Current.radius);
            Monster monster = merge.Spawn(Current, new Vector2(clampedX, tank.SpawnY));
            nextDropTime = Time.time + dropCooldown;

            Current = Next;
            Next = RollSpawn();
            RefreshPreview();

            QueueChanged?.Invoke();
            Dropped?.Invoke();
            return monster;
        }

        private MonsterData RollSpawn()
        {
            float total = 0f;
            foreach (SpawnEntry entry in spawnTable)
            {
                total += entry.weight;
            }

            float roll = UnityEngine.Random.value * total;
            foreach (SpawnEntry entry in spawnTable)
            {
                roll -= entry.weight;
                if (roll <= 0f && entry.weight > 0f)
                {
                    return entry.data;
                }
            }

            return spawnTable[0].data;
        }

        private void RefreshPreview()
        {
            Current.ApplyVisual(previewBody, previewLabel);
            previewRoot.localScale = Vector3.one * (Current.radius * 2f);
        }

        private void UpdatePreviewPosition()
        {
            bool visible = CanDrop && !IsCoolingDown;
            if (previewRoot.gameObject.activeSelf != visible)
            {
                previewRoot.gameObject.SetActive(visible);
            }

            aimX = tank.ClampX(aimX, Current.radius);
            previewRoot.position = new Vector3(aimX, tank.SpawnY, 0f);
            UpdateDropGuide(visible);
        }

        private void UpdateDropGuide(bool visible)
        {
            if (dropGuide == null)
            {
                return;
            }

            if (dropGuide.gameObject.activeSelf != visible)
            {
                dropGuide.gameObject.SetActive(visible);
            }

            float top = tank.SpawnY - Current.radius;
            float height = Mathf.Max(0f, top - tank.FloorY);
            Transform guide = dropGuide.transform;
            guide.position = new Vector3(aimX, tank.FloorY + height * 0.5f, 0f);
            Vector3 scale = guide.localScale;
            scale.y = height;
            guide.localScale = scale;
        }

        /// <summary>Vị trí preview hiện tại; dùng cho test.</summary>
        public Vector3 PreviewPosition => previewRoot.position;

        public bool IsPreviewVisible => previewRoot.gameObject.activeSelf;

        /// <summary>Vạch chỉ vị trí thả; null nếu scene chưa có.</summary>
        public Transform DropGuideTransform => dropGuide != null ? dropGuide.transform : null;

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                aiming = false;
            }
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
            {
                aiming = false;
            }
        }
    }
}
