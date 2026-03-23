using System;
using TetrisTactic.PlayField;

namespace TetrisTactic.Units
{
    public sealed class UnitFactory
    {
        private static readonly UnitType[] EnemyTypes =
        {
            UnitType.Warrior,
            UnitType.Archer,
            UnitType.Mage,
        };

        private readonly UnitConfig unitConfig;

        public UnitFactory(UnitConfig unitConfig)
        {
            this.unitConfig = unitConfig;
        }

        public UnitRuntimeModel CreatePlayer(GridPosition position)
        {
            return CreatePlayer(position, 0, 0);
        }

        public UnitRuntimeModel CreatePlayer(GridPosition position, int damageBonus, int hpBonus)
        {
            return new UnitRuntimeModel(unitConfig.CreateUnitData(UnitType.Player, damageBonus, hpBonus), position);
        }

        public UnitRuntimeModel CreateEnemy(UnitType enemyType, GridPosition position)
        {
            return CreateEnemy(enemyType, position, 0, 0);
        }

        public UnitRuntimeModel CreateEnemy(UnitType enemyType, GridPosition position, int damageBonus, int hpBonus)
        {
            return new UnitRuntimeModel(unitConfig.CreateUnitData(enemyType, damageBonus, hpBonus), position);
        }

        public UnitType GetRandomEnemyType(Random random)
        {
            var index = random.Next(0, EnemyTypes.Length);
            return EnemyTypes[index];
        }
    }
}
