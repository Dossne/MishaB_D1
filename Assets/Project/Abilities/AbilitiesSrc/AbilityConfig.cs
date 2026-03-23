using System.Collections.Generic;
using UnityEngine;

namespace TetrisTactic.Abilities
{
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

        public static AbilityConfig CreateDefault()
        {
            return CreateInstance<AbilityConfig>();
        }
    }
}
