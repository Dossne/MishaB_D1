namespace TetrisTactic.PlayField
{
    public enum CellContentType
    {
        Empty = 0,
        Player = 1,
        Enemy = 2,
        Treasure = 3,
        Obstacle = 4,
    }

    public sealed class PlayFieldModel
    {
        private readonly CellContentType[,] cells;

        public PlayFieldModel(int columns, int rows)
        {
            Columns = columns;
            Rows = rows;
            cells = new CellContentType[columns, rows];
        }

        public int Columns { get; }
        public int Rows { get; }

        public bool IsInside(GridPosition position)
        {
            return position.X >= 0 && position.X < Columns && position.Y >= 0 && position.Y < Rows;
        }

        public CellContentType GetCell(GridPosition position)
        {
            if (!IsInside(position))
            {
                return CellContentType.Empty;
            }

            return cells[position.X, position.Y];
        }

        public void SetCell(GridPosition position, CellContentType content)
        {
            if (!IsInside(position))
            {
                return;
            }

            cells[position.X, position.Y] = content;
        }

        public void ClearAll()
        {
            for (var x = 0; x < Columns; x++)
            {
                for (var y = 0; y < Rows; y++)
                {
                    cells[x, y] = CellContentType.Empty;
                }
            }
        }
    }
}
