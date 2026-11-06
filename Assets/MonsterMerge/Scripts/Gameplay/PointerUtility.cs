using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MonsterMerge
{
    public static class PointerUtility
    {
        private static readonly List<RaycastResult> Hits = new();

        /// <summary>
        /// Raycast trực tiếp thay vì IsPointerOverGameObject: kết quả đúng ngay trong frame bấm,
        /// không phụ thuộc thứ tự cập nhật của EventSystem với touch.
        /// </summary>
        public static bool IsOverUi(Vector2 screenPosition)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return false;
            }

            Hits.Clear();
            eventSystem.RaycastAll(new PointerEventData(eventSystem) { position = screenPosition }, Hits);
            return Hits.Count > 0;
        }

        public static Vector2 ToWorld(Camera camera, Vector2 screenPosition)
        {
            return camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -camera.transform.position.z));
        }
    }
}
