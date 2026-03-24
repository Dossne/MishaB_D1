using UnityEngine;

namespace TetrisTactic.PlayField
{
    [CreateAssetMenu(menuName = "Project/PlayField/PlayField Config", fileName = "PlayFieldConfig")]
    public sealed class PlayFieldConfig : ScriptableObject
    {
        [SerializeField, Min(1)] private int columns = 6;
        [SerializeField, Min(1)] private int rows = 8;
        [SerializeField, Min(0.1f)] private float cellWorldSize = 1f;
        [SerializeField] private Vector2 boardWorldOffset = new Vector2(0f, 0.9f);

        [Header("Art")]
        [SerializeField] private Sprite grassSprite;
        [SerializeField] private Sprite obstacleSprite;
        [SerializeField] private Sprite treasureSprite;
        [SerializeField] private Sprite movePreviewSprite;

        [Header("Cell Colors")]
        [SerializeField] private Color emptyCellColor = new Color(0.2f, 0.24f, 0.3f, 1f);
        [SerializeField] private Color playerCellColor = new Color(0.22f, 0.78f, 0.32f, 1f);
        [SerializeField] private Color enemyCellColor = new Color(0.86f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color treasureCellColor = new Color(1f, 0.82f, 0.18f, 1f);
        [SerializeField] private Color obstacleCellColor = new Color(0.43f, 0.45f, 0.5f, 1f);

        public int Columns => Mathf.Max(1, columns);
        public int Rows => Mathf.Max(1, rows);
        public float CellWorldSize => Mathf.Max(0.1f, cellWorldSize);
        public Vector2 BoardWorldOffset => boardWorldOffset;
        public Sprite GrassSprite => grassSprite;
        public Sprite ObstacleSprite => obstacleSprite;
        public Sprite TreasureSprite => treasureSprite;
        public Sprite MovePreviewSprite => movePreviewSprite;
        public Color EmptyCellColor => emptyCellColor;
        public Color PlayerCellColor => playerCellColor;
        public Color EnemyCellColor => enemyCellColor;
        public Color TreasureCellColor => treasureCellColor;
        public Color ObstacleCellColor => obstacleCellColor;

        public static PlayFieldConfig CreateDefault()
        {
            return CreateInstance<PlayFieldConfig>();
        }
    }
}

