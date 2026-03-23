using UnityEngine;

namespace TetrisTactic.Units
{
    public sealed class UnitView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        private static Sprite cachedSprite;

        public void Initialize(UnitRuntimeModel model, float cellWorldSize, Color tint)
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
            spriteRenderer.sortingOrder = 10;
            transform.localScale = Vector3.one * (cellWorldSize * 0.68f);
            name = model.UnitType + "_View";

            CreateOrRefreshUnitLabel(model.UnitType, cellWorldSize);
            CreateOrRefreshHpLabel(model, cellWorldSize);
        }

        private void CreateOrRefreshUnitLabel(UnitType unitType, float cellWorldSize)
        {
            const string labelObjectName = "UnitLabel";
            var labelTransform = transform.Find(labelObjectName);
            TextMesh label;

            if (labelTransform == null)
            {
                var labelObject = new GameObject(labelObjectName, typeof(TextMesh));
                labelObject.transform.SetParent(transform, false);
                labelObject.transform.localPosition = new Vector3(0f, 0f, -0.1f);
                label = labelObject.GetComponent<TextMesh>();
            }
            else
            {
                label = labelTransform.GetComponent<TextMesh>();
                if (label == null)
                {
                    label = labelTransform.gameObject.AddComponent<TextMesh>();
                }
            }

            label.text = GetUnitLetter(unitType);
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = Mathf.Max(0.05f, cellWorldSize * 0.18f);
            label.fontSize = 64;
            label.color = Color.white;

            var renderer = label.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 20;
            }
        }

        private void CreateOrRefreshHpLabel(UnitRuntimeModel model, float cellWorldSize)
        {
            const string labelObjectName = "HpLabel";
            var labelTransform = transform.Find(labelObjectName);
            TextMesh label;

            if (labelTransform == null)
            {
                var labelObject = new GameObject(labelObjectName, typeof(TextMesh));
                labelObject.transform.SetParent(transform, false);
                labelTransform = labelObject.transform;
                label = labelObject.GetComponent<TextMesh>();
            }
            else
            {
                label = labelTransform.GetComponent<TextMesh>();
                if (label == null)
                {
                    label = labelTransform.gameObject.AddComponent<TextMesh>();
                }
            }

            var halfUnitSize = cellWorldSize * 0.34f;
            var cornerInset = cellWorldSize * 0.04f;
            labelTransform.localPosition = new Vector3(-halfUnitSize + cornerInset, -halfUnitSize + cornerInset, -0.12f);
            label.text = $"{model.Health.CurrentHp}/{model.Health.MaxHp}";
            label.anchor = TextAnchor.LowerLeft;
            label.alignment = TextAlignment.Left;
            label.characterSize = Mathf.Max(0.03f, cellWorldSize * 0.08f);
            label.fontSize = 44;
            label.color = Color.white;

            var renderer = label.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 21;
            }
        }

        private static string GetUnitLetter(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Player => "P",
                UnitType.Warrior => "W",
                UnitType.Archer => "A",
                UnitType.Mage => "M",
                _ => "?",
            };
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