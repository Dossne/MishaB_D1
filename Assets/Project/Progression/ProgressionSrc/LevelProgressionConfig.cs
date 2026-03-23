using System.Collections.Generic;
using TetrisTactic.Units;
using UnityEngine;

namespace TetrisTactic.Progression
{
    [CreateAssetMenu(menuName = "Project/Progression/Level Progression Config", fileName = "LevelProgressionConfig")]
    public sealed class LevelProgressionConfig : ScriptableObject
    {
        [Header("Resources")]
        [SerializeField, Min(1)] private int upgradeCost = 1;
        [SerializeField, Min(0)] private int victoryBonusResource = 1;

        [Header("Enemy Count")]
        [SerializeField, Min(1)] private int minEnemyCount = 1;
        [SerializeField, Min(1)] private int maxEnemyCount = 5;

        [Header("Enemy Scaling")]
        [SerializeField, Min(0)] private int enemyDamagePerLevel = 1;
        [SerializeField, Min(0)] private int enemyHpPerLevel = 1;

        public int UpgradeCost => Mathf.Max(1, upgradeCost);
        public int VictoryBonusResource => Mathf.Max(0, victoryBonusResource);

        public LevelDefinition BuildLevelDefinition(int level)
        {
            var safeLevel = Mathf.Max(1, level);
            var enemyCount = Mathf.Clamp(minEnemyCount + safeLevel - 1, minEnemyCount, maxEnemyCount);
            var scalingLevel = safeLevel - 1;
            var enemyDamageBonus = scalingLevel * enemyDamagePerLevel;
            var enemyHpBonus = scalingLevel * enemyHpPerLevel;

            var roster = BuildEnemyRoster(safeLevel);
            return new LevelDefinition(safeLevel, enemyCount, enemyDamageBonus, enemyHpBonus, roster);
        }

        public static LevelProgressionConfig CreateDefault()
        {
            return CreateInstance<LevelProgressionConfig>();
        }

        private static List<UnitType> BuildEnemyRoster(int level)
        {
            var roster = new List<UnitType> { UnitType.Warrior };

            if (level >= 2)
            {
                roster.Add(UnitType.Archer);
            }

            if (level >= 3)
            {
                roster.Add(UnitType.Mage);
            }

            return roster;
        }
    }
}
