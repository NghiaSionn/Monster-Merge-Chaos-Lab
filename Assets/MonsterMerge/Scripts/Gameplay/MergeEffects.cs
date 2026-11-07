using System;
using System.Collections;
using UnityEngine;

namespace MonsterMerge
{
    /// <summary>
    /// Phản hồi hình và tiếng khi hợp nhất (v0.2 TASK-01, TASK-04). Chỉ là trình bày:
    /// không collider, không lực, không chặn input, không đổi luật chơi.
    /// </summary>
    public sealed class MergeEffects : MonoBehaviour
    {
        private const int SampleRate = 44100;

        [SerializeField] private MergeManager merge;
        [SerializeField] private Sprite ringSprite;
        [SerializeField] private Material material;

        [Header("Juice (TASK-04)")]
        [Tooltip("Tắt để quay về cảm giác v0.1: vẫn giữ flash và popup điểm, tắt pop phần hình, vòng sáng merge, âm thanh, nhấn combo.")]
        [SerializeField] private bool juiceEnabled = true;

        [Header("Pop phần hình (không đụng collider)")]
        [SerializeField, Range(0.12f, 0.2f)] private float visualPopDuration = 0.16f;
        [Tooltip("Mức phình thêm ở giữa pop. 0.18 = to hơn 18%.")]
        [SerializeField, Range(0f, 0.5f)] private float visualPopOvershoot = 0.18f;

        [Header("Vòng sáng merge")]
        [SerializeField, Min(0.05f)] private float mergeRingDuration = 0.25f;
        [Tooltip("Kích thước tính theo đường kính quái mới.")]
        [SerializeField, Min(0f)] private float mergeRingStartScale = 1f;
        [SerializeField, Min(0f)] private float mergeRingEndScale = 1.8f;
        [Tooltip("Pha màu quái với trắng để vòng nổi trên nền.")]
        [SerializeField, Range(0f, 1f)] private float mergeRingWhiten = 0.5f;
        [SerializeField, Range(0f, 1f)] private float mergeRingAlpha = 0.8f;

        [Header("Burst (TASK-01)")]
        [Tooltip("Vàng nhạt để không bị nhầm với Crag màu cam.")]
        [SerializeField] private Color burstColor = new(1f, 0.95f, 0.7f, 0.9f);
        [SerializeField, Min(0.05f)] private float burstDuration = 0.35f;
        [SerializeField, Min(0f)] private float burstStartSize = 0.6f;
        [SerializeField, Min(0f)] private float burstEndSize = 2.4f;

        [Header("Giới hạn và thứ tự vẽ")]
        [Tooltip("Số vòng sáng tối đa cùng lúc; vượt quá thì bỏ qua vòng mới.")]
        [SerializeField, Min(1)] private int maxActive = 8;
        [Tooltip("Thấp hơn quái (5) để vòng không che quái.")]
        [SerializeField] private int sortingOrder = 3;

        [Header("Âm thanh")]
        [Tooltip("Bỏ trống thì dùng tiếng bíp sinh bằng code (không có vấn đề bản quyền).")]
        [SerializeField] private AudioClip mergeClip;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 0.5f;
        [SerializeField] private bool sfxMuted;
        [Tooltip("Khoảng cách tối thiểu giữa hai tiếng để không bị chồng âm khi nhiều merge cùng lúc.")]
        [SerializeField, Min(0f)] private float sfxMinInterval = 0.05f;
        [SerializeField, Range(1, 8)] private int sfxVoices = 4;
        [SerializeField, Range(0.5f, 2f)] private float basePitch = 1.2f;
        [Tooltip("Mỗi cấp cao hơn hạ cao độ một chút để nghe được cấp lớn.")]
        [SerializeField, Range(0f, 0.3f)] private float pitchStepPerTier = 0.1f;
        [SerializeField, Range(0.3f, 2f)] private float burstPitch = 0.6f;

        private AudioSource[] voices;
        private int nextVoice;
        private float lastSfxTime = float.NegativeInfinity;
        private AudioClip generatedClip;
        private int activeCount;

        /// <summary>Bật/tắt toàn bộ juice của TASK-04. Có setter để test và A/B nhanh.</summary>
        public bool JuiceEnabled
        {
            get => juiceEnabled;
            set => juiceEnabled = value;
        }

        /// <summary>Số vòng sáng đang hiển thị.</summary>
        public int ActiveCount => activeCount;

        /// <summary>Số tiếng đã phát trong ván.</summary>
        public int SfxPlayCount { get; private set; }

