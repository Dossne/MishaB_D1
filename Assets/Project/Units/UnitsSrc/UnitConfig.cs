using System;
using UnityEngine;

namespace TetrisTactic.Units
{
    [CreateAssetMenu(menuName = "Project/Units/Unit Config", fileName = "UnitConfig")]
    public sealed class UnitConfig : ScriptableObject
    {
        [Header("Generation")]
        [SerializeField, Min(1)] private int minEnemyCount = 2;
        [SerializeField, Min(1)] private int maxEnemyCount = 3;
        [SerializeField, Min(0)] private int minObstacleCount = 4;
        [SerializeField, Min(0)] private int maxObstacleCount = 6;

        [Header("Player")]
        [SerializeField, Min(1)] private int playerBaseDamage = 1;
        [SerializeField, Min(1)] private int playerBaseHp = 1;

        [Header("Warrior")]
        [SerializeField, Min(1)] private int warriorBaseDamage = 3;
        [SerializeField, Min(1)] private int warriorBaseHp = 2;

        [Header("Archer")]
        [SerializeField, Min(1)] private int archerBaseDamage = 2;
        [SerializeField, Min(1)] private int archerBaseHp = 1;

        [Header("Mage")]
        [SerializeField, Min(1)] private int mageBaseDamage = 2;
        [SerializeField, Min(1)] private int mageBaseHp = 3;

        public int GetEnemyCount(System.Random random, int maxAvailable)
        {
            return GetCountWithinBounds(random, minEnemyCount, maxEnemyCount, maxAvailable);
        }

        public int GetObstacleCount(System.Random random, int maxAvailable)
        {
            return GetCountWithinBounds(random, minObstacleCount, maxObstacleCount, maxAvailable);
        }

        public UnitData CreateUnitData(UnitType unitType)
        {
            return CreateUnitData(unitType, 0, 0);
        }

        public UnitData CreateUnitData(UnitType unitType, int damageBonus, int hpBonus)
        {
            var safeDamageBonus = Mathf.Max(0, damageBonus);
            var safeHpBonus = Mathf.Max(0, hpBonus);

            return unitType switch
            {
                UnitType.Player => new UnitData(
                    UnitType.Player,
                    TeamType.Player,
                    playerBaseDamage + safeDamageBonus,
                    playerBaseHp + safeHpBonus),
                UnitType.Warrior => new UnitData(
                    UnitType.Warrior,
                    TeamType.Enemy,
                    warriorBaseDamage + safeDamageBonus,
                    warriorBaseHp + safeHpBonus),
                UnitType.Archer => new UnitData(
                    UnitType.Archer,
                    TeamType.Enemy,
                    archerBaseDamage + safeDamageBonus,
                    archerBaseHp + safeHpBonus),
                UnitType.Mage => new UnitData(
                    UnitType.Mage,
                    TeamType.Enemy,
                    mageBaseDamage + safeDamageBonus,
                    mageBaseHp + safeHpBonus),
                _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, "Unsupported unit type."),
            };
        }

        public static UnitConfig CreateDefault()
        {
            return CreateInstance<UnitConfig>();
        }

        private static int GetCountWithinBounds(System.Random random, int min, int max, int maxAvailable)
        {
            if (maxAvailable <= 0)
            {
                return 0;
            }

            var safeMin = Mathf.Max(0, min);
            var safeMax = Mathf.Max(safeMin, max);
            var clampedMin = Mathf.Min(safeMin, maxAvailable);
            var clampedMax = Mathf.Min(safeMax, maxAvailable);
            if (clampedMax <= clampedMin)
            {
                return clampedMin;
            }

            return random.Next(clampedMin, clampedMax + 1);
        }
    }
}
