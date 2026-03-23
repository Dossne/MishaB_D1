using UnityEngine;

namespace TetrisTactic.Treasure
{
    public sealed class TreasureView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        private static Sprite fallbackSprite;

        public void Initialize(TreasureData treasureData, float cellWorldSize, Color tint, Sprite treasureSprite = null)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (fallbackSprite == null)
            {
                fallbackSprite = CreateFallbackSprite();
            }

            spriteRenderer.sprite = treasureSprite != null ? treasureSprite : fallbackSprite;
            spriteRenderer.color = tint;
            spriteRenderer.sortingOrder = 8;
            transform.localScale = Vector3.one * CalculateScale(spriteRenderer.sprite, cellWorldSize);
            transform.localRotation = Quaternion.identity;
            name = "Treasure_View";
        }

        private static float CalculateScale(Sprite sprite, float cellWorldSize)
        {
            var targetSize = cellWorldSize * 0.56f;
            if (sprite == null)
            {
                return targetSize;
            }

            var bounds = sprite.bounds.size;
            var maxSize = Mathf.Max(bounds.x, bounds.y);
            if (maxSize <= 0.0001f)
            {
                return targetSize;
            }

            return targetSize / maxSize;
        }

        private static Sprite CreateFallbackSprite()
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.SetPixel(1, 0, Color.white);
            texture.SetPixel(0, 1, Color.white);
            texture.SetPixel(1, 1, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 2f);
        }
    }
}
