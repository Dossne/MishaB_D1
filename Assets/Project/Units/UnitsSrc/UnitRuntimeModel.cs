using TetrisTactic.Abilities;
using TetrisTactic.PlayField;

namespace TetrisTactic.Units
{
    public sealed class UnitRuntimeModel
    {
        public UnitRuntimeModel(UnitData data, GridPosition position)
        {
            Data = data;
            Position = position;
            Health = new HealthComponent(data.MaxHp);
        }

        public UnitData Data { get; }
        public GridPosition Position { get; set; }
        public HealthComponent Health { get; }
        public UnitType UnitType => Data.UnitType;
        public TeamType TeamType => Data.TeamType;
        public int BaseDamage => Data.BaseDamage;
        public AbilityDefinition AbilityDefinition => Data.AbilityDefinition;
    }
}
