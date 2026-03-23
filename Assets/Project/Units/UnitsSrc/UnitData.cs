namespace TetrisTactic.Units
{
    public sealed class UnitData
    {
        public UnitData(UnitType unitType, TeamType teamType, int baseDamage, int maxHp)
        {
            UnitType = unitType;
            TeamType = teamType;
            BaseDamage = baseDamage;
            MaxHp = maxHp;
        }
        public UnitType UnitType { get; }
        public TeamType TeamType { get; }
        public int BaseDamage { get; }
        public int MaxHp { get; }
    }
}