        public float SfxMinInterval => sfxMinInterval;

        public event Action SfxPlayed;

        private void Awake()
        {
            voices = new AudioSource[sfxVoices];
            for (int i = 0; i < voices.Length; i++)
            {
                AudioSource voice = gameObject.AddComponent<AudioSource>();
                voice.playOnAwake = false;
                voice.spatialBlend = 0f;
                voices[i] = voice;
            }
        }

        private void OnEnable()
        {
            merge.Merged += OnMerged;
        }

        private void OnDisable()
        {
            merge.Merged -= OnMerged;
        }

        private void OnDestroy()
        {
            if (generatedClip != null)
            {
                Destroy(generatedClip);
            }
        }

        private void OnMerged(MergeResult result)
        {
            if (result.IsBurst)
            {
                // Vòng nổ là phản hồi chức năng của TASK-01 nên luôn bật.
                PlayBurst(result.Position);
                if (juiceEnabled)
                {
                    PlaySfx(burstPitch);
                }

                return;
            }

            if (!juiceEnabled || result.Created == null)
            {
                return;
            }

            MonsterData data = result.Created.Data;
            result.Created.PlayVisualPop(visualPopDuration, visualPopOvershoot);

            Color color = Color.Lerp(data.color, Color.white, mergeRingWhiten);
            color.a = mergeRingAlpha;
            float diameter = data.radius * 2f;
            SpawnRing("MergeFx", result.Position, color, diameter * mergeRingStartScale, diameter * mergeRingEndScale, mergeRingDuration);

            PlaySfx(Mathf.Max(0.3f, basePitch - pitchStepPerTier * (data.tier - 2)));
        }

        /// <summary>Phát vòng nổ tại vị trí world. Trả về false nếu đã chạm giới hạn số hiệu ứng.</summary>
        public bool PlayBurst(Vector2 position)
        {
            return SpawnRing("BurstFx", position, burstColor, burstStartSize, burstEndSize, burstDuration);
        }

        private bool SpawnRing(string ringName, Vector2 position, Color color, float startSize, float endSize, float duration)
        {
            if (activeCount >= maxActive || ringSprite == null)
            {
                return false;
            }

            var ring = new GameObject(ringName);
            ring.transform.SetParent(transform, false);
            ring.transform.position = position;
            ring.transform.localScale = Vector3.one * startSize;

            var renderer = ring.AddComponent<SpriteRenderer>();
            renderer.sprite = ringSprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = sortingOrder;
            renderer.color = color;

            activeCount++;
            StartCoroutine(Animate(ring, renderer, color, startSize, endSize, duration));
            return true;
        }

        private IEnumerator Animate(GameObject ring, SpriteRenderer renderer, Color color, float startSize, float endSize, float duration)
        {
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                float t = elapsed / duration;
                ring.transform.localScale = Vector3.one * Mathf.Lerp(startSize, endSize, t);
                Color faded = color;
                faded.a *= 1f - t;
                renderer.color = faded;
                yield return null;
            }

            activeCount--;
            Destroy(ring);
        }

        private bool PlaySfx(float pitch)
        {
            if (sfxMuted || sfxVolume <= 0f || Time.time - lastSfxTime < sfxMinInterval)
            {
                return false;
            }

            AudioSource voice = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;

            voice.clip = mergeClip != null ? mergeClip : GetGeneratedClip();
            voice.pitch = pitch;
            voice.volume = sfxVolume;
            voice.Play();

            lastSfxTime = Time.time;
            SfxPlayCount++;
            SfxPlayed?.Invoke();
            return true;
        }

        /// <summary>Tiếng "bíp" ngắn: sóng sin 660 Hz, lên nhanh rồi tắt dần, dài 0,09 giây.</summary>
        private AudioClip GetGeneratedClip()
        {
            if (generatedClip != null)
            {
                return generatedClip;
            }

            const float Duration = 0.09f;
            const float Frequency = 660f;
            const float Attack = 0.005f;
            int samples = Mathf.CeilToInt(SampleRate * Duration);
            var data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float time = (float)i / SampleRate;
                float envelope = Mathf.Min(1f, time / Attack) * Mathf.Exp(-time * 40f);
                data[i] = Mathf.Sin(2f * Mathf.PI * Frequency * time) * envelope * 0.6f;
            }

            generatedClip = AudioClip.Create("MergeBlip", samples, 1, SampleRate, false);
            generatedClip.SetData(data, 0);
            return generatedClip;
        }
    }
}
