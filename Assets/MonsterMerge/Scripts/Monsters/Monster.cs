using System.Collections.Generic;
using UnityEngine;

namespace MonsterMerge
{
    /// <summary>
    /// Một quái trong bể. Chiến lược kích thước duy nhất: collider.radius cố định 0.5,
    /// transform scale = đường kính theo dữ liệu. Không tự tính điểm, không spawn.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public sealed class Monster : MonoBehaviour
    {
        private const float ColliderRadius = 0.5f;
        private const float PopStartScale = 0.7f;
        private const float PopDuration = 0.12f;
        private const float FlashFadePerSecond = 3f;

        // Mỗi cấp một material. Project tắt Domain Reload nên entry có thể đã bị hủy: luôn kiểm tra null.
        private static readonly Dictionary<MonsterData, PhysicsMaterial2D> Materials = new();

        [SerializeField] private SpriteRenderer body;
        [SerializeField] private TextMesh label;
        [SerializeField] private Rigidbody2D rigidbody2d;
        [SerializeField] private CircleCollider2D circleCollider;

        private MergeManager owner;
        private float maxSpeed;
        private float popElapsed = PopDuration;
        private float flash;
        private float highlight;
        private Vector3 bodyBaseScale = Vector3.one;
        private Vector3 labelBaseScale = Vector3.one;
        private float visualPopElapsed;
        private float visualPopDuration;
        private float visualPopOvershoot;

        public MonsterData Data { get; private set; }
        public int Tier => Data != null ? Data.tier : 0;
        public float Radius => Data != null ? Data.radius : 0f;
        public int Serial { get; private set; }
        public bool IsMerging { get; private set; }
        public bool HasBeenDropped { get; private set; }
        public float DroppedAt { get; private set; }
        public Rigidbody2D Body => rigidbody2d;
        public Vector2 Position => rigidbody2d.position;
        public float TopY => rigidbody2d.position.y + Radius;

        public void Initialize(MonsterData data, MergeManager manager, int serial, float speedLimit, bool popIn)
        {
            Data = data;
            owner = manager;
            Serial = serial;
            maxSpeed = speedLimit;
            name = $"Monster_T{data.tier}_{serial}";

            circleCollider.radius = ColliderRadius;
            circleCollider.sharedMaterial = GetMaterial(data);
            rigidbody2d.mass = data.mass;
            rigidbody2d.angularDamping = data.rollResistance;
            data.ApplyVisual(body, label);
            bodyBaseScale = body.transform.localScale;
            labelBaseScale = label != null ? label.transform.localScale : Vector3.one;
            visualPopElapsed = visualPopDuration = 0f;
            VisualPopScale = 1f;

            popElapsed = popIn ? 0f : PopDuration;
            ApplyScale();
        }

        /// <summary>
        /// Nảy phần hình (thân và số) lên rồi về, không đụng transform gốc nên collider giữ nguyên (v0.2 TASK-04).
        /// </summary>
        public void PlayVisualPop(float duration, float overshoot)
        {
            visualPopDuration = Mathf.Max(0.01f, duration);
            visualPopOvershoot = overshoot;
            visualPopElapsed = 0f;
        }

        /// <summary>Tỉ lệ phóng hiện tại của phần hình so với kích thước gốc; 1 khi không pop.</summary>
        public float VisualPopScale { get; private set; } = 1f;

        private void UpdateVisualPop()
        {
            if (visualPopElapsed >= visualPopDuration)
            {
                return;
            }

            visualPopElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(visualPopElapsed / visualPopDuration);
            VisualPopScale = t >= 1f ? 1f : 1f + visualPopOvershoot * Mathf.Sin(Mathf.PI * t);

            body.transform.localScale = bodyBaseScale * VisualPopScale;
            if (label != null)
            {
                label.transform.localScale = labelBaseScale * VisualPopScale;
            }
        }

        public void MarkDropped()
        {
            HasBeenDropped = true;
            DroppedAt = Time.time;
        }

        /// <summary>Chỉ MergeManager gọi. Khóa quái trước khi xếp hàng hợp nhất.</summary>
        public void LockForMerge()
        {
            IsMerging = true;
        }

        public void Flash()
        {
            flash = 1f;
        }

        /// <summary>Mức sáng ổn định đang áp dụng (0 = bình thường).</summary>
        public float HighlightAmount => highlight;

        /// <summary>Làm sáng ổn định, không nhấp nháy. Gọi lặp lại cùng giá trị không tốn gì.</summary>
        public void SetHighlight(float amount)
        {
            if (Mathf.Approximately(amount, highlight))
            {
                return;
            }

            highlight = amount;
            ApplyColor();
        }

        private void ApplyColor()
        {
            if (Data != null)
            {
                body.color = Color.Lerp(Data.color, Color.white, Mathf.Max(flash, highlight));
            }
        }

        private static PhysicsMaterial2D GetMaterial(MonsterData data)
        {
            if (!Materials.TryGetValue(data, out PhysicsMaterial2D material) || material == null)
            {
                material = new PhysicsMaterial2D(data.name);
                Materials[data] = material;
            }

            // Gán lại mỗi lần để chỉnh Inspector trong Play Mode có tác dụng với quái spawn sau đó.
            material.friction = data.friction;
            material.bounciness = data.bounciness;
            return material;
        }

        private void ApplyScale()
        {
            float t = Mathf.Clamp01(popElapsed / PopDuration);
            float pop = Mathf.Lerp(PopStartScale, 1f, t);
            transform.localScale = Vector3.one * (Data.radius * 2f * pop);
        }

        private void Update()
        {
            UpdateVisualPop();

            if (flash <= 0f || Data == null)
            {
                return;
            }

            flash = Mathf.Max(0f, flash - Time.deltaTime * FlashFadePerSecond);
            ApplyColor();
        }

        /// <summary>
        /// Giữ số cấp luôn đứng thẳng trong khi thân vẫn lăn (v0.2 TASK-03).
        /// Chạy sau khi Rigidbody2D đã nội suy transform, chỉ đổi góc của nhãn nên không tạo lực vật lý.
        /// </summary>
        private void LateUpdate()
        {
            if (label != null)
            {
                label.transform.rotation = Quaternion.identity;
            }
        }

        /// <summary>Nhãn số cấp; dùng cho test kiểm tra hướng đọc.</summary>
        public Transform LabelTransform => label != null ? label.transform : null;

        private void FixedUpdate()
        {
            if (Data == null)
            {
                return;
            }

            if (popElapsed < PopDuration)
            {
                // Lớn dần thay vì xuất hiện đủ cỡ, tránh đẩy văng các quái đang nằm cạnh.
                popElapsed += Time.fixedDeltaTime;
                ApplyScale();
            }

            Vector2 velocity = rigidbody2d.linearVelocity;
            if (velocity.sqrMagnitude > maxSpeed * maxSpeed)
            {
                rigidbody2d.linearVelocity = velocity.normalized * maxSpeed;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision) => ReportContact(collision);

        // Cần Stay cho cặp đã chạm sẵn; MergeManager lọc trùng bằng IsMerging.
        private void OnCollisionStay2D(Collision2D collision) => ReportContact(collision);

        private void ReportContact(Collision2D collision)
        {
            if (owner != null && !IsMerging && collision.collider.TryGetComponent(out Monster other))
            {
                owner.RequestMerge(this, other);
            }
        }
    }
}
