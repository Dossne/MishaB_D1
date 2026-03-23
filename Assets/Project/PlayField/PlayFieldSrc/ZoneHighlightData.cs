using TetrisTactic.Units;

namespace TetrisTactic.PlayField
{
    public sealed class ZoneHighlightData
    {
        public ZoneHighlightData(GridPosition position, UnitType ownerUnitType, int priority)
        {
            Position = position;
            OwnerUnitType = ownerUnitType;
            Priority = priority;
        }

        public GridPosition Position { get; }
        public UnitType OwnerUnitType { get; }
        public int Priority { get; }
    }
}
