using System;
using System.Collections.Generic;
using TetrisTactic.Abilities;
using UnityEngine;

namespace TetrisTactic.Units
{
    [System.Serializable]
    public sealed class UnitSpriteEntry
    {
        public UnitType unitType;
        public Sprite sprite;
    }

    [CreateAssetMenu(menuName = "Project/Units/Unit Config", fileName = "UnitConfig")]
    public sealed class UnitConfig : ScriptableObject
    {
        [Header("Generation")]
        [SerializeField, Min(1)] private int minEnemyCount = 2;
        [SerializeField, Min(1)] private int maxEnemyCount = 3;
        [SerializeField, Min(0)] private int minObstacleCount = 4;
        [SerializeField, Min(0)] private int maxObstacleCount = 6;

        [Header("Art")]
        [SerializeField] private List<UnitSpriteEntry> unitSprites = new();

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

        [Header("Enemy Turn")]
        [SerializeField, Min(0f)] private float enemyMinPreActionDelay = 0.5f;
        [SerializeField, Min(0f)] private float enemyMaxPreActionDelay = 1f;
        [SerializeField, Min(0f)] private float enemyPostActionDelay = 0.25f;
        [SerializeField, Min(0.01f)] private float enemyWaveCellDelay = 0.09f;
        [SerializeField, Min(0.01f)] private float enemyImpactDuration = 0.12f;
        [SerializeField, Min(0)] private int enemyWaitHealAmount = 1;

        public int GetEnemyCount(System.Random random, int maxAvailable)
        {
            return GetCountWithinBounds(random, minEnemyCount, maxEnemyCount, maxAvailable);
        }

        public int GetObstacleCount(System.Random random, int maxAvailable)
        {
            return GetCountWithinBounds(random, minObstacleCount, maxObstacleCount, maxAvailable);
        }

        public float EnemyMinPreActionDelay => Mathf.Max(0f, enemyMinPreActionDelay);
        public float EnemyMaxPreActionDelay => Mathf.Max(EnemyMinPreActionDelay, enemyMaxPreActionDelay);
        public float EnemyPostActionDelay => Mathf.Max(0f, enemyPostActionDelay);
        public float EnemyWaveCellDelay => Mathf.Max(0.01f, enemyWaveCellDelay);
        public float EnemyImpactDuration => Mathf.Max(0.01f, enemyImpactDuration);
        public int EnemyWaitHealAmount => Mathf.Max(0, enemyWaitHealAmount);

        public Sprite GetUnitSprite(UnitType unitType)
        {
            for (var i = 0; i < unitSprites.Count; i++)
            {
                var entry = unitSprites[i];
                if (entry != null && entry.unitType == unitType)
                {
                    return entry.sprite;
                }
            }

            return null;
        }

        public UnitData CreateUnitData(UnitType unitType)
        {
            return CreateUnitData(unitType, 0, 0);
        }

        public UnitData CreateUnitData(UnitType unitType, int damageBonus, int hpBonus)
        {
            return CreateUnitData(unitType, damageBonus, hpBonus, null);
        }

        public UnitData CreateUnitData(UnitType unitType, int damageBonus, int hpBonus, AbilityDefinition abilityDefinition)
        {
            var safeDamageBonus = Mathf.Max(0, damageBonus);
            var safeHpBonus = Mathf.Max(0, hpBonus);

            return unitType switch
            {
                UnitType.Player => new UnitData(
                    UnitType.Player,
                    TeamType.Player,
                    playerBaseDamage + safeDamageBonus,
                    playerBaseHp + safeHpBonus,
                    abilityDefinition),
                UnitType.Warrior => new UnitData(
                    UnitType.Warrior,
                    TeamType.Enemy,
                    warriorBaseDamage + safeDamageBonus,
                    warriorBaseHp + safeHpBonus,
                    abilityDefinition),
                UnitType.Archer => new UnitData(
                    UnitType.Archer,
                    TeamType.Enemy,
                    archerBaseDamage + safeDamageBonus,
                    archerBaseHp + safeHpBonus,
                    abilityDefinition),
                UnitType.Mage => new UnitData(
                    UnitType.Mage,
                    TeamType.Enemy,
                    mageBaseDamage + safeDamageBonus,
                    mageBaseHp + safeHpBonus,
                    abilityDefinition),
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

