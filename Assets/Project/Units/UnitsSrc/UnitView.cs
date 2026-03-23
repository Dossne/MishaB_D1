using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TetrisTactic.Units
{
    public sealed class UnitView : MonoBehaviour
    {
        private const float UnitFillRatio = 0.82f;
        private const float ReferenceCellSizePixels = 180f;
        private const float CornerInsetPixels = 12f;

        [SerializeField] private SpriteRenderer spriteRenderer;

        private static Sprite fallbackSprite;
        private static readonly Dictionary<UnitType, Sprite> UnitSpriteCache = new();
        private Transform labelRoot;

        public void Initialize(UnitRuntimeModel model, float cellWorldSize, Color tint)
        {
            _ = tint;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            var sprite = ResolveUnitSprite(model.UnitType);
            if (sprite == null)
            {
                fallbackSprite ??= CreateFallbackSprite();
                sprite = fallbackSprite;
            }

            spriteRenderer.sprite = sprite;
            spriteRenderer.color = Color.white;
            spriteRenderer.sortingOrder = 10;

            var unitScale = CalculateScaleToFitCell(sprite, cellWorldSize);
            transform.localScale = Vector3.one * unitScale;
            name = model.UnitType + "_View";

            EnsureLabelRoot(unitScale);
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

        private static Sprite ResolveUnitSprite(UnitType unitType)
        {
            if (UnitSpriteCache.TryGetValue(unitType, out var cached) && cached != null)
            {
                return cached;
            }

            var fileName = unitType switch
            {
                UnitType.Player => "player",
                UnitType.Warrior => "warrior",
                UnitType.Archer => "archer",
                UnitType.Mage => "mage",
                _ => string.Empty,
            };

            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            var sprite = Resources.Load<Sprite>($"Project/Units/UnitsArt/{fileName}");

#if UNITY_EDITOR
            if (sprite == null)
            {
                var editorPath = $"Assets/Project/Units/UnitsArt/{fileName}.png";
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
            }
#endif

            UnitSpriteCache[unitType] = sprite;
            return sprite;
        }

        private static float CalculateScaleToFitCell(Sprite sprite, float cellWorldSize)
        {
            var safeCellSize = Mathf.Max(0.01f, cellWorldSize);
            var targetMaxSize = safeCellSize * UnitFillRatio;

            if (sprite == null)
            {
                return targetMaxSize;
            }

            var sourceSize = sprite.bounds.size;
            var sourceMaxSize = Mathf.Max(sourceSize.x, sourceSize.y);
            if (sourceMaxSize <= 0.0001f)
            {
                return targetMaxSize;
            }

            return targetMaxSize / sourceMaxSize;
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
