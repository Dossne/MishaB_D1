using System;
using System.Collections.Generic;
using TetrisTactic.Units;

namespace TetrisTactic.Progression
{
    public sealed class LevelDefinition
    {
        private readonly List<UnitType> enemyRoster;

        public LevelDefinition(
            int levelIndex,
            int enemyCount,
            int enemyDamageBonus,
            int enemyHpBonus,
            List<UnitType> roster)
        {
            LevelIndex = levelIndex;
            EnemyCount = enemyCount;
            EnemyDamageBonus = enemyDamageBonus;
            EnemyHpBonus = enemyHpBonus;
            enemyRoster = roster ?? new List<UnitType>();
        }

        public int LevelIndex { get; }
        public int EnemyCount { get; }
        public int EnemyDamageBonus { get; }
        public int EnemyHpBonus { get; }
        public IReadOnlyList<UnitType> EnemyRoster => enemyRoster;

        public UnitType GetRandomEnemyType(Random random)
        {
            if (enemyRoster.Count == 0)
            {
                return UnitType.Warrior;
            }

            var randomIndex = random.Next(0, enemyRoster.Count);
            return enemyRoster[randomIndex];
        }
    }
}
