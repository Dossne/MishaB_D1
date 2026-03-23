using System.Collections.Generic;
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

        private readonly Dictionary<GridPosition, CellView> cellViews = new();

        private PlayFieldConfig playFieldConfig;
        private Sprite defaultCellSprite;
        private int currentColumns;
        private int currentRows;

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
            EnsureRoot();

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

            EnsureRoot();

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
                        view.SetColor(GetCellColor(model.GetCell(position)));
                    }
                }
            }
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
            currentColumns = 0;
            currentRows = 0;
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

        private Vector3 GetCellLocalPosition(int x, int y, int columns, int rows)
        {
            var centerOffsetX = (columns - 1) * 0.5f;
            var centerOffsetY = (rows - 1) * 0.5f;

            return new Vector3(
                (x - centerOffsetX) * playFieldConfig.CellWorldSize,
                (centerOffsetY - y) * playFieldConfig.CellWorldSize,
                0f);
        }

        private void EnsureRoot()
        {
            if (cellRoot != null)
            {
                return;
            }

            var rootObject = new GameObject("CellRoot");
            cellRoot = rootObject.transform;
            cellRoot.SetParent(transform, false);
        }

        private void OnCellClicked(GridPosition position)
        {
            CellTapped?.Invoke(position);
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
