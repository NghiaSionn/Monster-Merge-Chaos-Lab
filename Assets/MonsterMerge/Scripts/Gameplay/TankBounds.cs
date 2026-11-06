using UnityEngine;

namespace MonsterMerge
{
    /// <summary>Nguồn số liệu duy nhất về hình học bể, dùng chung cho thả, hợp nhất và tràn.</summary>
    public sealed class TankBounds : MonoBehaviour
    {
        [SerializeField] private float innerLeft = -2.2f;
        [SerializeField] private float innerRight = 2.2f;
        [SerializeField] private float floorY = -3.3f;
        [SerializeField] private float dangerY = 1.9f;
        [SerializeField] private float spawnY = 2.7f;

        public float InnerLeft => innerLeft;
        public float InnerRight => innerRight;
        public float FloorY => floorY;
        public float DangerY => dangerY;
        public float SpawnY => spawnY;

        public float ClampX(float x, float radius)
        {
            return Mathf.Clamp(x, innerLeft + radius, innerRight - radius);
        }

        public Vector2 ClampInside(Vector2 position, float radius)
        {
            return new Vector2(ClampX(position.x, radius), Mathf.Max(position.y, floorY + radius));
        }
    }
}
