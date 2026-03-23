using TetrisTactic.Units;

namespace TetrisTactic.PlayField
{
    public sealed class DamageEventData
    {
        public DamageEventData(UnitRuntimeModel sourceUnit, UnitRuntimeModel targetUnit, GridPosition targetPosition, int damageAmount, bool wasFatal)
        {
            SourceUnit = sourceUnit;
            TargetUnit = targetUnit;
            TargetPosition = targetPosition;
            DamageAmount = damageAmount;
            WasFatal = wasFatal;
        }

        public UnitRuntimeModel SourceUnit { get; }
        public UnitRuntimeModel TargetUnit { get; }
        public GridPosition TargetPosition { get; }
        public int DamageAmount { get; }
        public bool WasFatal { get; }
    }
}
