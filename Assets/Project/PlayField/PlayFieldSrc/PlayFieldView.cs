using System.Collections.Generic;
using TetrisTactic.Treasure;
using TetrisTactic.Units;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TetrisTactic.PlayField
{
    public sealed class PlayFieldView : MonoBehaviour
    {
        public event System.Action<GridPosition> CellTapped;

        [SerializeField] private Transform cellRoot;
        [SerializeField] private Transform contentRoot;

        private readonly Dictionary<GridPosition, CellView> cellViews = new();
        private readonly List<GameObject> spawnedContentObjects = new();
        private readonly HashSet<GridPosition> moveHighlightedCells = new();
        private readonly HashSet<GridPosition> abilityHighlightedCells = new();
        private readonly HashSet<GridPosition> enemyDangerHighlightedCells = new();

        private PlayFieldConfig playFieldConfig;
        private Sprite defaultCellSprite;
        private int currentColumns;
        private int currentRows;
        private Color moveHighlightColor = new(0.4f, 0.7f, 1f, 1f);
        private Color abilityHighlightColor = new(0.95f, 0.53f, 0.25f, 1f);
        private Color enemyDangerHighlightColor = new(0.92f, 0.35f, 0.3f, 0.68f);

        private void Update()
        {
            if (cellViews.Count == 0)
            {
                return;
            }

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

            cellViews.Clear();
            moveHighlightedCells.Clear();
            abilityHighlightedCells.Clear();
            enemyDangerHighlightedCells.Clear();
            ClearContent();
            currentColumns = 0;
            currentRows = 0;
        }

        public void SetMoveHighlights(IReadOnlyList<GridPosition> positions)
        {
            moveHighlightedCells.Clear();
            if (positions == null)
            {
                ApplyHighlights();
                return;
            }

            for (var i = 0; i < positions.Count; i++)
            {
                moveHighlightedCells.Add(positions[i]);
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
            abilityHighlightedCells.Clear();
            if (positions == null)
            {
                ApplyHighlights();
                return;
            }

            for (var i = 0; i < positions.Count; i++)
            {
                abilityHighlightedCells.Add(positions[i]);
            }

            ApplyHighlights();
        }

        public void ClearAbilityHighlights()
        {
            if (abilityHighlightedCells.Count == 0)
            {
                return;
            }

            abilityHighlightedCells.Clear();
            ApplyHighlights();
        }

        public void SetEnemyDangerHighlights(IReadOnlyList<GridPosition> positions)
        {
            enemyDangerHighlightedCells.Clear();
            if (positions == null)
            {
                ApplyHighlights();
                return;
            }

            for (var i = 0; i < positions.Count; i++)
            {
                enemyDangerHighlightedCells.Add(positions[i]);
            }

            ApplyHighlights();
        }

        public void ClearEnemyDangerHighlights()
        {
            if (enemyDangerHighlightedCells.Count == 0)
            {
                return;
            }

            enemyDangerHighlightedCells.Clear();
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

            for (var x = 0; x < columns; x++)
            {
                for (var y = 0; y < rows; y++)
                {
                    var position = new GridPosition(x, y);
                    var cellObject = new GameObject($"Cell_{x}_{y}", typeof(CellView));
                    cellObject.transform.SetParent(cellRoot, false);
                    cellObject.transform.localPosition = GetCellLocalPosition(x, y, columns, rows);

                    var cellView = cellObject.GetComponent<CellView>();
                    cellView.Initialize(position, playFieldConfig.CellWorldSize, defaultCellSprite);
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
                spriteRenderer.sprite = defaultCellSprite;
                spriteRenderer.color = playFieldConfig.ObstacleCellColor;
                spriteRenderer.sortingOrder = 5;
                obstacleObject.transform.localScale = Vector3.one * (playFieldConfig.CellWorldSize * 0.52f);
            }

            var treasures = model.Treasures;
            for (var i = 0; i < treasures.Count; i++)
            {
                var treasure = treasures[i];
                var treasureObject = CreateContentObject($"Treasure_{treasure.Position.X}_{treasure.Position.Y}", treasure.Position);
                var treasureView = treasureObject.AddComponent<TreasureView>();
                treasureView.Initialize(treasure, playFieldConfig.CellWorldSize, playFieldConfig.TreasureCellColor);
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

            if (contentRoot == null)
            {
                var contentRootObject = new GameObject("ContentRoot");
                contentRoot = contentRootObject.transform;
                contentRoot.SetParent(transform, false);
            }
        }

        private void OnCellClicked(GridPosition position)
        {
            CellTapped?.Invoke(position);
        }

        private void ApplyHighlights()
        {
            foreach (var pair in cellViews)
            {
                var position = pair.Key;
                if (abilityHighlightedCells.Contains(position))
                {
                    pair.Value.SetHighlight(true, abilityHighlightColor);
                    continue;
                }

                if (moveHighlightedCells.Contains(position))
                {
                    pair.Value.SetHighlight(true, moveHighlightColor);
                    continue;
                }

                if (enemyDangerHighlightedCells.Contains(position))
                {
                    pair.Value.SetHighlight(true, enemyDangerHighlightColor);
                    continue;
                }

                pair.Value.SetHighlight(false, Color.white);
            }
        }

        private Color GetCellColor(CellContentType contentType)
        {
            return contentType switch
            {
                CellContentType.Player => playFieldConfig.PlayerCellColor,
                CellContentType.Enemy => playFieldConfig.EnemyCellColor,
                CellContentType.Treasure => playFieldConfig.TreasureCellColor,
                CellContentType.Obstacle => playFieldConfig.ObstacleCellColor,
                _ => playFieldConfig.EmptyCellColor,
            };
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
