using System.Collections.Generic;
using TetrisTactic.Units;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TetrisTactic.Abilities
{
    public static class AbilityIconResolver
    {
        private static readonly Dictionary<UnitType, Sprite> Cache = new();

        public static Sprite GetAbilityIcon(UnitType unitType)
        {
            if (Cache.TryGetValue(unitType, out var cached) && cached != null)
            {
                return cached;
            }

            var fileName = unitType switch
            {
                UnitType.Player => "player_ability",
                UnitType.Warrior => "warrior_ability",
                UnitType.Archer => "archer_ability",
                UnitType.Mage => "mage_ability",
                _ => string.Empty,
            };

            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            var sprite = Resources.Load<Sprite>($"Project/Abilities/AbilitiesArt/{fileName}");

#if UNITY_EDITOR
            if (sprite == null)
            {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Project/Abilities/AbilitiesArt/{fileName}.png");
            }
#endif

            Cache[unitType] = sprite;
            return sprite;
        }
    }
}
