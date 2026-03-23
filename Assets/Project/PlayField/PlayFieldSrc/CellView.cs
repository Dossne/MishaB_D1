using UnityEngine;

namespace TetrisTactic.PlayField
{
    public sealed class CellView : MonoBehaviour
    {
        public event System.Action<GridPosition> Clicked;

        [SerializeField] private SpriteRenderer spriteRenderer;

        private GridPosition position;
        private Color baseColor = Color.white;
        private bool hasHighlight;
        private Color highlightColor = Color.white;

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
            spriteRenderer.sortingOrder = 0;
            transform.localScale = new Vector3(worldSize, worldSize, 1f);

            var collider = GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider2D>();
            }

            collider.size = Vector2.one;
            ApplyVisualColor();
        }

        public void SetBaseColor(Color color)
        {
            baseColor = color;
            ApplyVisualColor();
        }

        public void SetHighlight(bool enabled, Color color)
        {
            hasHighlight = enabled;
            highlightColor = color;
            ApplyVisualColor();
        }

        public void NotifyPointerTap()
        {
            Clicked?.Invoke(position);
        }

        private void ApplyVisualColor()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.color = hasHighlight ? highlightColor : baseColor;
        }
    }
}
