using System.Collections.Generic;
using TetrisTactic.Core;
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
        private const float StatIconFillRatio = 0.48f;
        private const float StatOutlineOffsetRatio = 0.012f;

        [SerializeField] private SpriteRenderer spriteRenderer;

        private static Sprite fallbackSprite;
        private static readonly Dictionary<UnitType, Sprite> UnitSpriteCache = new();
        private static readonly Dictionary<string, Sprite> StatSpriteCache = new();
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

            var cornerPosition = GetCornerLocalPosition(cellWorldSize, CornerInsetPixels, isTop: false);
            var labelTransform = label.transform;
            labelTransform.localPosition = cornerPosition;
            labelTransform.localRotation = Quaternion.identity;

            GameTextStyling.SetWorldText(label, model.Health.CurrentHp.ToString());
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = Mathf.Max(0.04f, cellWorldSize * 0.1f);
            label.fontSize = 28;
            label.color = Color.white;

            var renderer = label.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 31;
            }

            CreateOrRefreshStatIcon(
                "HpIconBg",
                ResolveStatSprite("health", "Assets/Project/Abilities/AbilitiesArt/health.png"),
                cornerPosition,
                cellWorldSize,
                sortingOrder: 20);

            CreateOrRefreshStatOutline("HpLabelOutline", label, cellWorldSize, sortingOrder: 30);
        }

        private void CreateOrRefreshDamageLabel(UnitRuntimeModel model, float cellWorldSize)
        {
            var label = CreateOrGetStatLabel("DamageLabel");
            if (label == null)
            {
                return;
            }

            var cornerPosition = GetCornerLocalPosition(cellWorldSize, CornerInsetPixels, isTop: true);
            var labelTransform = label.transform;
            labelTransform.localPosition = cornerPosition;
            labelTransform.localRotation = Quaternion.identity;

            GameTextStyling.SetWorldText(label, model.BaseDamage.ToString());
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = Mathf.Max(0.04f, cellWorldSize * 0.1f);
            label.fontSize = 28;
            label.color = new Color(1f, 0.9f, 0.2f, 1f);

            var renderer = label.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 32;
            }

            CreateOrRefreshStatIcon(
                "DamageIconBg",
                ResolveStatSprite("damage", "Assets/Project/Abilities/AbilitiesArt/damage.png"),
                cornerPosition,
                cellWorldSize,
                sortingOrder: 21);

            CreateOrRefreshStatOutline("DamageLabelOutline", label, cellWorldSize, sortingOrder: 31);
        }

        private void CreateOrRefreshStatIcon(string objectName, Sprite iconSprite, Vector3 localPosition, float cellWorldSize, int sortingOrder)
        {
            if (labelRoot == null || iconSprite == null)
            {
                return;
            }

            var iconTransform = labelRoot.Find(objectName);
            SpriteRenderer iconRenderer;
            if (iconTransform == null)
            {
                var iconObject = new GameObject(objectName);
                iconObject.transform.SetParent(labelRoot, false);
                iconRenderer = iconObject.AddComponent<SpriteRenderer>();
            }
            else
            {
                iconRenderer = iconTransform.GetComponent<SpriteRenderer>();
                if (iconRenderer == null)
                {
                    iconRenderer = iconTransform.gameObject.AddComponent<SpriteRenderer>();
                }
            }

            iconRenderer.sprite = iconSprite;
            iconRenderer.color = Color.white;
            iconRenderer.sortingOrder = sortingOrder;

            var transformRef = iconRenderer.transform;
            transformRef.localPosition = new Vector3(localPosition.x, localPosition.y, -0.13f);
            transformRef.localRotation = Quaternion.identity;
            transformRef.localScale = Vector3.one * CalculateStatIconScale(iconSprite, cellWorldSize);
        }


        private void CreateOrRefreshStatOutline(string outlineNamePrefix, TextMesh sourceLabel, float cellWorldSize, int sortingOrder)
        {
            if (labelRoot == null || sourceLabel == null)
            {
                return;
            }

            var offset = Mathf.Max(0.0015f, cellWorldSize * StatOutlineOffsetRatio);
            var basePosition = sourceLabel.transform.localPosition;
            var font = sourceLabel.font;

            var offsets = new[]
            {
                new Vector3(-offset, 0f, 0f),
                new Vector3(offset, 0f, 0f),
                new Vector3(0f, -offset, 0f),
                new Vector3(0f, offset, 0f),
            };

            for (var i = 0; i < offsets.Length; i++)
            {
                var outline = CreateOrGetOutlineLabel($"{outlineNamePrefix}_{i}");
                if (outline == null)
                {
                    continue;
                }

                outline.text = sourceLabel.text;
                outline.anchor = sourceLabel.anchor;
                outline.alignment = sourceLabel.alignment;
                outline.characterSize = sourceLabel.characterSize;
                outline.fontSize = sourceLabel.fontSize;
                outline.color = Color.black;
                outline.font = font;

                var outlineTransform = outline.transform;
                outlineTransform.localPosition = basePosition + offsets[i] + new Vector3(0f, 0f, 0.0005f);
                outlineTransform.localRotation = Quaternion.identity;

                var outlineRenderer = outline.GetComponent<MeshRenderer>();
                if (outlineRenderer != null)
                {
                    outlineRenderer.sortingOrder = sortingOrder;
                    if (font != null && font.material != null)
                    {
                        outlineRenderer.sharedMaterial = font.material;
                    }
                }
            }
        }

        private TextMesh CreateOrGetOutlineLabel(string outlineObjectName)
        {
            if (labelRoot == null)
            {
                return null;
            }

            var transformRef = labelRoot.Find(outlineObjectName);
            TextMesh label;
            if (transformRef == null)
            {
                var labelObject = new GameObject(outlineObjectName, typeof(TextMesh));
                labelObject.transform.SetParent(labelRoot, false);
                label = labelObject.GetComponent<TextMesh>();
            }
            else
            {
                label = transformRef.GetComponent<TextMesh>();
                if (label == null)
                {
                    label = transformRef.gameObject.AddComponent<TextMesh>();
                }
            }

            return label;
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

            label.font = LoadGameFont();
            var meshRenderer = label.GetComponent<MeshRenderer>();
            if (meshRenderer != null && label.font != null && label.font.material != null)
            {
                meshRenderer.sharedMaterial = label.font.material;
            }

            return label;
        }

        private static Font LoadGameFont()
        {
            var font = Resources.Load<Font>("bangerscyrillic");
            if (FontSupportsStatDigits(font))
            {
                return font;
            }

            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (FontSupportsStatDigits(font))
            {
                return font;
            }

            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static bool FontSupportsStatDigits(Font font)
        {
            if (font == null)
            {
                return false;
            }

            return font.HasCharacter('0')
                && font.HasCharacter('1')
                && font.HasCharacter('2')
                && font.HasCharacter('3')
                && font.HasCharacter('4')
                && font.HasCharacter('5')
                && font.HasCharacter('6')
                && font.HasCharacter('7')
                && font.HasCharacter('8')
                && font.HasCharacter('9');
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

        private static Sprite ResolveStatSprite(string name, string editorPath)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (StatSpriteCache.TryGetValue(name, out var cached) && cached != null)
            {
                return cached;
            }

            var sprite = Resources.Load<Sprite>($"Project/Abilities/AbilitiesArt/{name}");
#if UNITY_EDITOR
            if (sprite == null && !string.IsNullOrEmpty(editorPath))
            {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
            }
#endif

            StatSpriteCache[name] = sprite;
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

        private static float CalculateStatIconScale(Sprite sprite, float cellWorldSize)
        {
            var safeCellSize = Mathf.Max(0.01f, cellWorldSize);
            var targetMaxSize = safeCellSize * StatIconFillRatio;
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

