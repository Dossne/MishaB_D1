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
        private static readonly Dictionary<UnitType, Sprite> UnitAbilityIconCache = new();
        private static AbilityConfig abilityConfig;

        public static void SetAbilityConfig(AbilityConfig config)
        {
            if (abilityConfig == config)
            {
                return;
            }

            abilityConfig = config;
            UnitAbilityIconCache.Clear();
        }

        public static Sprite GetAbilityIcon(UnitType unitType)
        {
            if (UnitAbilityIconCache.TryGetValue(unitType, out var cached) && cached != null)
            {
                return cached;
            }

            var configured = abilityConfig?.GetUnitAbilityIcon(unitType);
            if (configured != null)
            {
                UnitAbilityIconCache[unitType] = configured;
                return configured;
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

            UnitAbilityIconCache[unitType] = sprite;
            return sprite;
        }

        public static Sprite GetDamageIcon()
        {
            var configured = abilityConfig?.GetDamageIcon();
            if (configured != null)
            {
                return configured;
            }

            return LoadFallbackSprite("damage", "Assets/Project/Abilities/AbilitiesArt/damage.png");
        }

        public static Sprite GetHealthIcon()
        {
            var configured = abilityConfig?.GetHealthIcon();
            if (configured != null)
            {
                return configured;
            }

            return LoadFallbackSprite("health", "Assets/Project/Abilities/AbilitiesArt/health.png");
        }

        public static Sprite GetResourceIcon()
        {
            var configured = abilityConfig?.GetResourceIcon();
            if (configured != null)
            {
                return configured;
            }

            return LoadFallbackSprite("resource", "Assets/Project/Resource/ResourceArt/resource.png", "Project/Resource/ResourceArt");
        }

        private static Sprite LoadFallbackSprite(string fileName, string editorPath, string resourcesRoot = "Project/Abilities/AbilitiesArt")
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            var sprite = Resources.Load<Sprite>($"{resourcesRoot}/{fileName}");

#if UNITY_EDITOR
            if (sprite == null && !string.IsNullOrWhiteSpace(editorPath))
            {
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(editorPath);
            }
#endif

            return sprite;
        }
    }
}

