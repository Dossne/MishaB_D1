using System.Collections.Generic;
using UnityEngine;

namespace TetrisTactic.Abilities
{
    [CreateAssetMenu(menuName = "Project/Abilities/Ability Config", fileName = "AbilityConfig")]
    public sealed class AbilityConfig : ScriptableObject
    {
        [SerializeField] private List<AbilityShapeType> startingAbilities = new()
        {
            AbilityShapeType.O,
            AbilityShapeType.T,
            AbilityShapeType.L,
        };

        [SerializeField, Min(0.01f)] private float waveCellDelay = 0.08f;
        [SerializeField, Min(0.01f)] private float impactDuration = 0.12f;

        public IReadOnlyList<AbilityShapeType> StartingAbilities => startingAbilities;
        public float WaveCellDelay => Mathf.Max(0.01f, waveCellDelay);
        public float ImpactDuration => Mathf.Max(0.01f, impactDuration);

        public static AbilityConfig CreateDefault()
        {
            return CreateInstance<AbilityConfig>();
        }
    }
}