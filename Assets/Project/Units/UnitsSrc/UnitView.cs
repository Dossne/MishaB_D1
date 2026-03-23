using UnityEngine;

namespace TetrisTactic.Units
{
    public sealed class UnitView : MonoBehaviour
    {
        private const float UnitScaleFactor = 0.68f;
        private const float ReferenceCellSizePixels = 180f;
        private const float CornerInsetPixels = 12f;

        [SerializeField] private SpriteRenderer spriteRenderer;

        private static Sprite cachedSprite;
        private Transform labelRoot;

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

            var unitScale = Mathf.Max(0.01f, cellWorldSize * UnitScaleFactor);
            transform.localScale = Vector3.one * unitScale;
            name = model.UnitType + "_View";

            EnsureLabelRoot(unitScale);
            CreateOrRefreshUnitLabel(model.UnitType, cellWorldSize);
            CreateOrRefreshHpLabel(model, cellWorldSize);
            CreateOrRefreshDamageLabel(model, cellWorldSize);
        }

        private void EnsureLabelRoot(float unitScale)
        {
            const string labelRootName = "LabelRoot";
            if (labelRoot == null)
            {
                labelRoot = transform.Find(labelRootName);
            }

            if (labelRoot == null)
            {
                var labelRootObject = new GameObject(labelRootName);
                labelRoot = labelRootObject.transform;
                labelRoot.SetParent(transform, false);
            }

            var inverseScale = 1f / Mathf.Max(0.01f, unitScale);
            labelRoot.localScale = new Vector3(inverseScale, inverseScale, 1f);
            labelRoot.localPosition = Vector3.zero;
            labelRoot.localRotation = Quaternion.identity;
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
            var label = CreateOrGetStatLabel("HpLabel");
            if (label == null)
            {
                return;
            }

            var labelTransform = label.transform;
            labelTransform.localPosition = GetCornerLocalPosition(cellWorldSize, CornerInsetPixels, isTop: false);
            labelTransform.localRotation = Quaternion.identity;

            label.text = $"{model.Health.CurrentHp}/{model.Health.MaxHp}";
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = Mathf.Max(0.03f, cellWorldSize * 0.08f);
            label.fontSize = 44;
            label.color = Color.white;

            var renderer = label.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 21;
            }
        }

        private void CreateOrRefreshDamageLabel(UnitRuntimeModel model, float cellWorldSize)
        {
            var label = CreateOrGetStatLabel("DamageLabel");
            if (label == null)
            {
                return;
            }

            var labelTransform = label.transform;
            labelTransform.localPosition = GetCornerLocalPosition(cellWorldSize, CornerInsetPixels, isTop: true);
            labelTransform.localRotation = Quaternion.identity;

            label.text = model.BaseDamage.ToString();
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = Mathf.Max(0.03f, cellWorldSize * 0.08f);
            label.fontSize = 44;
            label.color = Color.white;

            var renderer = label.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 22;
            }
        }

        private TextMesh CreateOrGetStatLabel(string labelObjectName)
        {
            if (labelRoot == null)
            {
                return null;
            }

            var labelTransform = labelRoot.Find(labelObjectName);
            TextMesh label;

            if (labelTransform == null)
            {
                var labelObject = new GameObject(labelObjectName, typeof(TextMesh));
                labelObject.transform.SetParent(labelRoot, false);
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

            return label;
        }

        private static Vector3 GetCornerLocalPosition(float cellWorldSize, float insetPixels, bool isTop)
        {
            var insetWorld = (insetPixels / Mathf.Max(1f, ReferenceCellSizePixels)) * cellWorldSize;
            var edgeOffset = (cellWorldSize * 0.5f) - insetWorld;
            var y = isTop ? edgeOffset : -edgeOffset;
            return new Vector3(-edgeOffset, y, -0.12f);
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
