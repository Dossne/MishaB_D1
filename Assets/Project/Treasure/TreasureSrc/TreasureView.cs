using UnityEngine;
namespace TetrisTactic.Treasure
{
    public sealed class TreasureView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        private static Sprite cachedSprite;
        public void Initialize(TreasureData treasureData, float cellWorldSize, Color tint)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            if (cachedSprite == null)
            {
                cachedSprite = CreateSprite();
            }
            spriteRenderer.sprite = cachedSprite;
            spriteRenderer.color = tint;
            spriteRenderer.sortingOrder = 8;
            transform.localScale = Vector3.one * (cellWorldSize * 0.42f);
            transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            name = "Treasure_View";
        }
        private static Sprite CreateSprite()
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
