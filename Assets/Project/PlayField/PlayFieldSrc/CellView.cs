using UnityEngine;

namespace TetrisTactic.PlayField
{
    public sealed class CellView : MonoBehaviour
    {
        public event System.Action<GridPosition> Clicked;

        [SerializeField] private SpriteRenderer spriteRenderer;

        private GridPosition position;

        public void Initialize(GridPosition gridPosition, float worldSize, Sprite sprite)
        {
            position = gridPosition;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = sprite;
            transform.localScale = new Vector3(worldSize, worldSize, 1f);

            var collider = GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider2D>();
            }

            collider.size = Vector2.one;
        }

        public void SetColor(Color color)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }

        public void NotifyPointerTap()
        {
            Clicked?.Invoke(position);
        }
    }
}
