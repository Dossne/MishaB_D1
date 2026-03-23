using TetrisTactic.Abilities;

namespace TetrisTactic.Units
{
    public sealed class UnitData
    {
        public UnitData(UnitType unitType, TeamType teamType, int baseDamage, int maxHp, AbilityDefinition abilityDefinition = null)
        {
            UnitType = unitType;
            TeamType = teamType;
            BaseDamage = baseDamage;
            MaxHp = maxHp;
            AbilityDefinition = abilityDefinition;
        }

        public UnitType UnitType { get; }
        public TeamType TeamType { get; }
        public int BaseDamage { get; }
        public int MaxHp { get; }
        public AbilityDefinition AbilityDefinition { get; }
    }
}
