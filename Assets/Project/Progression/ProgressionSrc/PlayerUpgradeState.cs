namespace TetrisTactic.Progression
{
    public sealed class PlayerUpgradeState
    {
        public PlayerUpgradeState(int damageBonus, int hpBonus)
        {
            DamageBonus = damageBonus < 0 ? 0 : damageBonus;
            HpBonus = hpBonus < 0 ? 0 : hpBonus;
        }

        public int DamageBonus { get; }
        public int HpBonus { get; }

        public PlayerUpgradeState Apply(PlayerUpgradeType upgradeType)
        {
            return upgradeType switch
            {
                PlayerUpgradeType.Damage => new PlayerUpgradeState(DamageBonus + 1, HpBonus),
                PlayerUpgradeType.Health => new PlayerUpgradeState(DamageBonus, HpBonus + 1),
                _ => this,
            };
        }

        public static PlayerUpgradeState CreateDefault()
        {
            return new PlayerUpgradeState(0, 0);
        }
    }
}
