using TetrisTactic.PlayField;
namespace TetrisTactic.Treasure
{
    public sealed class TreasureData
    {
        public TreasureData(GridPosition position, int resourceAmount)
        {
            Position = position;
            ResourceAmount = resourceAmount;
        }
        public GridPosition Position { get; }
        public int ResourceAmount { get; }
    }
}
