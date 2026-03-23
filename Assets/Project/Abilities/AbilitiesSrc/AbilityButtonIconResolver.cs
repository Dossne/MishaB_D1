using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TetrisTactic.Abilities
{
    public static class AbilityButtonIconResolver
    {
        public static Sprite Resolve(AbilityConfig abilityConfig, AbilityDefinitionId definitionId)
        {
            var configured = abilityConfig?.GetButtonIcon(definitionId);
            if (configured != null)
            {
                return configured;
            }

#if UNITY_EDITOR
            var assetPath = GetEditorAssetPath(definitionId);
            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            for (var i = 0; i < assets.Length; i++)
            {
                if (assets[i] is Sprite sprite)
                {
                    return sprite;
                }
            }
#endif

            return null;
        }

#if UNITY_EDITOR
        private static string GetEditorAssetPath(AbilityDefinitionId definitionId)
        {
            return definitionId switch
            {
                AbilityDefinitionId.OLeft => "Assets/Project/Abilities/AbilitiesArt/O_L.png",
                AbilityDefinitionId.ORight => "Assets/Project/Abilities/AbilitiesArt/O_R.png",
                AbilityDefinitionId.T => "Assets/Project/Abilities/AbilitiesArt/T.png",
                AbilityDefinitionId.LLeft => "Assets/Project/Abilities/AbilitiesArt/L_L.png",
                AbilityDefinitionId.LRight => "Assets/Project/Abilities/AbilitiesArt/L_R.png",
                AbilityDefinitionId.S => "Assets/Project/Abilities/AbilitiesArt/S.png",
                AbilityDefinitionId.I => "Assets/Project/Abilities/AbilitiesArt/I.png",
                _ => null,
            };
        }
#endif
    }
}
