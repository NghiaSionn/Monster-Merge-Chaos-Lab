using UnityEngine;

namespace MonsterMerge
{
    /// <summary>Giữ trọn bể trong khung hình ở mọi tỉ lệ màn hình, không hardcode theo thiết bị.</summary>
    [RequireComponent(typeof(Camera))]
    public sealed class PortraitCameraFit : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float minHalfHeight = 5f;
        [SerializeField, Min(0.1f)] private float minHalfWidth = 2.7f;

        private Camera target;
        private float appliedAspect;

        private void Awake()
        {
            target = GetComponent<Camera>();
            Apply();
        }

        private void Update()
        {
            if (!Mathf.Approximately(appliedAspect, target.aspect))
            {
                Apply();
            }
        }

        private void Apply()
        {
            appliedAspect = target.aspect;
            target.orthographicSize = Mathf.Max(minHalfHeight, minHalfWidth / appliedAspect);
        }
    }
}
