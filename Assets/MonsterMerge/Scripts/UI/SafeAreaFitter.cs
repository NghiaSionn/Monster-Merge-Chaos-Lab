using UnityEngine;

namespace MonsterMerge
{
    /// <summary>Co RectTransform vào Screen.safeArea để HUD tránh tai thỏ/thanh điều hướng.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private Rect applied;
        private Vector2Int appliedScreen;

        private void Start() => Apply();

        private void Update()
        {
            if (Screen.safeArea != applied || Screen.width != appliedScreen.x || Screen.height != appliedScreen.y)
            {
                Apply();
            }
        }

        private void Apply()
        {
            applied = Screen.safeArea;
            appliedScreen = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            var rect = (RectTransform)transform;
            rect.anchorMin = new Vector2(applied.xMin / Screen.width, applied.yMin / Screen.height);
            rect.anchorMax = new Vector2(applied.xMax / Screen.width, applied.yMax / Screen.height);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
