using System.Collections;
using UnityEngine;

namespace MonsterMerge
{
    /// <summary>
    /// Hiệu ứng hình tại điểm hợp nhất. Chỉ là hình ảnh: không collider, không lực, không chặn input.
    /// Hiện chỉ có vòng nổ cho OverloadBurst; TASK-04 sẽ mở rộng cho merge thường.
    /// </summary>
    public sealed class MergeEffects : MonoBehaviour
    {
        [SerializeField] private MergeManager merge;
        [SerializeField] private Sprite ringSprite;
        [SerializeField] private Material material;

        [Header("Burst")]
        [SerializeField] private Color burstColor = new(1f, 0.6f, 0.2f, 0.9f);
        [SerializeField, Min(0.05f)] private float burstDuration = 0.35f;
        [SerializeField, Min(0f)] private float burstStartSize = 0.6f;
        [SerializeField, Min(0f)] private float burstEndSize = 2.4f;
        [Tooltip("Số hiệu ứng tối đa tồn tại cùng lúc; vượt quá thì bỏ qua hiệu ứng mới.")]
        [SerializeField, Min(1)] private int maxActive = 8;
        [SerializeField] private int sortingOrder = 8;

        private int activeCount;

        /// <summary>Số hiệu ứng đang hiển thị.</summary>
        public int ActiveCount => activeCount;

        private void OnEnable()
        {
            merge.Merged += OnMerged;
        }

        private void OnDisable()
        {
            merge.Merged -= OnMerged;
        }

        private void OnMerged(MergeResult result)
        {
            if (result.IsBurst)
            {
                PlayBurst(result.Position);
            }
        }

        /// <summary>Phát vòng nổ tại vị trí world. Trả về false nếu đã chạm giới hạn số hiệu ứng.</summary>
        public bool PlayBurst(Vector2 position)
        {
            if (activeCount >= maxActive || ringSprite == null)
            {
                return false;
            }

            var ring = new GameObject("BurstFx");
            ring.transform.SetParent(transform, false);
            ring.transform.position = position;

            var renderer = ring.AddComponent<SpriteRenderer>();
            renderer.sprite = ringSprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;

            activeCount++;
            StartCoroutine(Animate(ring, renderer));
            return true;
        }

        private IEnumerator Animate(GameObject ring, SpriteRenderer renderer)
        {
            for (float elapsed = 0f; elapsed < burstDuration; elapsed += Time.deltaTime)
            {
                float t = elapsed / burstDuration;
                ring.transform.localScale = Vector3.one * Mathf.Lerp(burstStartSize, burstEndSize, t);
                Color color = burstColor;
                color.a *= 1f - t;
                renderer.color = color;
                yield return null;
            }

            activeCount--;
            Destroy(ring);
        }
    }
}
