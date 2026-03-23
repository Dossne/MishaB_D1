using System.Collections.Generic;
using TetrisTactic.Abilities;
using TetrisTactic.Treasure;
using TetrisTactic.Units;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TetrisTactic.PlayField
{
    public sealed class PlayFieldView : MonoBehaviour
    {
        private const float BoardFitWidthPaddingWorld = 0.2f;
        private const float ZonePreviewMinAlpha = 0.62f;
        private const float ZonePreviewMaxAlpha = 0.86f;
        private const float ZonePulseSpeed = 4.6f;
        private const float ZonePulseScaleFactor = 0.06f;
        private const float ZoneFillRatio = 0.53f;
        private const int ZoneSortingOrder = 12;

        private const int MovePreviewSortingOrder = 9;
        private const float MovePreviewMinAlpha = 0.62f;
        private const float MovePreviewMaxAlpha = 0.88f;
        private const float MovePulseScaleFactor = 0.08f;
        private const float MovePreviewFillRatio = 0.5f;

        public event System.Action<GridPosition> CellTapped;

        [SerializeField] private Transform cellRoot;
        [SerializeField] private Transform zoneRoot;
        [SerializeField] private Transform contentRoot;

        private readonly Dictionary<GridPosition, CellView> cellViews = new();
        private readonly Dictionary<GridPosition, SpriteRenderer> zoneOverlayViews = new();
        private readonly Dictionary<GridPosition, SpriteRenderer> moveOverlayViews = new();
        private readonly Dictionary<GridPosition, float> zoneOverlayBaseScales = new();
        private readonly List<GameObject> spawnedContentObjects = new();
        private readonly HashSet<GridPosition> moveHighlightedCells = new();
        private readonly Dictionary<GridPosition, ZoneHighlightData> abilityHighlightedZones = new();
        private readonly Dictionary<GridPosition, ZoneHighlightData> enemyDangerHighlightedZones = new();

        private PlayFieldConfig playFieldConfig;
        private Sprite defaultCellSprite;
        private Sprite grassCellSprite;
        private Sprite obstacleCellSprite;
        private Sprite treasureCellSprite;
        private Sprite movePreviewSprite;
        private int currentColumns;
        private int currentRows;

        private void Update()
        {
            if (cellViews.Count == 0)
            {
                return;
            }

            AnimateMoveOverlaysPulse();
            AnimateZoneOverlaysPulse();

            if (!TryGetPointerDownWorldPosition(out var worldPosition))
            {
                return;
            }

            var hit = Physics2D.OverlapPoint(worldPosition);
            if (hit == null)
            {
                return;
            }

            var cellView = hit.GetComponent<CellView>();
            if (cellView == null)
            {
                return;
            }

            cellView.NotifyPointerTap();
        }

        public void Initialize(PlayFieldConfig config)
        {
            playFieldConfig = config;
            EnsureRoots();

            if (defaultCellSprite == null)
            {
                defaultCellSprite = CreateDefaultSprite();
            }

            EnsureArtSpritesLoaded();
        }

        public void Render(PlayFieldModel model)
        {
            if (model == null)
            {
                return;
            }

            if (playFieldConfig == null)
            {
                playFieldConfig = PlayFieldConfig.CreateDefault();
            }

            EnsureRoots();

            if (defaultCellSprite == null)
            {
                defaultCellSprite = CreateDefaultSprite();
            }

            EnsureArtSpritesLoaded();
            EnsureBoardFitsScreenWidth(model.Columns);

            if (model.Columns != currentColumns || model.Rows != currentRows)
            {
                RebuildGrid(model.Columns, model.Rows);
            }

            for (var x = 0; x < model.Columns; x++)
            {
                for (var y = 0; y < model.Rows; y++)
                {
                    var position = new GridPosition(x, y);
                    if (cellViews.TryGetValue(position, out var view))
                    {
                        view.SetBaseColor(GetCellColor(model.GetCell(position)));
                    }
                }
            }

            ApplyHighlights();
            RenderContent(model);
        }

        public void Clear()
        {
            foreach (var pair in cellViews)
            {
                if (pair.Value != null)
                {
                    pair.Value.Clicked -= OnCellClicked;
                    Destroy(pair.Value.gameObject);
                }
            }

            foreach (var overlay in zoneOverlayViews.Values)
            {
                if (overlay != null)
                {
                    Destroy(overlay.gameObject);
                }
            }

            foreach (var overlay in moveOverlayViews.Values)
            {
                if (overlay != null)
                {
                    Destroy(overlay.gameObject);
                }
            }

            cellViews.Clear();
            zoneOverlayViews.Clear();
            moveOverlayViews.Clear();
            zoneOverlayBaseScales.Clear();
            moveHighlightedCells.Clear();
            abilityHighlightedZones.Clear();
            enemyDangerHighlightedZones.Clear();
            ClearContent();
            currentColumns = 0;
            currentRows = 0;
        }

        public void SetMoveHighlights(IReadOnlyList<GridPosition> positions)
        {
            moveHighlightedCells.Clear();
            if (positions != null)
            {
                for (var i = 0; i < positions.Count; i++)
                {
                    moveHighlightedCells.Add(positions[i]);
                }
            }

            ApplyHighlights();
        }

        public void ClearMoveHighlights()
        {
            if (moveHighlightedCells.Count == 0)
            {
                return;
            }

            moveHighlightedCells.Clear();
            ApplyHighlights();
        }

        public void SetAbilityHighlights(IReadOnlyList<GridPosition> positions)
        {
            abilityHighlightedZones.Clear();
            if (positions != null)
            {
                for (var i = 0; i < positions.Count; i++)
                {
                    var position = positions[i];
                    abilityHighlightedZones[position] = new ZoneHighlightData(position, UnitType.Player, -1);
                }
            }

            ApplyHighlights();
        }

        public void SetAbilityHighlights(IReadOnlyList<ZoneHighlightData> zones)
        {
            abilityHighlightedZones.Clear();
            if (zones != null)
            {
                for (var i = 0; i < zones.Count; i++)
                {
                    var zone = zones[i];
                    if (zone == null)
                    {
                        continue;
                    }

                    abilityHighlightedZones[zone.Position] = zone;
                }
            }

            ApplyHighlights();
        }

        public void ClearAbilityHighlights()
        {
            if (abilityHighlightedZones.Count == 0)
            {
                return;
            }

            abilityHighlightedZones.Clear();
            ApplyHighlights();
        }

        public void SetEnemyDangerHighlights(IReadOnlyList<GridPosition> positions)
        {
            enemyDangerHighlightedZones.Clear();
            if (positions != null)
            {
                for (var i = 0; i < positions.Count; i++)
                {
                    var position = positions[i];
                    enemyDangerHighlightedZones[position] = new ZoneHighlightData(position, UnitType.Warrior, 1);
                }
            }

            ApplyHighlights();
        }

        public void SetEnemyDangerHighlights(IReadOnlyList<ZoneHighlightData> zones)
        {
            enemyDangerHighlightedZones.Clear();
            if (zones != null)
            {
                for (var i = 0; i < zones.Count; i++)
                {
                    var zone = zones[i];
                    if (zone == null)
                    {
                        continue;
                    }

                    enemyDangerHighlightedZones[zone.Position] = zone;
                }
            }

            ApplyHighlights();
        }

        public void ClearEnemyDangerHighlights()
        {
            if (enemyDangerHighlightedZones.Count == 0)
            {
                return;
            }

            enemyDangerHighlightedZones.Clear();
            ApplyHighlights();
        }

        public bool TryGetCellWorldPosition(GridPosition position, out Vector3 worldPosition)
        {
            if (!cellViews.TryGetValue(position, out var cellView) || cellView == null)
            {
                worldPosition = Vector3.zero;
                return false;
            }

            worldPosition = cellView.transform.position;
            return true;
        }

        private void EnsureBoardFitsScreenWidth(int columns)
        {
            if (playFieldConfig == null || columns <= 0)
            {
                return;
            }

            var cameraForFit = Camera.main;
            if (cameraForFit == null || !cameraForFit.orthographic)
            {
                return;
            }

            var boardWidthWorld = columns * playFieldConfig.CellWorldSize;
            var requiredHalfWidthWorld = (boardWidthWorld * 0.5f) + BoardFitWidthPaddingWorld;
            var aspect = Mathf.Max(0.01f, cameraForFit.aspect);
            var requiredOrthoSizeForWidth = requiredHalfWidthWorld / aspect;

            if (cameraForFit.orthographicSize < requiredOrthoSizeForWidth)
            {
                cameraForFit.orthographicSize = requiredOrthoSizeForWidth;
            }
        }

        private void OnDestroy()
        {
            foreach (var pair in cellViews)
            {
                if (pair.Value != null)
                {
                    pair.Value.Clicked -= OnCellClicked;
                }
            }
        }

        private void RebuildGrid(int columns, int rows)
        {
            Clear();

            currentColumns = columns;
            currentRows = rows;

            var backgroundSprite = grassCellSprite != null ? grassCellSprite : defaultCellSprite;

            for (var x = 0; x < columns; x++)
            {
                for (var y = 0; y < rows; y++)
                {
                    var position = new GridPosition(x, y);
                    var cellObject = new GameObject($"Cell_{x}_{y}", typeof(CellView));
                    cellObject.transform.SetParent(cellRoot, false);
                    cellObject.transform.localPosition = GetCellLocalPosition(x, y, columns, rows);

                    var cellView = cellObject.GetComponent<CellView>();
                    cellView.Initialize(position, playFieldConfig.CellWorldSize, backgroundSprite);
                    cellView.Clicked += OnCellClicked;

                    cellViews[position] = cellView;
                }
            }
        }

        private void RenderContent(PlayFieldModel model)
        {
            ClearContent();

            foreach (var obstaclePosition in model.Obstacles)
            {
                var obstacleObject = CreateContentObject($"Obstacle_{obstaclePosition.X}_{obstaclePosition.Y}", obstaclePosition);
                var spriteRenderer = obstacleObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = obstacleCellSprite != null ? obstacleCellSprite : defaultCellSprite;
                spriteRenderer.color = Color.white;
                spriteRenderer.sortingOrder = 5;
                obstacleObject.transform.localScale = Vector3.one * CalculateSpriteScale(spriteRenderer.sprite, playFieldConfig.CellWorldSize * 0.62f);
            }

            var treasures = model.Treasures;
            for (var i = 0; i < treasures.Count; i++)
            {
                var treasure = treasures[i];
                var treasureObject = CreateContentObject($"Treasure_{treasure.Position.X}_{treasure.Position.Y}", treasure.Position);
                var treasureView = treasureObject.AddComponent<TreasureView>();
                treasureView.Initialize(treasure, playFieldConfig.CellWorldSize, Color.white, treasureCellSprite);
            }

            foreach (var unit in model.GetAllUnits())
            {
                var unitObject = CreateContentObject($"{unit.UnitType}_{unit.Position.X}_{unit.Position.Y}", unit.Position);
                var unitView = unitObject.AddComponent<UnitView>();
                unitView.Initialize(unit, playFieldConfig.CellWorldSize, GetUnitColor(unit));
            }
        }

        private GameObject CreateContentObject(string name, GridPosition position)
        {
            var contentObject = new GameObject(name);
            contentObject.transform.SetParent(contentRoot, false);
            contentObject.transform.localPosition = GetCellLocalPosition(position.X, position.Y, currentColumns, currentRows);
            spawnedContentObjects.Add(contentObject);
            return contentObject;
        }

        private void ClearContent()
        {
            for (var i = 0; i < spawnedContentObjects.Count; i++)
            {
                if (spawnedContentObjects[i] != null)
                {
                    Destroy(spawnedContentObjects[i]);
                }
            }

            spawnedContentObjects.Clear();
        }

        private Color GetUnitColor(UnitRuntimeModel unit)
        {
            return unit.TeamType == TeamType.Player
                ? playFieldConfig.PlayerCellColor
                : playFieldConfig.EnemyCellColor;
        }

        private Vector3 GetCellLocalPosition(int x, int y, int columns, int rows)
        {
            var centerOffsetX = (columns - 1) * 0.5f;
            var centerOffsetY = (rows - 1) * 0.5f;

            return new Vector3(
                (x - centerOffsetX) * playFieldConfig.CellWorldSize,
                (centerOffsetY - y) * playFieldConfig.CellWorldSize,
                0f);
        }

        private void EnsureRoots()
        {
            if (cellRoot == null)
            {
                var cellRootObject = new GameObject("CellRoot");
                cellRoot = cellRootObject.transform;
                cellRoot.SetParent(transform, false);
            }

            if (zoneRoot == null)
            {
                var zoneRootObject = new GameObject("ZoneRoot");
                zoneRoot = zoneRootObject.transform;
                zoneRoot.SetParent(transform, false);
            }

            if (contentRoot == null)
            {
                var contentRootObject = new GameObject("ContentRoot");
                contentRoot = contentRootObject.transform;
                contentRoot.SetParent(transform, false);
            }
        }

        private void EnsureArtSpritesLoaded()
        {
            grassCellSprite ??= LoadPlayFieldArtSprite("grass");
            obstacleCellSprite ??= LoadPlayFieldArtSprite("obstacle");
            treasureCellSprite ??= LoadPlayFieldArtSprite("treasure");
            movePreviewSprite ??= LoadPlayerTurnArtSprite("move");
        }

        private static Sprite LoadPlayFieldArtSprite(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            var sprite = Resources.Load<Sprite>($"Project/PlayField/PlayFieldArt/{fileName}");

#if UNITY_EDITOR
            if (sprite == null)
            {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Project/PlayField/PlayFieldArt/{fileName}.png");
            }
#endif

            return sprite;
        }

        private static Sprite LoadPlayerTurnArtSprite(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            var sprite = Resources.Load<Sprite>($"Project/PlayerTurn/PlayerTurnArt/{fileName}");

#if UNITY_EDITOR
            if (sprite == null)
            {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Project/PlayerTurn/PlayerTurnArt/{fileName}.png");
            }
#endif

            return sprite;
        }

        private static float CalculateSpriteScale(Sprite sprite, float targetSize)
        {
            var safeTarget = Mathf.Max(0.01f, targetSize);
            if (sprite == null)
            {
                return safeTarget;
            }

            var sourceSize = sprite.bounds.size;
            var sourceMax = Mathf.Max(sourceSize.x, sourceSize.y);
            if (sourceMax <= 0.0001f)
            {
                return safeTarget;
            }

            return safeTarget / sourceMax;
        }

        private void OnCellClicked(GridPosition position)
        {
            CellTapped?.Invoke(position);
        }

        private void ApplyHighlights()
        {
            foreach (var pair in cellViews)
            {
                pair.Value.SetHighlight(false, Color.white);
            }

            ApplyMoveOverlays();
            ApplyZoneOverlays();
        }

        private void ApplyMoveOverlays()
        {
            var stalePositions = new List<GridPosition>();
            foreach (var existing in moveOverlayViews.Keys)
            {
                if (!moveHighlightedCells.Contains(existing))
                {
                    stalePositions.Add(existing);
                }
            }

            for (var i = 0; i < stalePositions.Count; i++)
            {
                var position = stalePositions[i];
                if (moveOverlayViews.TryGetValue(position, out var renderer) && renderer != null)
                {
                    Destroy(renderer.gameObject);
                }

                moveOverlayViews.Remove(position);
            }

            foreach (var position in moveHighlightedCells)
            {
                if (!cellViews.TryGetValue(position, out var cellView) || cellView == null)
                {
                    continue;
                }

                if (!moveOverlayViews.TryGetValue(position, out var overlay) || overlay == null)
                {
                    var overlayObject = new GameObject($"Move_{position.X}_{position.Y}", typeof(SpriteRenderer));
                    overlayObject.transform.SetParent(zoneRoot, false);
                    overlay = overlayObject.GetComponent<SpriteRenderer>();
                    moveOverlayViews[position] = overlay;
                }

                overlay.sprite = movePreviewSprite != null ? movePreviewSprite : defaultCellSprite;
                overlay.sortingOrder = MovePreviewSortingOrder;
                var color = Color.white;
                color.a = MovePreviewMaxAlpha;
                overlay.color = color;
                overlay.transform.localPosition = cellView.transform.localPosition;
                overlay.transform.localScale = Vector3.one * CalculateMovePreviewScale(overlay.sprite);
            }
        }

        private float CalculateMovePreviewScale(Sprite sprite)
        {
            var targetSize = playFieldConfig.CellWorldSize * MovePreviewFillRatio;
            if (sprite == null)
            {
                return targetSize;
            }

            var sourceSize = sprite.bounds.size;
            var sourceMaxSize = Mathf.Max(sourceSize.x, sourceSize.y);
            if (sourceMaxSize <= 0.0001f)
            {
                return targetSize;
            }

            return targetSize / sourceMaxSize;
        }

        private void ApplyZoneOverlays()
        {
            var finalZones = new Dictionary<GridPosition, ZoneHighlightData>();

            foreach (var enemyPair in enemyDangerHighlightedZones)
            {
                finalZones[enemyPair.Key] = enemyPair.Value;
            }

            foreach (var abilityPair in abilityHighlightedZones)
            {
                if (finalZones.TryGetValue(abilityPair.Key, out var existing))
                {
                    if (abilityPair.Value.Priority < existing.Priority)
                    {
                        finalZones[abilityPair.Key] = abilityPair.Value;
                    }
                }
                else
                {
                    finalZones[abilityPair.Key] = abilityPair.Value;
                }
            }

            var stalePositions = new List<GridPosition>();
            foreach (var existing in zoneOverlayViews.Keys)
            {
                if (!finalZones.ContainsKey(existing))
                {
                    stalePositions.Add(existing);
                }
            }

            for (var i = 0; i < stalePositions.Count; i++)
            {
                var position = stalePositions[i];
                if (zoneOverlayViews.TryGetValue(position, out var renderer) && renderer != null)
                {
                    Destroy(renderer.gameObject);
                }

                zoneOverlayViews.Remove(position);
                zoneOverlayBaseScales.Remove(position);
            }

            foreach (var pair in finalZones)
            {
                if (!cellViews.TryGetValue(pair.Key, out var cellView) || cellView == null)
                {
                    continue;
                }

                if (!zoneOverlayViews.TryGetValue(pair.Key, out var overlay) || overlay == null)
                {
                    var overlayObject = new GameObject($"Zone_{pair.Key.X}_{pair.Key.Y}", typeof(SpriteRenderer));
                    overlayObject.transform.SetParent(zoneRoot, false);
                    overlay = overlayObject.GetComponent<SpriteRenderer>();
                    zoneOverlayViews[pair.Key] = overlay;
                }

                var icon = AbilityIconResolver.GetAbilityIcon(pair.Value.OwnerUnitType);
                if (icon == null)
                {
                    icon = defaultCellSprite;
                }

                overlay.sprite = icon;
                overlay.sortingOrder = ZoneSortingOrder;
                var color = Color.white;
                color.a = ZonePreviewMaxAlpha;
                overlay.color = color;
                overlay.transform.localPosition = cellView.transform.localPosition;
                var baseScale = CalculateZoneScale(icon);
                zoneOverlayBaseScales[pair.Key] = baseScale;
                overlay.transform.localScale = Vector3.one * baseScale;
            }
        }
        private void AnimateMoveOverlaysPulse()
        {
            if (moveOverlayViews.Count == 0)
            {
                return;
            }

            var pulse = (Mathf.Sin(Time.unscaledTime * ZonePulseSpeed) + 1f) * 0.5f;
            var alpha = Mathf.Lerp(MovePreviewMinAlpha, MovePreviewMaxAlpha, pulse);
            var scaleMul = Mathf.Lerp(1f - MovePulseScaleFactor, 1f + MovePulseScaleFactor, pulse);

            foreach (var pair in moveOverlayViews)
            {
                var renderer = pair.Value;
                if (renderer == null)
                {
                    continue;
                }

                var color = renderer.color;
                color.a = alpha;
                renderer.color = color;

                var sprite = renderer.sprite;
                var baseScale = CalculateMovePreviewScale(sprite);
                renderer.transform.localScale = Vector3.one * (baseScale * scaleMul);
            }
        }
        private void AnimateZoneOverlaysPulse()
        {
            if (zoneOverlayViews.Count == 0)
            {
                return;
            }

            var pulse = (Mathf.Sin(Time.unscaledTime * ZonePulseSpeed) + 1f) * 0.5f;
            var alpha = Mathf.Lerp(ZonePreviewMinAlpha, ZonePreviewMaxAlpha, pulse);
            var scaleMul = Mathf.Lerp(1f - ZonePulseScaleFactor, 1f + ZonePulseScaleFactor, pulse);

            foreach (var pair in zoneOverlayViews)
            {
                var position = pair.Key;
                var renderer = pair.Value;
                if (renderer == null)
                {
                    continue;
                }

                var color = renderer.color;
                color.a = alpha;
                renderer.color = color;

                if (zoneOverlayBaseScales.TryGetValue(position, out var baseScale))
                {
                    renderer.transform.localScale = Vector3.one * (baseScale * scaleMul);
                }
            }
        }

        private float CalculateZoneScale(Sprite sprite)
        {
            var targetSize = playFieldConfig.CellWorldSize * ZoneFillRatio;
            if (sprite == null)
            {
                return targetSize;
            }

            var sourceSize = sprite.bounds.size;
            var sourceMaxSize = Mathf.Max(sourceSize.x, sourceSize.y);
            if (sourceMaxSize <= 0.0001f)
            {
                return targetSize;
            }

            return targetSize / sourceMaxSize;
        }

        private static Color GetCellColor(CellContentType contentType)
        {
            _ = contentType;
            return Color.white;
        }

        private bool TryGetPointerDownWorldPosition(out Vector2 worldPosition)
        {
#if ENABLE_INPUT_SYSTEM
            var cameraForInput = Camera.main;
            if (cameraForInput == null)
            {
                worldPosition = Vector2.zero;
                return false;
            }

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                var screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                worldPosition = cameraForInput.ScreenToWorldPoint(screenPosition);
                return true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                var screenPosition = Mouse.current.position.ReadValue();
                worldPosition = cameraForInput.ScreenToWorldPoint(screenPosition);
                return true;
            }
#else
            var cameraForInput = Camera.main;
            if (cameraForInput == null)
            {
                worldPosition = Vector2.zero;
                return false;
            }

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    worldPosition = cameraForInput.ScreenToWorldPoint(touch.position);
                    return true;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                worldPosition = cameraForInput.ScreenToWorldPoint(Input.mousePosition);
                return true;
            }
#endif

            worldPosition = Vector2.zero;
            return false;
        }

        private static Sprite CreateDefaultSprite()
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            var color = Color.white;
            texture.SetPixel(0, 0, color);
            texture.SetPixel(1, 0, color);
            texture.SetPixel(0, 1, color);
            texture.SetPixel(1, 1, color);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 2f);
        }
    }
}





