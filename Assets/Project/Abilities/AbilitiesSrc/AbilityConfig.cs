using System.Collections.Generic;
using TetrisTactic.Units;
using UnityEngine;

namespace TetrisTactic.Abilities
{
    [System.Serializable]
    public sealed class AbilityButtonIconEntry
    {
        public AbilityDefinitionId definitionId;
        public Sprite icon;
    }

    [System.Serializable]
    public sealed class UnitAbilityIconEntry
    {
        public UnitType unitType;
        public Sprite icon;
    }

    [CreateAssetMenu(menuName = "Project/Abilities/Ability Config", fileName = "AbilityConfig")]
    public sealed class AbilityConfig : ScriptableObject
    {
        [SerializeField] private List<AbilityDefinitionId> startingAbilities = new()
        {
            AbilityDefinitionId.OLeft,
            AbilityDefinitionId.T,
            AbilityDefinitionId.LLeft,
        };

        [SerializeField] private List<AbilityDefinitionId> randomGainPool = new()
        {
            AbilityDefinitionId.OLeft,
            AbilityDefinitionId.ORight,
            AbilityDefinitionId.T,
            AbilityDefinitionId.LLeft,
            AbilityDefinitionId.LRight,
            AbilityDefinitionId.S,
            AbilityDefinitionId.I,
        };

        [SerializeField] private List<AbilityButtonIconEntry> buttonIcons = new();
        [SerializeField] private List<UnitAbilityIconEntry> unitAbilityIcons = new();
        [SerializeField] private Sprite damageIcon;
        [SerializeField] private Sprite healthIcon;
        [SerializeField] private Sprite resourceIcon;
        [SerializeField, Min(1)] private int maxAvailableAbilities = 3;
        [SerializeField, Min(0)] private int waitAbilityGainThreshold = 2;
        [SerializeField, Min(0.01f)] private float waveCellDelay = 0.08f;
        [SerializeField, Min(0.01f)] private float impactDuration = 0.12f;
        [SerializeField] private bool enableAbilityQueueDebugLogs;

        public IReadOnlyList<AbilityDefinitionId> StartingAbilities => startingAbilities;
        public IReadOnlyList<AbilityDefinitionId> RandomGainPool => randomGainPool;
        public int MaxAvailableAbilities => Mathf.Max(1, maxAvailableAbilities);
        public int WaitAbilityGainThreshold => Mathf.Max(0, waitAbilityGainThreshold);
        public float WaveCellDelay => Mathf.Max(0.01f, waveCellDelay);
        public float ImpactDuration => Mathf.Max(0.01f, impactDuration);
        public bool EnableAbilityQueueDebugLogs => enableAbilityQueueDebugLogs;

        public Sprite GetButtonIcon(AbilityDefinitionId definitionId)
        {
            for (var i = 0; i < buttonIcons.Count; i++)
            {
                var entry = buttonIcons[i];
                if (entry != null && entry.definitionId == definitionId)
                {
                    return entry.icon;
                }
            }

            return null;
        }

        public Sprite GetUnitAbilityIcon(UnitType unitType)
        {
            for (var i = 0; i < unitAbilityIcons.Count; i++)
            {
                var entry = unitAbilityIcons[i];
                if (entry != null && entry.unitType == unitType)
                {
                    return entry.icon;
                }
            }

            return null;
        }

        public Sprite GetDamageIcon()
        {
            return damageIcon;
        }

        public Sprite GetHealthIcon()
        {
            return healthIcon;
        }

        public Sprite GetResourceIcon()
        {
            return resourceIcon;
        }

        public static AbilityConfig CreateDefault()
        {
            return CreateInstance<AbilityConfig>();
        }
    }
}

