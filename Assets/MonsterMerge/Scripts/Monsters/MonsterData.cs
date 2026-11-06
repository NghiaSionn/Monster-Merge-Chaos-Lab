using UnityEngine;

namespace MonsterMerge
{
    /// <summary>Dữ liệu tĩnh của một cấp quái. Không giữ state runtime.</summary>
    [CreateAssetMenu(menuName = "Monster Merge/Monster Data", fileName = "MonsterTier")]
    public sealed class MonsterData : ScriptableObject
    {
        [Min(1)] public int tier = 1;
        public string displayName = "Monster";
        [Tooltip("Tùy chọn. Bỏ trống thì dùng hình tròn mặc định của prefab.")]
        public Sprite sprite;
        public Color color = Color.white;

        [Header("Vật lý")]
        [Min(0.05f)] public float radius = 0.25f;
        [Min(0.01f)] public float mass = 1f;
        [Min(0f)] public float friction = 0.4f;
        [Range(0f, 1f)] public float bounciness;
        [Tooltip("Rigidbody2D.angularDamping. Friction không làm chậm hình tròn đang lăn, nên 'khó lăn' phải chỉnh ở đây.")]
        [Min(0f)] public float rollResistance = 0.05f;

        [Header("Điểm")]
        [Tooltip("Điểm nhận được khi TẠO ra cấp này bằng hợp nhất.")]
        [Min(0)] public int mergeScore;

        /// <summary>Áp màu/sprite/số cấp. Dùng chung cho quái thật và preview.</summary>
        public void ApplyVisual(SpriteRenderer body, TextMesh label)
        {
            if (sprite != null)
            {
                body.sprite = sprite;
            }

            if (body.sprite != null)
            {
                // Sprite luôn được đưa về đường kính 1 unit trong không gian local của quái.
                float width = Mathf.Max(body.sprite.bounds.size.x, 0.0001f);
                body.transform.localScale = Vector3.one / width;
            }

            body.color = color;

            if (label != null)
            {
                label.text = tier.ToString();
            }
        }
    }
}
